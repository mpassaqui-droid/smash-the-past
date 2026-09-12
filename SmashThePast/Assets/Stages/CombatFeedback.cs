using System;
using SmashThePast.Characters;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Stages
{
    [DefaultExecutionOrder(100)]
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private MatchController match;

        private Fighter boundOne;
        private Fighter boundTwo;
        private HitDetector detectorOne;
        private HitDetector detectorTwo;
        private Action<int> hitOne;
        private Action<int> hitTwo;
        private CameraShake cameraShake;
        private Material impactMaterial;

        private void Awake()
        {
            Shader shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null)
                impactMaterial = new Material(shader) { color = new Color(1f, 0.7f, 0.18f, 1f) };
        }

        private void Start()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                cameraShake = camera.GetComponent<CameraShake>();
                if (cameraShake == null) cameraShake = camera.gameObject.AddComponent<CameraShake>();
            }
            BindCurrentFighters();
        }

        private void Update()
        {
            if (match == null) return;
            if (match.PlayerOne != boundOne || match.PlayerTwo != boundTwo)
                BindCurrentFighters();
        }

        private void OnDisable() => Unbind();

        private void OnDestroy()
        {
            if (impactMaterial != null) Destroy(impactMaterial);
        }

        private void BindCurrentFighters()
        {
            Unbind();
            boundOne = match == null ? null : match.PlayerOne;
            boundTwo = match == null ? null : match.PlayerTwo;
            Bind(boundOne, out detectorOne, out hitOne);
            Bind(boundTwo, out detectorTwo, out hitTwo);
        }

        private void Bind(Fighter fighter, out HitDetector detector, out Action<int> callback)
        {
            detector = fighter == null ? null : fighter.GetComponent<HitDetector>();
            var detectorForCallback = detector;
            callback = detectorForCallback == null ? null : damage => HitLanded(detectorForCallback, damage);
            if (detector != null) detector.HitLanded += callback;
        }

        private void Unbind()
        {
            if (detectorOne != null) detectorOne.HitLanded -= hitOne;
            if (detectorTwo != null) detectorTwo.HitLanded -= hitTwo;
            boundOne = null;
            boundTwo = null;
            detectorOne = null;
            detectorTwo = null;
            hitOne = null;
            hitTwo = null;
        }

        private void HitLanded(HitDetector detector, int damage)
        {
            if (damage <= 0 || detector == null || detector.Opponent == null) return;
            bool heavy = damage >= 10;
            SpawnImpact(detector.Opponent.transform.position + Vector3.up * 1.1f, heavy);
            if (cameraShake != null) cameraShake.AddShake(heavy ? 0.22f : 0.1f);
        }

        private void SpawnImpact(Vector3 position, bool heavy)
        {
            var effect = new GameObject(heavy ? "Impacto fuerte" : "Impacto ligero");
            effect.transform.position = position;
            var particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.duration = 0.18f;
            main.loop = false;
            main.startLifetime = heavy ? 0.32f : 0.22f;
            main.startSpeed = heavy ? 4.6f : 3.2f;
            main.startSize = heavy ? 0.14f : 0.09f;
            main.startColor = heavy ? new Color(1f, 0.36f, 0.14f) : new Color(1f, 0.85f, 0.36f);
            main.maxParticles = heavy ? 18 : 10;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, heavy ? 18 : 10) });
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.04f;
            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            if (impactMaterial != null) renderer.material = impactMaterial;
            particles.Play();
            Destroy(effect, 0.8f);
        }
    }
}
