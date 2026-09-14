using UnityEngine;
using UnityEngine.Video;

namespace SmashThePast.UI
{
    /// <summary>
    /// Fondo animado y musica del menu principal. El video se dibuja en una
    /// RenderTexture y UI Toolkit la usa como imagen de fondo del panel: asi el menu
    /// sigue siendo UI normal, con sus botones encima, y no hace falta una camara aparte.
    ///
    /// El clip ya viene sin zoom y preparado para enlazar (se le quito el zoom original
    /// y se le hizo un fundido consigo mismo), asi que aqui solo hay que ponerlo en
    /// bucle y dejarlo correr.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TituloDelMenu : MonoBehaviour
    {
        public const string RutaVideo = "Video/TituloMenu";
        public const string RutaMusica = "Musica/titulo";

        [SerializeField] private VideoClip clip;
        [SerializeField, Range(0f, 1f)] private float volumenMusica = 0.55f;

        private VideoPlayer reproductor;
        private AudioSource musica;
        private RenderTexture lienzo;
        private bool encendido, preparado;

        /// <summary>La textura donde cae el video, o null si no hay clip.</summary>
        public Texture Textura { get { Preparar(); return lienzo; } }

        private void Awake() => Preparar();

        /// <summary>
        /// Se puede llamar desde Awake o desde la propiedad. Los dos componentes viven
        /// en el mismo objeto y Unity no garantiza en que orden llama a sus Awake: la
        /// primera version montaba esto en Awake y la interfaz pedia la textura antes,
        /// se la encontraba vacia y caia al titulo escrito.
        /// </summary>
        private void Preparar()
        {
            if (preparado) return;
            preparado = true;

            if (clip == null) clip = Resources.Load<VideoClip>(RutaVideo);
            if (clip == null)
            {
                Debug.LogWarning("[titulo] no hay clip en Resources/" + RutaVideo + ": el menu se queda sin fondo animado");
                return;
            }

            lienzo = new RenderTexture((int)clip.width, (int)clip.height, 0) { name = "TituloMenu" };
            reproductor = gameObject.AddComponent<VideoPlayer>();
            reproductor.clip = clip;
            reproductor.renderMode = VideoRenderMode.RenderTexture;
            reproductor.targetTexture = lienzo;
            reproductor.isLooping = true;
            reproductor.playOnAwake = false;
            reproductor.waitForFirstFrame = true;
            // El video no lleva sonido: la musica va aparte para poder pararla sola.
            reproductor.audioOutputMode = VideoAudioOutputMode.None;

            var pista = Resources.Load<AudioClip>(RutaMusica);
            if (pista != null)
            {
                musica = gameObject.AddComponent<AudioSource>();
                musica.clip = pista;
                musica.loop = true;
                musica.playOnAwake = false;
                musica.volume = volumenMusica;
                musica.spatialBlend = 0f;
            }
            else Debug.LogWarning("[titulo] no hay musica en Resources/" + RutaMusica);

            Debug.Log($"[titulo] listo: video {clip.width}x{clip.height} {clip.length:F2}s, musica {(pista != null ? pista.length.ToString("F0") + "s" : "ninguna")}");
        }

        /// <summary>Se llama desde la interfaz cada vez que refresca, con si el menu se ve.</summary>
        public void Mostrar(bool visible)
        {
            // Se avisa siempre, no solo al cambiar: MusicaDelJuego decide en cada
            // Update y tiene que encontrar la bandera puesta.
            SmashThePast.Audio.MusicaDelJuego.TituloEnPantalla = visible;
            if (visible == encendido) return;
            encendido = visible;
            if (reproductor != null) { if (visible) reproductor.Play(); else reproductor.Pause(); }
            if (musica != null) { if (visible) musica.Play(); else musica.Stop(); }
        }

        private void OnDisable() => SmashThePast.Audio.MusicaDelJuego.TituloEnPantalla = false;

        private void OnDestroy()
        {
            if (lienzo != null) { lienzo.Release(); Destroy(lienzo); }
        }
    }
}
