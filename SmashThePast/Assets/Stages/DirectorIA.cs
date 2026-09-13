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
                if (teclado.f1Key.wasPressedThisFrame) Alternar();
                if (teclado.f2Key.wasPressedThisFrame) SubirNivel();
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
