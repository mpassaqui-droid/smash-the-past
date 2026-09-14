using SmashThePast.Stages;
using UnityEngine;

namespace SmashThePast.Audio
{
    /// <summary>
    /// Pone musica sin que haya que tocar codigo para anadirla: busca los archivos por
    /// nombre dentro de Resources/Musica y, si el de una arena no existe, usa el de
    /// respaldo. Asi Diego solo tiene que dejar el fichero con el nombre correcto.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class MusicaDelJuego : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField, Range(0f, 1f)] private float volumen = 0.35f;

        public const string Carpeta = "Musica";
        public const string PistaDeMenu = "menu";
        public const string PistaDeRespaldo = "combate";

        private AudioSource fuente;
        private string sonando = "";

        /// <summary>
        /// Mientras la pantalla de titulo esta a la vista, esta musica se calla: el
        /// titulo tiene la suya. El menu principal se muestra estando ya en fase
        /// Selection, asi que sin esto sonaban las dos pistas a la vez.
        /// </summary>
        public static bool TituloEnPantalla { get; set; }

        /// <summary>Nombres a probar, en orden, para un estado del juego. El primero que
        /// exista en Resources/Musica es el que suena.</summary>
        public static string[] Candidatas(MatchPhase fase, int arena)
        {
            if (fase == MatchPhase.Selection)
                return new[] { PistaDeMenu, PistaDeRespaldo };

            if (arena < 0) return new[] { PistaDeRespaldo };
            return new[] { $"arena{arena + 1}", PistaDeRespaldo };
        }

        private void Awake()
        {
            fuente = GetComponent<AudioSource>();
            fuente.loop = true;
            fuente.playOnAwake = false;
            fuente.spatialBlend = 0f;
            fuente.volume = volumen;
        }

        private void Update()
        {
            if (match == null) return;

            if (TituloEnPantalla)
            {
                if (fuente.isPlaying) { fuente.Stop(); sonando = ""; }
                return;
            }

            int arena = -1;
            if (match.PlayerOne != null && match.PlayerOne.Data != null && match.Roster != null)
                arena = System.Array.IndexOf(match.Roster, match.PlayerOne.Data);

            foreach (string nombre in Candidatas(match.Phase, arena))
            {
                var pista = Resources.Load<AudioClip>($"{Carpeta}/{nombre}");
                if (pista == null) continue;
                if (sonando == nombre && fuente.isPlaying) return;

                sonando = nombre;
                fuente.clip = pista;
                fuente.volume = volumen;
                fuente.Play();
                return;
            }

            // Sin ningun archivo puesto todavia, el juego suena solo con los golpes.
            if (fuente.isPlaying) { fuente.Stop(); sonando = ""; }
        }
    }
}
