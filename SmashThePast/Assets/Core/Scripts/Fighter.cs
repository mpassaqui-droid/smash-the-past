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
        // Creacion perezosa: en EditMode Unity no llama a Awake al anadir el componente,
        // y sin esto cualquier prueba sobre Fighter revienta con una referencia nula.
        private HealthSystem _health;
        private SpecialMeter _special;
        public HealthSystem Health => _health ??= new HealthSystem(characterData != null ? characterData.maxHealth : 100);
        public SpecialMeter Special => _special ??= new SpecialMeter();
        public bool IsBlocking { get; private set; }

        /// <summary>Segundos que quedan de aturdimiento. Mientras corre, este fighter no actua.</summary>
        public float StunRemaining { get; private set; }
        public bool IsStunned => StunRemaining > 0f;

        // Reglas fijas del aturdimiento. Lo que hace que un golpe tenga consecuencia:
        // el que encaja pierde el turno unas decimas, el que para lo pierde mucho menos.
        // La diferencia entre las dos es lo que premia acertar en vez de solo tocar.
        public const float BlockStunSeconds = 0.12f;
        public const float LightHitStunSeconds = 0.25f;
        public const float HeavyHitStunSeconds = 0.38f;
        private const int HeavyDamageThreshold = 10;

        public event Action OnLightAttackPerformed;
        public event Action OnHeavyAttackPerformed;
        public event Action OnSpecialUsed;
        public event Action OnKnockedOut;
        public event Action<float> OnStunned;

        private CharacterController _controller;
        private Vector3 _velocity;
        private const float Gravity = -20f;
        private const float GroundedStickForce = -2f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health ??= new HealthSystem(characterData != null ? characterData.maxHealth : 100);
            _special ??= new SpecialMeter();
        }

        private void Update()
        {
            if (StunRemaining > 0f) StunRemaining = Mathf.Max(0f, StunRemaining - Time.deltaTime);
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
            if (IsStunned) return; // aturdido no se mueve
            if (IsBlocking) return; // regla fija: bloqueando no te mueves
            float speed = characterData != null ? characterData.moveSpeed : 5f;
            _controller.Move(new Vector3(horizontalInput, 0, 0) * speed * Time.deltaTime);
        }

        public void Jump()
        {
            if (IsStunned) return;
            if (!_controller.isGrounded || IsBlocking) return;
            _velocity.y = characterData != null ? characterData.jumpForce : 8f;
        }

        public void SetBlocking(bool blocking)
        {
            // Aturdido no se puede levantar la guardia: si no, encajar un golpe seria
            // gratis y bastaria con mantener el boton para no perder nunca.
            if (IsStunned && blocking) return;
            IsBlocking = blocking;
        }

        public void PerformLightAttack()
        {
            if (IsStunned) return;
            if (IsBlocking) return;
            OnLightAttackPerformed?.Invoke();
        }

        public void PerformHeavyAttack()
        {
            if (IsStunned) return;
            if (IsBlocking) return;
            OnHeavyAttackPerformed?.Invoke();
        }

        public void TryUseSpecial()
        {
            if (IsStunned) return;
            if (!Special.TryConsume()) return;
            OnSpecialUsed?.Invoke();
        }

        /// <summary>Llamado por el futuro HitDetector cuando este fighter recibe un golpe.</summary>
        public int ReceiveHit(int rawDamage)
        {
            bool blocked = IsBlocking;
            int applied = Health.ApplyHit(rawDamage, blocked);
            Special.Fill(Math.Max(applied, 5));

            if (Health.IsDead)
            {
                OnKnockedOut?.Invoke();
                return applied;
            }

            float aturdimiento = blocked
                ? BlockStunSeconds
                : (rawDamage >= HeavyDamageThreshold ? HeavyHitStunSeconds : LightHitStunSeconds);
            ApplyStun(aturdimiento, blocked);

            return applied;
        }

        /// <summary>Aturde a este fighter. Gana el mayor de los dos: un golpe fuerte no
        /// se acorta porque justo antes llegara uno ligero.</summary>
        public void ApplyStun(float seconds, bool keepGuard = false)
        {
            if (seconds <= 0f) return;
            StunRemaining = Mathf.Max(StunRemaining, seconds);
            // Al parar se sigue en guardia durante el aturdimiento: si no, un segundo
            // golpe dentro de esas centesimas entraria limpio pese a estar bloqueando.
            if (!keepGuard) IsBlocking = false;
            OnStunned?.Invoke(StunRemaining);
        }

        public int LightAttackDamage => characterData != null ? characterData.lightAttackDamage : 5;
        public int HeavyAttackDamage => characterData != null ? characterData.heavyAttackDamage : 10;
    }
}
