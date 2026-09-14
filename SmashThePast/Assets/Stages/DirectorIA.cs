using SmashThePast.Characters;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmashThePast.Stages
{
    /// <summary>
    /// Pone y quita la maquina en el puesto de P2. Por defecto activa, para que el juego
    /// se pueda jugar solo; F1 la apaga y devuelve el mando al segundo jugador.
    /// </summary>
    public sealed class DirectorIA : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        // Apagada por defecto: el juego nacio para dos en el mismo teclado y encenderla
        // sola cambiaria esa experiencia sin avisar. Ademas, con la maquina peleando
        // dentro de las pruebas de escena, los resultados dejan de ser reproducibles.
        [SerializeField] private bool activa;
        [SerializeField] private NivelIA nivel = NivelIA.Normal;

        public bool Activa => activa;
        public NivelIA Nivel => nivel;

        private CerebroIA cerebro;

        private void Update()
        {
            var teclado = Keyboard.current;
            if (teclado != null)
            {
                // En un Mac las teclas F son brillo y volumen salvo que se mantenga Fn,
                // asi que la maquina se enciende con M (maquina) y el nivel cambia con N.
                // Las F se dejan como alternativa para quien las tenga configuradas.
                if (teclado.mKey.wasPressedThisFrame || teclado.f1Key.wasPressedThisFrame) Alternar();
                if (teclado.nKey.wasPressedThisFrame || teclado.f2Key.wasPressedThisFrame) SubirNivel();
            }

            if (match == null) return;
            var segundo = match.PlayerTwo;
            if (segundo == null) return;

            if (!activa)
            {
                if (cerebro != null) cerebro.enabled = false;
                return;
            }

            if (cerebro == null || cerebro.gameObject != segundo.gameObject)
            {
                cerebro = segundo.GetComponent<CerebroIA>();
                if (cerebro == null) cerebro = segundo.gameObject.AddComponent<CerebroIA>();
            }

            cerebro.enabled = true;
            cerebro.Configurar(match.PlayerOne, nivel);
        }

        /// <summary>Enciende o apaga la maquina desde el menu, sin alternar a ciegas.</summary>
        public void Activar(bool encendida)
        {
            activa = encendida;
            if (!activa && cerebro != null) cerebro.enabled = false;
        }

        public void FijarNivel(NivelIA nuevo)
        {
            nivel = nuevo;
            if (cerebro != null) cerebro.Nivel = nivel;
        }

        public void Alternar()
        {
            activa = !activa;
            if (!activa && cerebro != null) cerebro.enabled = false;
            Debug.Log($"IA {(activa ? "activada" : "desactivada")}");
        }

        public void SubirNivel()
        {
            nivel = nivel == NivelIA.Facil ? NivelIA.Normal
                : nivel == NivelIA.Normal ? NivelIA.Dificil : NivelIA.Facil;
            if (cerebro != null) cerebro.Nivel = nivel;
            Debug.Log($"IA en nivel {nivel}");
        }
    }
}
