using UnityEngine;

namespace SmashThePast.Stages
{
    /// <summary>
    /// Parallax a mano, porque la camara del combate es ORTOGRAFICA: sin perspectiva,
    /// todo se desplaza igual en pantalla este cerca o lejos, y el fondo se lee pegado
    /// a la arena. Moviendo cada capa una fraccion de lo que se mueve la camara, el
    /// fondo se queda atras y aparece la profundidad.
    ///
    /// Factor 0 = clavado al mundo (se mueve todo lo que la camara). Factor 1 = clavado
    /// a la camara (no se mueve nada en pantalla, como un cielo infinito).
    /// Las cumbres lejanas estan en z 25 y las cercanas en z 15, asi que llevan
    /// factores distintos: si no, siguen pareciendo una sola pared.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Parallax : MonoBehaviour
    {
        [System.Serializable]
        public struct Capa
        {
            public Transform objetivo;
            [Range(0f, 1f)] public float factor;
        }

        [SerializeField] private Capa[] capas;
        [SerializeField] private Camera vista;

        private Vector3[] origen;
        private float xInicial;

        private void Awake()
        {
            if (vista == null) vista = Camera.main;
            if (capas == null || capas.Length == 0 || vista == null) { enabled = false; return; }

            origen = new Vector3[capas.Length];
            for (int i = 0; i < capas.Length; i++)
                if (capas[i].objetivo != null) origen[i] = capas[i].objetivo.position;
            xInicial = vista.transform.position.x;
            Debug.Log($"[parallax] {capas.Length} capas siguiendo a '{vista.name}'");
        }

        private void LateUpdate()
        {
            float desvio = vista.transform.position.x - xInicial;
            for (int i = 0; i < capas.Length; i++)
            {
                if (capas[i].objetivo == null) continue;
                var p = origen[i];
                p.x += desvio * capas[i].factor;
                capas[i].objetivo.position = p;
            }
        }

#if UNITY_EDITOR
        public void Configurar(Capa[] nuevas, Camera camara) { capas = nuevas; vista = camara; }
#endif
    }
}
