using System.Collections;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Combatant))]
    public class SpecialVisual : MonoBehaviour
    {
        private Combatant combatant;
        private Material material;
        private void Awake()
        {
            combatant = GetComponent<Combatant>();
            material = new Material(Shader.Find("Sprites/Default"));
        }
        private void OnEnable()
        {
            combatant = GetComponent<Combatant>();
            combatant.SpecialActivated += Activate;
        }
        private void OnDisable()
        {
            combatant.SpecialActivated -= Activate;
            StopAllCoroutines();
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
        private void Activate(SpecialKind kind) => StartCoroutine(Animate((int)kind));

        private IEnumerator Animate(int kind)
        {
            var effect = new GameObject("Special");
            effect.transform.SetParent(transform, false);
            var line = effect.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.widthMultiplier = kind == 2 ? 0.16f : 0.07f;
            line.startColor = line.endColor = kind == 6 ? new Color(0.4f, 1, 0.35f) : new Color(1, 0.83f, 0.25f);
            float duration = kind == 3 ? 1.5f : 0.65f;
            float elapsed = 0;
            try
            {
                while (elapsed < duration && combatant.CombatEnabled)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    Vector3 source = transform.position + Vector3.up * 1.4f;
                    Vector3 target = combatant.Opponent == null ? source + Vector3.right * 4 : combatant.Opponent.transform.position + Vector3.up * 1.4f;
                    if (kind == 1 || kind == 2 || kind == 5 || kind == 7)
                    {
                        line.loop = false;
                        line.positionCount = 4;
                        float travel = kind == 7 ? Mathf.Sin(progress * Mathf.PI) : progress;
                        Vector3 tip = Vector3.Lerp(source, target, travel);
                        Vector3 direction = (target - source).normalized;
                        line.SetPositions(new[] { tip - direction * 0.7f, tip, tip - direction * 0.2f + Vector3.up * 0.15f, tip });
                    }
                    else
                    {
                        line.loop = true;
                        line.positionCount = 40;
                        float radius = kind == 0 ? progress * 3 : 0.6f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
                        Vector3 center = kind == 6 || kind == 8 ? target : source;
                        for (int point = 0; point < 40; point++)
                        {
                            float angle = point * Mathf.PI * 2 / 40;
                            line.SetPosition(point, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), -0.1f) * radius);
                        }
                    }
                    yield return null;
                }
            }
            finally { if (effect != null) Destroy(effect); }
        }
    }
}
