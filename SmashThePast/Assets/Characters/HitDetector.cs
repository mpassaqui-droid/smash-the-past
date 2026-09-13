using System;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Fighter))]
    public class HitDetector : MonoBehaviour
    {
        [SerializeField] private Fighter opponent;
        [SerializeField, Min(0)] private float lightRange = 1.8f;
        [SerializeField, Min(0)] private float heavyRange = 2.1f;
        [SerializeField, Min(0)] private float verticalRange = 1.2f;
        [SerializeField, Min(0)] private float grabRange = 1.35f;
        private Fighter fighter;
        private float nextAttack;

        public Fighter Opponent => opponent;
        public bool CombatEnabled { get; set; } = true;
        public event Action<bool> AttackPerformed;
        public event Action<int> HitLanded;
        /// <summary>true si la presa conecto, false si se fallo (y queda el castigo).</summary>
        public event Action<bool> GrabPerformed;
        public event Action<int> GrabLanded;

        private void Awake() => fighter = GetComponent<Fighter>();

        private void OnEnable()
        {
            fighter = GetComponent<Fighter>();
            fighter.OnLightAttackPerformed += Light;
            fighter.OnHeavyAttackPerformed += Heavy;
            fighter.OnGrabPerformed += Grab;
        }

        private void OnDisable()
        {
            // Al destruir el objeto, Unity puede quitar el Fighter antes que este
            // componente: sin la guarda, desuscribirse revienta.
            if (fighter == null) return;
            fighter.OnLightAttackPerformed -= Light;
            fighter.OnHeavyAttackPerformed -= Heavy;
            fighter.OnGrabPerformed -= Grab;
        }

        public void SetOpponent(Fighter target) => opponent = target;
        public void ResetAttack() => nextAttack = 0;
        private void Light() => Attack(false);
        private void Heavy() => Attack(true);

        private void Attack(bool heavy)
        {
            var combatant = GetComponent<Combatant>();
            if (!CombatEnabled || Time.timeScale <= 0 || fighter.IsBlocking
                || (combatant != null && !combatant.CanAct)
                || fighter.Health == null || fighter.Health.IsDead
                || opponent == null || opponent.Health == null || opponent.Health.IsDead
                || Time.time < nextAttack) return;

            nextAttack = Time.time + (heavy ? 0.55f : 0.3f);
            AttackPerformed?.Invoke(heavy);
            if (!IsInRange(transform.position, opponent.transform.position,
                    heavy ? heavyRange : lightRange, verticalRange)) return;

            var target = opponent.GetComponent<Combatant>();
            int raw = heavy ? fighter.HeavyAttackDamage : fighter.LightAttackDamage;
            int damage = target != null ? target.ReceiveDamage(raw) : opponent.ReceiveHit(raw);
            if (combatant != null) combatant.ChargeFromDamage(damage);
            else fighter.Special.Fill(damage);
            HitLanded?.Invoke(damage);
        }

        // La presa es corta y lenta. Si conecta atraviesa la guardia; si falla, el
        // atacante se queda vendido casi un segundo. Ese riesgo es lo que impide que
        // agarrar sea la respuesta a todo.
        private void Grab()
        {
            var combatant = GetComponent<Combatant>();
            if (!CombatEnabled || Time.timeScale <= 0 || fighter.IsBlocking
                || (combatant != null && !combatant.CanAct)
                || fighter.Health == null || fighter.Health.IsDead
                || opponent == null || opponent.Health == null || opponent.Health.IsDead
                || Time.time < nextAttack) return;

            bool conecta = IsInRange(transform.position, opponent.transform.position, grabRange, verticalRange);
            nextAttack = Time.time + (conecta ? 0.5f : 0.85f);
            GrabPerformed?.Invoke(conecta);
            if (!conecta) return;

            var target = opponent.GetComponent<Combatant>();
            int raw = fighter.GrabDamage;
            int damage;
            if (target != null) damage = target.ReceiveGrabDamage(raw);
            else
            {
                opponent.SetBlocking(false);
                opponent.ApplyStun(Fighter.GrabStunSeconds);
                damage = opponent.ReceiveHit(raw);
            }

            if (combatant != null) combatant.ChargeFromDamage(damage);
            else fighter.Special.Fill(damage);
            GrabLanded?.Invoke(damage);
        }

        public static bool IsInRange(Vector3 attacker, Vector3 defender, float reach, float height)
        {
            Vector3 offset = defender - attacker;
            return reach >= 0 && height >= 0 && Mathf.Abs(offset.y) <= height
                && new Vector2(offset.x, offset.z).sqrMagnitude <= reach * reach;
        }
    }
}
