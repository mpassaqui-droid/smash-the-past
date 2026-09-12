using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Stages
{
    public sealed class StageSetController : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private GameObject[] stages;
        private int activeIndex = -1;
        private Light luzClave;
        private Camera camara;

        // Atmosfera por escenario. Las nueve arenas comparten geometria y logica, asi
        // que lo que las separa de verdad es la luz: color y angulo del sol, ambiente,
        // niebla y fondo. Es gratis y cambia mas el cuadro que anadir poligonos.
        private readonly struct Atmosfera
        {
            public readonly Color Sol, Cielo, Ecuador, Suelo, Niebla, Fondo;
            public readonly float Intensidad, InclinacionSol, GiroSol, NieblaCerca, NieblaLejos;

            public Atmosfera(Color sol, float intensidad, float inclinacion, float giro,
                Color cielo, Color ecuador, Color suelo, Color niebla, float cerca, float lejos, Color fondo)
            {
                Sol = sol; Intensidad = intensidad; InclinacionSol = inclinacion; GiroSol = giro;
                Cielo = cielo; Ecuador = ecuador; Suelo = suelo;
                Niebla = niebla; NieblaCerca = cerca; NieblaLejos = lejos; Fondo = fondo;
            }
        }

        private static readonly Atmosfera[] Atmosferas =
        {
            // Patio andino: sol de altura, sombras frias, cielo de pizarra.
            new Atmosfera(new Color(1f, 0.94f, 0.82f), 1.15f, 48f, -38f,
                new Color(0.28f, 0.33f, 0.42f), new Color(0.2f, 0.21f, 0.24f), new Color(0.16f, 0.15f, 0.14f),
                new Color(0.42f, 0.5f, 0.58f), 18f, 70f, new Color(0.16f, 0.2f, 0.27f)),
            // Campamento mongol: estepa encapotada, luz plana y fria.
            new Atmosfera(new Color(0.86f, 0.89f, 0.95f), 0.85f, 62f, -20f,
                new Color(0.34f, 0.37f, 0.41f), new Color(0.24f, 0.25f, 0.27f), new Color(0.15f, 0.15f, 0.14f),
                new Color(0.55f, 0.58f, 0.6f), 12f, 55f, new Color(0.2f, 0.22f, 0.25f)),
            // Dojo de Musashi: tarde de ambar, sombras largas.
            new Atmosfera(new Color(1f, 0.8f, 0.55f), 1.05f, 22f, -62f,
                new Color(0.33f, 0.26f, 0.24f), new Color(0.24f, 0.19f, 0.17f), new Color(0.14f, 0.11f, 0.09f),
                new Color(0.55f, 0.4f, 0.29f), 16f, 60f, new Color(0.22f, 0.15f, 0.13f)),
            // Ashram: mediodia calido y polvoriento, todo lavado de blanco.
            new Atmosfera(new Color(1f, 0.97f, 0.88f), 1.3f, 70f, -10f,
                new Color(0.45f, 0.44f, 0.4f), new Color(0.33f, 0.31f, 0.28f), new Color(0.2f, 0.18f, 0.15f),
                new Color(0.74f, 0.71f, 0.62f), 14f, 52f, new Color(0.3f, 0.29f, 0.26f)),
            // Palacio de Rasputin: noche de invierno, azul y niebla cerrada.
            new Atmosfera(new Color(0.62f, 0.72f, 1f), 0.7f, 35f, 145f,
                new Color(0.16f, 0.19f, 0.3f), new Color(0.12f, 0.14f, 0.2f), new Color(0.07f, 0.08f, 0.11f),
                new Color(0.2f, 0.24f, 0.36f), 8f, 38f, new Color(0.07f, 0.09f, 0.14f)),
            // Campamento napoleonico: amanecer gris y humo de campamento.
            new Atmosfera(new Color(0.92f, 0.9f, 0.86f), 0.9f, 18f, -75f,
                new Color(0.3f, 0.31f, 0.34f), new Color(0.22f, 0.22f, 0.24f), new Color(0.13f, 0.13f, 0.13f),
                new Color(0.5f, 0.51f, 0.52f), 10f, 45f, new Color(0.18f, 0.19f, 0.21f)),
            // Palacio de Cleopatra: sol del desierto a plomo, oro y sombra dura.
            new Atmosfera(new Color(1f, 0.9f, 0.68f), 1.45f, 76f, -25f,
                new Color(0.42f, 0.36f, 0.26f), new Color(0.3f, 0.26f, 0.19f), new Color(0.18f, 0.15f, 0.11f),
                new Color(0.72f, 0.62f, 0.42f), 20f, 80f, new Color(0.28f, 0.23f, 0.16f)),
            // Plaza de Lincoln: otono encapotado, azul apagado.
            new Atmosfera(new Color(0.88f, 0.91f, 0.98f), 0.8f, 40f, -50f,
                new Color(0.27f, 0.31f, 0.36f), new Color(0.2f, 0.22f, 0.25f), new Color(0.12f, 0.13f, 0.14f),
                new Color(0.47f, 0.52f, 0.57f), 15f, 58f, new Color(0.17f, 0.2f, 0.23f)),
            // Agora de Socrates: mediodia mediterraneo, luz blanca y limpia.
            new Atmosfera(new Color(1f, 0.98f, 0.93f), 1.25f, 66f, -30f,
                new Color(0.4f, 0.45f, 0.5f), new Color(0.29f, 0.31f, 0.33f), new Color(0.17f, 0.17f, 0.16f),
                new Color(0.68f, 0.72f, 0.75f), 22f, 85f, new Color(0.24f, 0.28f, 0.32f)),
        };

        public GameObject[] Stages => stages;

        private void OnEnable()
        {
            if (match != null) match.Changed += Refresh;
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (match != null) match.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (match == null || match.PlayerOne == null || match.PlayerOne.Data == null || match.Roster == null) return;
            int selected = System.Array.IndexOf(match.Roster, match.PlayerOne.Data);
            if (selected < 0 || selected == activeIndex) return;
            activeIndex = Mathf.Clamp(selected, 0, stages.Length - 1);
            for (int index = 0; index < stages.Length; index++)
                if (stages[index] != null) stages[index].SetActive(index == activeIndex);
            AplicarAtmosfera(activeIndex);
        }

        private void AplicarAtmosfera(int indice)
        {
            if (indice < 0 || indice >= Atmosferas.Length) return;
            var aire = Atmosferas[indice];

            if (luzClave == null)
            {
                var objeto = GameObject.Find("LuzClave");
                if (objeto != null) luzClave = objeto.GetComponent<Light>();
            }

            if (luzClave != null)
            {
                luzClave.color = aire.Sol;
                luzClave.intensity = aire.Intensidad;
                luzClave.transform.rotation = Quaternion.Euler(aire.InclinacionSol, aire.GiroSol, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = aire.Cielo;
            RenderSettings.ambientEquatorColor = aire.Ecuador;
            RenderSettings.ambientGroundColor = aire.Suelo;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = aire.Niebla;
            RenderSettings.fogStartDistance = aire.NieblaCerca;
            RenderSettings.fogEndDistance = aire.NieblaLejos;

            if (camara == null) camara = Camera.main;
            if (camara != null) camara.backgroundColor = aire.Fondo;
        }
    }
}
