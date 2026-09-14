using UnityEngine;

namespace SmashThePast.UI
{
    /// <summary>
    /// Los dos sonidos de la interfaz: uno al pasar por encima de un boton y otro al
    /// pulsarlo. Estan sintetizados, asi que no dependen de ninguna licencia.
    ///
    /// Se llega por una instancia estatica porque los botones se crean en un metodo
    /// estatico de la interfaz; asi no hay que pasar una referencia por toda la cadena.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SonidoDeMenu : MonoBehaviour
    {
        private static SonidoDeMenu instancia;

        [SerializeField, Range(0f, 1f)] private float volumen = 0.5f;

        private AudioSource fuente;
        private AudioClip mover, aceptar;
        private float ultimoMover;

        private void Awake()
        {
            instancia = this;
            fuente = gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            fuente.spatialBlend = 0f;
            mover = Resources.Load<AudioClip>("Interfaz/menu_mover");
            aceptar = Resources.Load<AudioClip>("Interfaz/menu_aceptar");
            if (mover == null || aceptar == null)
                Debug.LogWarning("[sonido menu] faltan clips en Resources/Interfaz");
        }

        private void OnDestroy() { if (instancia == this) instancia = null; }

        public static void Mover()
        {
            // El raton puede entrar y salir de un boton varias veces por segundo; sin
            // este limite el sonido se dispara en rafaga y molesta.
            if (instancia == null || instancia.mover == null) return;
            if (Time.unscaledTime - instancia.ultimoMover < 0.06f) return;
            instancia.ultimoMover = Time.unscaledTime;
            instancia.fuente.PlayOneShot(instancia.mover, instancia.volumen);
        }

        public static void Aceptar()
        {
            if (instancia == null || instancia.aceptar == null) return;
            instancia.fuente.PlayOneShot(instancia.aceptar, instancia.volumen);
        }
    }
}
