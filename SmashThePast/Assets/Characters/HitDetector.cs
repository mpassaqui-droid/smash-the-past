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
        private Fighter fighter;
        private float nextAttack;

        public Fighter Opponent => opponent;
        public bool CombatEnabled { get; set; } = true;
        public event Action<bool> AttackPerformed;
        public event Action<int> HitLanded;

        private void Awake() => fighter = GetComponent<Fighter>();

        private void OnEnable()
        {
            fighter = GetComponent<Fighter>();
            fighter.OnLightAttackPerformed += Light;
            fighter.OnHeavyAttackPerformed += Heavy;
        }

        private void OnDisable()
        {
            fighter.OnLightAttackPerformed -= Light;
            fighter.OnHeavyAttackPerformed -= Heavy;
        }

        public void SetOpponent(Fighter target) => opponent = target;
        public void ResetAttack() => nextAttack = 0;
        private void Light() => Attack(false);
        private void Heavy() => Attack(true);

        private void Attack(bool heavy)
        {
            if (!CombatEnabled || fighter.Health == null || fighter.Health.IsDead
                || opponent == null || opponent.Health == null || opponent.Health.IsDead
                || Time.time < nextAttack) return;

            nextAttack = Time.time + (heavy ? 0.55f : 0.3f);
            AttackPerformed?.Invoke(heavy);
            if (!IsInRange(transform.position, opponent.transform.position,
                    heavy ? heavyRange : lightRange, verticalRange)) return;

            int before = opponent.Health.CurrentHealth;
            opponent.ReceiveHit(heavy ? fighter.HeavyAttackDamage : fighter.LightAttackDamage);
            int damage = before - opponent.Health.CurrentHealth;
            fighter.Special.Fill(damage);
            HitLanded?.Invoke(damage);
        }

        public static bool IsInRange(Vector3 attacker, Vector3 defender, float reach, float height)
        {
            Vector3 offset = defender - attacker;
            return reach >= 0 && height >= 0 && Mathf.Abs(offset.y) <= height
                && new Vector2(offset.x, offset.z).sqrMagnitude <= reach * reach;
        }
    }
}
