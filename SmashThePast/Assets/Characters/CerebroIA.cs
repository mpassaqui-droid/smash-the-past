using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    public enum NivelIA { Facil, Normal, Dificil }

    public enum AccionIA { Esperar, Acercarse, Alejarse, GolpeLigero, GolpeFuerte, Guardia, Presa, Special }

    /// <summary>
    /// Adversario de la maquina. No es aprendizaje automatico ni falta que hace: un
    /// jugador de pelea decide por distancia y por lo que hace el rival. Toda la
    /// decision vive en Decidir, que es una funcion pura y por eso se puede probar
    /// sin abrir el juego; el MonoBehaviour solo mide, temporiza y ejecuta.
    /// </summary>
    public sealed class CerebroIA : MonoBehaviour
    {
        [SerializeField] private Fighter rival;
        [SerializeField] private NivelIA nivel = NivelIA.Normal;

        private Fighter propio;
        private Combatant combatiente;
        private float proximaDecision;
        private AccionIA accion = AccionIA.Esperar;
        private float finGuardia;

        public NivelIA Nivel { get => nivel; set => nivel = value; }
        public AccionIA UltimaAccion => accion;

        // Alcance del golpe ligero en HitDetector. Por debajo de esto se pega.
        public const float DistanciaGolpe = 1.8f;
        public const float DistanciaPresa = 1.35f;

        public static float SegundosDeReaccion(NivelIA nivel) => nivel switch
        {
            NivelIA.Facil => 0.45f,
            NivelIA.Dificil => 0.12f,
            _ => 0.26f,
        };

        /// <summary>Probabilidad de tapar cuando el rival esta encima y amenaza.</summary>
        public static float ProbabilidadDeGuardia(NivelIA nivel) => nivel switch
        {
            NivelIA.Facil => 0.15f,
            NivelIA.Dificil => 0.7f,
            _ => 0.4f,
        };

        /// <summary>
        /// Decision pura. `sorteo` es un numero entre 0 y 1 que el llamante saca del azar,
        /// asi la prueba puede fijarlo y el resultado deja de depender de la suerte.
        /// </summary>
        public static AccionIA Decidir(float distancia, bool rivalBloquea, bool rivalAtaca,
            bool specialListo, NivelIA nivel, float sorteo)
        {
            if (distancia > DistanciaGolpe * 1.15f) return AccionIA.Acercarse;

            // Al que se queda tapado se le agarra: es justo para lo que sirve la presa.
            if (rivalBloquea && distancia <= DistanciaPresa) return AccionIA.Presa;

            // Si el rival esta pegando y estamos a tiro, taparse segun el nivel.
            if (rivalAtaca && sorteo < ProbabilidadDeGuardia(nivel)) return AccionIA.Guardia;

            if (specialListo && sorteo < 0.35f) return AccionIA.Special;

            // Mezcla de golpes: el fuerte pega mas pero deja mas vendido, asi que no se
            // abusa. En facil casi solo golpe ligero, en dificil mas variedad.
            float fuerte = nivel == NivelIA.Facil ? 0.15f : nivel == NivelIA.Dificil ? 0.45f : 0.3f;
            return sorteo < fuerte ? AccionIA.GolpeFuerte : AccionIA.GolpeLigero;
        }

        public void Configurar(Fighter contrincante, NivelIA dificultad)
        {
            rival = contrincante;
            nivel = dificultad;
        }

        private void Awake()
        {
            propio = GetComponent<Fighter>();
            combatiente = GetComponent<Combatant>();
        }

        private void OnDisable()
        {
            if (combatiente != null) combatiente.ControlledByAi = false;
            if (propio != null) propio.SetBlocking(false);
        }

        private void Update()
        {
            if (propio == null) propio = GetComponent<Fighter>();
            if (combatiente == null) combatiente = GetComponent<Combatant>();
            if (propio == null || rival == null) return;

            combatiente ??= null;
            if (combatiente != null) combatiente.ControlledByAi = true;

            if (Time.timeScale <= 0f) return;
            if (propio.Health == null || propio.Health.IsDead) return;
            if (rival.Health == null || rival.Health.IsDead) { propio.SetBlocking(false); return; }
            if (propio.IsStunned) return;

            // La guardia se mantiene un rato corto, si no la IA parpadea entre tapar y no.
            if (Time.time < finGuardia)
            {
                propio.SetBlocking(true);
                return;
            }

            if (Time.time < proximaDecision)
            {
                Ejecutar(accion);
                return;
            }

            proximaDecision = Time.time + SegundosDeReaccion(nivel);
            float distancia = Mathf.Abs(rival.transform.position.x - transform.position.x);
            bool rivalAtaca = Vector3.Distance(rival.transform.position, transform.position) <= DistanciaGolpe * 1.2f;
            bool specialListo = combatiente != null && propio.Special != null && propio.Special.IsReady;
            accion = Decidir(distancia, rival.IsBlocking, rivalAtaca, specialListo, nivel, Random.value);
            Ejecutar(accion);
        }

        private void Ejecutar(AccionIA orden)
        {
            float direccion = Mathf.Sign(rival.transform.position.x - transform.position.x);
            if (Mathf.Approximately(direccion, 0f)) direccion = 1f;

            switch (orden)
            {
                case AccionIA.Acercarse:
                    propio.SetBlocking(false);
                    propio.Move(direccion);
                    break;
                case AccionIA.Alejarse:
                    propio.SetBlocking(false);
                    propio.Move(-direccion);
                    break;
                case AccionIA.Guardia:
                    finGuardia = Time.time + 0.35f;
                    propio.SetBlocking(true);
                    break;
                case AccionIA.GolpeLigero:
                    propio.SetBlocking(false);
                    propio.PerformLightAttack();
                    break;
                case AccionIA.GolpeFuerte:
                    propio.SetBlocking(false);
                    propio.PerformHeavyAttack();
                    break;
                case AccionIA.Presa:
                    propio.SetBlocking(false);
                    propio.PerformGrab();
                    break;
                case AccionIA.Special:
                    propio.SetBlocking(false);
                    if (combatiente != null) combatiente.TrySpecial();
                    else propio.TryUseSpecial();
                    break;
            }
        }
    }
}
