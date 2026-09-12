using System.Collections;
using System.Collections.Generic;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Combatant))]
    public class SpecialVisual : MonoBehaviour
    {
        private Combatant combatant;
        private Material material;
        private readonly List<GameObject> effects = new List<GameObject>();
        public int ActiveEffectCount => effects.Count;
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
            foreach (var effect in effects) if (effect != null) Destroy(effect);
            effects.Clear();
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
        private void Activate(SpecialKind kind) => StartCoroutine(Animate((int)kind));

        private IEnumerator Animate(int kind)
        {
            var effect = new GameObject("Special " + (SpecialKind)kind);
            effect.transform.SetParent(transform, false);
            effects.Add(effect);
            var line = effect.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.widthMultiplier = kind == 2 ? 0.2f : 0.13f;
            line.numCapVertices = 3;
            Color color = kind == 6 ? new Color(0.4f, 1, 0.35f) : kind == 3 ? new Color(0.4f, 0.95f, 1)
                : kind == 8 ? new Color(1, 0.5f, 0.8f) : new Color(1, 0.83f, 0.25f);
            line.startColor = line.endColor = color;
            Transform projectile = null;
            if (kind == 7)
            {
                projectile = new GameObject("Galera").transform;
                projectile.SetParent(effect.transform, false);
                Solid(projectile, "Ala", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.95f, 0.04f, 0.95f), new Color(0.12f, 0.13f, 0.17f));
                Solid(projectile, "Copa", PrimitiveType.Cylinder, Vector3.up * 0.27f, new Vector3(0.55f, 0.25f, 0.55f), new Color(0.12f, 0.13f, 0.17f));
                Solid(projectile, "Cinta", PrimitiveType.Cylinder, Vector3.up * 0.1f, new Vector3(0.57f, 0.055f, 0.57f), color);
            }
            else if (kind == 5)
                projectile = Solid(effect.transform, "Bala", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.6f, new Color(0.16f, 0.18f, 0.21f));
            float duration = kind == 6 ? 2.1f : kind == 3 ? 1.5f : kind == 8 ? 2 : 0.65f;
            float elapsed = 0;
            try
            {
                while (elapsed < duration && combatant.CombatEnabled)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    Vector3 source = transform.position + Vector3.up * 1.4f;
                    Vector3 target = combatant.Opponent == null ? source + Vector3.right * 4 : combatant.Opponent.transform.position + Vector3.up * 1.4f;
                    float facing = target.x >= source.x ? 1 : -1;
                    line.startColor = line.endColor = new Color(color.r, color.g, color.b, 1 - progress * 0.7f);
                    if (kind == 1 || kind == 5 || kind == 7)
                    {
                        line.loop = false;
                        float travel = kind == 7 ? elapsed < 0.2f ? elapsed / 0.2f : 1 - Mathf.Clamp01((elapsed - 0.2f) / 0.45f)
                            : kind == 1 ? (elapsed % 0.15f) / 0.15f : progress;
                        Vector3 origin = kind == 7 ? source : source - Vector3.right * facing * 8 + Vector3.up * 0.5f;
                        Vector3 tip = Vector3.Lerp(origin, target, travel);
                        Vector3 direction = (target - source).normalized;
                        line.enabled = kind != 1 || elapsed <= 0.45f;
                        line.positionCount = 5;
                        line.SetPositions(new[] { tip - direction * 1.2f, tip, tip - direction * 0.3f + Vector3.up * 0.2f, tip, tip - direction * 0.3f - Vector3.up * 0.2f });
                        if (projectile != null)
                        {
                            projectile.position = tip;
                            projectile.rotation = Quaternion.Euler(15, elapsed * 1100, 15);
                        }
                    }
                    else if (kind == 8)
                    {
                        line.positionCount = 20;
                        Vector3 center = target + Vector3.up * 1.1f;
                        for (int p = 0; p < 16; p++)
                        {
                            float angle = Mathf.PI - p * Mathf.PI * 1.5f / 15;
                            line.SetPosition(p, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), -0.3f) * 0.4f);
                        }
                        for (int p = 16; p < 20; p++) line.SetPosition(p, center + new Vector3(0, -0.4f - (p - 16) * 0.08f, -0.12f));
                    }
                    else
                    {
                        bool shock = kind == 0 || kind == 9;
                        line.loop = kind != 2 && kind != 6;
                        line.positionCount = 80;
                        float radius = shock ? progress * 3 : kind == 2 ? 1.5f : kind == 4 ? 0.5f + progress * 2 : 0.95f;
                        Vector3 center = kind == 6 ? target : shock ? transform.position + Vector3.up * 0.35f : source;
                        for (int point = 0; point < 80; point++)
                        {
                            float angle = point * Mathf.PI * (kind == 6 || kind == 2 ? 4 : 2) / 79;
                            if (kind == 2) angle += elapsed * 30;
                            Vector3 offset = shock || kind == 6
                                ? new Vector3(Mathf.Cos(angle), kind == 6 ? point / 79f * 1.6f - 0.8f : 0, Mathf.Sin(angle))
                                : new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), -0.25f + (kind == 2 ? point / 79f * 0.4f : 0));
                            line.SetPosition(point, center + offset * radius);
                        }
                    }
                    yield return null;
                }
            }
            finally
            {
                effects.Remove(effect);
                if (effect != null) Destroy(effect);
            }
        }

        private Transform Solid(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
            return part.transform;
        }
    }
}
