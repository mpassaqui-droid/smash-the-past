using System;
using UnityEngine;

namespace SmashThePast.Core
{
    /// <summary>
    /// Componente que conecta el input y la escena con la logica pura de combate.
    /// Es la MISMA clase para los 9+1 personajes; lo que cambia entre ellos vive en CharacterData.
    /// La resolucion real de hitbox/hurtbox (quien golpea a quien) es trabajo futuro,
    /// ver TASKS.md: aqui solo se exponen los eventos que un HitDetector futuro va a escuchar.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Fighter : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;

        public CharacterData Data => characterData;
        public HealthSystem Health { get; private set; }
        public SpecialMeter Special { get; private set; }
        public bool IsBlocking { get; private set; }

        public event Action OnLightAttackPerformed;
        public event Action OnHeavyAttackPerformed;
        public event Action OnSpecialUsed;
        public event Action OnKnockedOut;

        private CharacterController _controller;
        private Vector3 _velocity;
        private const float Gravity = -20f;
        private const float GroundedStickForce = -2f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            Health = new HealthSystem(characterData != null ? characterData.maxHealth : 100);
            Special = new SpecialMeter();
        }

        private void Update()
        {
            ApplyGravity();
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded && _velocity.y < 0)
                _velocity.y = GroundedStickForce;

            _velocity.y += Gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        public void Move(float horizontalInput)
        {
            if (IsBlocking) return; // regla fija: bloqueando no te mueves
            float speed = characterData != null ? characterData.moveSpeed : 5f;
            _controller.Move(new Vector3(horizontalInput, 0, 0) * speed * Time.deltaTime);
        }

        public void Jump()
        {
            if (!_controller.isGrounded || IsBlocking) return;
            _velocity.y = characterData != null ? characterData.jumpForce : 8f;
        }

        public void SetBlocking(bool blocking)
        {
            IsBlocking = blocking;
        }

        public void PerformLightAttack()
        {
            if (IsBlocking) return;
            OnLightAttackPerformed?.Invoke();
        }

        public void PerformHeavyAttack()
        {
            if (IsBlocking) return;
            OnHeavyAttackPerformed?.Invoke();
        }

        public void TryUseSpecial()
        {
            if (!Special.TryConsume()) return;
            OnSpecialUsed?.Invoke();
        }

        /// <summary>Llamado por el futuro HitDetector cuando este fighter recibe un golpe.</summary>
        public int ReceiveHit(int rawDamage)
        {
            int applied = Health.ApplyHit(rawDamage, IsBlocking);
            Special.Fill(Math.Max(applied, 5));

            if (Health.IsDead)
                OnKnockedOut?.Invoke();

            return applied;
        }

        public int LightAttackDamage => characterData != null ? characterData.lightAttackDamage : 5;
        public int HeavyAttackDamage => characterData != null ? characterData.heavyAttackDamage : 10;
    }
}
