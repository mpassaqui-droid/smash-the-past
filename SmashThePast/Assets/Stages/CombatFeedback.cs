using System;
using System.Collections;
using System.Collections.Generic;
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
        private ArenaCamera arenaCamera;
        private Material impactMaterial;
        private Coroutine pausaDeImpacto;
        private readonly Dictionary<Renderer, Color> coloresOriginales = new Dictionary<Renderer, Color>();
        private static readonly int IdColorBase = Shader.PropertyToID("_BaseColor");

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
                arenaCamera = camera.GetComponent<ArenaCamera>();
            }
            BindCurrentFighters();
        }

        private void Update()
        {
            if (match == null) return;
            if (match.PlayerOne != boundOne || match.PlayerTwo != boundTwo)
                BindCurrentFighters();
        }

        private void OnDisable()
        {
            // Si el objeto se apaga en plena parada de fotograma, el juego se quedaria
            // congelado para siempre.
            if (pausaDeImpacto != null)
            {
                StopCoroutine(pausaDeImpacto);
                pausaDeImpacto = null;
                if (match != null && match.Phase == MatchPhase.Fighting) Time.timeScale = 1f;
            }
            Unbind();
        }

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
            if (arenaCamera != null) arenaCamera.Golpe(heavy ? 0.26f : 0.12f);
            Congelar(heavy ? 0.15f : 0.08f);
            Destellar(detector.Opponent, heavy);
        }


        // Parada de fotograma. En un juego de pelea es lo que hace que un golpe
        // "duela" en pantalla: el tiempo se detiene unas centesimas en el impacto.
        // Nunca pisa la pausa del jugador: si el combate deja de estar en marcha,
        // no toca timeScale al terminar.
        private void Congelar(float duracion)
        {
            if (match == null || match.Phase != MatchPhase.Fighting) return;
            if (pausaDeImpacto != null) StopCoroutine(pausaDeImpacto);
            pausaDeImpacto = StartCoroutine(Congelacion(duracion));
        }

        private IEnumerator Congelacion(float duracion)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duracion);
            if (match != null && match.Phase == MatchPhase.Fighting) Time.timeScale = 1f;
            pausaDeImpacto = null;
        }

        // Destello blanco sobre el que encaja. Se hace con MaterialPropertyBlock para
        // no crear copias de material ni romper el SRP Batcher.
        private void Destellar(Fighter victima, bool fuerte)
        {
            if (victima == null) return;
            StartCoroutine(Destello(victima.GetComponentsInChildren<Renderer>(true), fuerte ? 0.12f : 0.07f));
        }

        private IEnumerator Destello(Renderer[] piezas, float duracion)
        {
            var bloque = new MaterialPropertyBlock();
            foreach (var pieza in piezas)
            {
                if (pieza == null || pieza.sharedMaterial == null) continue;
                if (!coloresOriginales.ContainsKey(pieza))
                    coloresOriginales[pieza] = pieza.sharedMaterial.HasProperty(IdColorBase)
                        ? pieza.sharedMaterial.GetColor(IdColorBase)
                        : Color.white;
                pieza.GetPropertyBlock(bloque);
                bloque.SetColor(IdColorBase, Color.white);
                pieza.SetPropertyBlock(bloque);
            }

            yield return new WaitForSecondsRealtime(duracion);

            foreach (var pieza in piezas)
            {
                if (pieza == null) continue;
                pieza.GetPropertyBlock(bloque);
                bloque.SetColor(IdColorBase, coloresOriginales.TryGetValue(pieza, out var color) ? color : Color.white);
                pieza.SetPropertyBlock(bloque);
            }
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
