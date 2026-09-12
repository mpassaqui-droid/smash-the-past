using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Fighter), typeof(HitDetector))]
    public class FighterVisual : MonoBehaviour
    {
        [SerializeField] private Transform model;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        private Fighter fighter;
        private HitDetector detector;
        private Combatant combatant;
        private Vector3 previousPosition;
        private float attackUntil;
        private bool heavyAttack;
        private int health;
        private float hitUntil;
        private Renderer[] renderers;
        private MaterialPropertyBlock properties;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            detector = GetComponent<HitDetector>();
            combatant = GetComponent<Combatant>();
            previousPosition = transform.position;
            renderers = GetComponentsInChildren<Renderer>();
            properties = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            detector = GetComponent<HitDetector>();
            detector.AttackPerformed += Attack;
        }

        private void OnDisable() => detector.AttackPerformed -= Attack;
        private void Attack(bool heavy) { heavyAttack = heavy; attackUntil = Time.time + 0.22f; }

        private void LateUpdate()
        {
            if (model == null || fighter.Health == null) return;
            if (health > fighter.Health.CurrentHealth) hitUntil = Time.time + 0.12f;
            health = fighter.Health.CurrentHealth;
            float direction = detector.Opponent == null ? 1 : Mathf.Sign(detector.Opponent.transform.position.x - transform.position.x);
            model.localRotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, fighter.Health.IsDead ? -80 : 0);
            float speed = Mathf.Abs(transform.position.x - previousPosition.x) / Mathf.Max(Time.deltaTime, 0.001f);
            previousPosition = transform.position;
            float stride = Mathf.Sin(Time.time * 13) * Mathf.Clamp01(speed / 3) * 25;
            bool attacking = Time.time < attackUntil;
            leftLeg.localRotation = Quaternion.Euler(0, 0, attacking && heavyAttack ? 65 : stride);
            rightLeg.localRotation = Quaternion.Euler(0, 0, -stride);
            leftArm.localRotation = Quaternion.Euler(0, 0, fighter.IsBlocking ? 115 : -stride);
            rightArm.localRotation = Quaternion.Euler(0, 0, fighter.IsBlocking ? 100 : attacking && !heavyAttack ? 95 : stride);
            properties.SetColor("_EmissionColor", Time.time < hitUntil ? Color.white * 0.7f : Color.black);
            foreach (var renderer in renderers) renderer.SetPropertyBlock(properties);
        }
    }
}
