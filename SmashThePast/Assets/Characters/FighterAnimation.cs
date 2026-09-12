using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Fighter))]
    public sealed class FighterAnimation : MonoBehaviour
    {
        private Fighter fighter;
        private Combatant combatant;
        private Transform model;
        private Vector3 lastPosition;
        private float actionUntil;
        private float actionStrength;
        private float specialUntil;
        private bool knockedOut;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            combatant = GetComponent<Combatant>();
            model = transform.Find("Modelo");
            lastPosition = transform.position;
        }

        private void OnEnable()
        {
            fighter = GetComponent<Fighter>();
            fighter.OnLightAttackPerformed += LightAttack;
            fighter.OnHeavyAttackPerformed += HeavyAttack;
            fighter.OnKnockedOut += KnockOut;
            combatant = GetComponent<Combatant>();
            if (combatant != null) combatant.SpecialActivated += Special;
        }

        private void OnDisable()
        {
            if (fighter != null)
            {
                fighter.OnLightAttackPerformed -= LightAttack;
                fighter.OnHeavyAttackPerformed -= HeavyAttack;
                fighter.OnKnockedOut -= KnockOut;
            }
            if (combatant != null) combatant.SpecialActivated -= Special;
        }

        private void LightAttack() => StartAction(0.24f, 1);
        private void HeavyAttack() => StartAction(0.32f, 1.35f);
        private void StartAction(float duration, float strength)
        {
            actionUntil = Time.time + duration;
            actionStrength = strength;
        }

        private void Special(SpecialKind _) => specialUntil = Time.time + 0.6f;
        private void KnockOut() => knockedOut = true;

        private void LateUpdate()
        {
            if (model == null) model = transform.Find("Modelo");
            if (model == null) return;
            float speed = Mathf.Abs(transform.position.x - lastPosition.x) / Mathf.Max(Time.deltaTime, 0.001f);
            lastPosition = transform.position;
            float movement = Mathf.Clamp01(speed / 4f);
            float bob = Mathf.Sin(Time.time * (movement > 0.1f ? 11f : 2.6f)) * (movement > 0.1f ? 0.035f : 0.018f);
            float action = Mathf.Clamp01((actionUntil - Time.time) / 0.32f) * actionStrength;
            float pulse = Mathf.Clamp01((specialUntil - Time.time) / 0.6f);
            float recoil = Mathf.Sin((1f - Mathf.Clamp01((actionUntil - Time.time) / 0.32f)) * Mathf.PI) * action;
            float facing = GetFacing();
            model.localPosition = new Vector3(facing * recoil * 0.14f, bob + pulse * 0.04f, 0);
            if (knockedOut)
            {
                model.localScale = Vector3.Lerp(model.localScale, Vector3.one * 0.96f, Time.unscaledDeltaTime * 5f);
                return;
            }
            float lean = movement * Mathf.Sin(Time.time * 11f) * 2.5f - action * 3f;
            model.localRotation = Quaternion.Euler(0, model.localRotation.eulerAngles.y, lean);
            float size = 1f + pulse * 0.035f - action * 0.018f;
            model.localScale = Vector3.Lerp(model.localScale, Vector3.one * size, Time.unscaledDeltaTime * 12f);
        }

        private float GetFacing()
        {
            if (combatant != null && combatant.Opponent != null)
                return Mathf.Sign(combatant.Opponent.transform.position.x - transform.position.x);
            return 1;
        }
    }
}
