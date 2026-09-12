using UnityEngine;

namespace SmashThePast.Stages
{
    [RequireComponent(typeof(Camera))]
    public class ArenaCamera : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        private const float AlturaBase = 2.24f;
        private const float DistanciaBase = -9.2f;
        private const float InclinacionBase = 4f;
        private const float TamanoMinimo = 2.9f;

        private Camera view;
        private float empuje;

        // Empujon de camara al confirmar un golpe: la vista se acerca de golpe y vuelve
        // sola. Va en unidades de tamano ortografico, no en posicion, para que no pelee
        // con el encuadre que sigue a los dos luchadores.
        public void Golpe(float fuerza) => empuje = Mathf.Max(empuje, fuerza);
        private void Awake() => view = GetComponent<Camera>();
        private void LateUpdate()
        {
            if (match == null || match.PlayerOne == null || match.PlayerTwo == null) return;
            float center = Mathf.Clamp((match.PlayerOne.transform.position.x + match.PlayerTwo.transform.position.x) / 2, -2, 2);
            // Estos valores son el encuadre de verdad: LateUpdate los reescribe cada
            // fotograma, asi que lo que se guarde en la escena no llega nunca al juego.
            transform.position = Vector3.Lerp(transform.position, new Vector3(center, AlturaBase, DistanciaBase), Time.unscaledDeltaTime * 4);
            transform.rotation = Quaternion.Euler(InclinacionBase, 0, 0);
            float span = Mathf.Abs(match.PlayerOne.transform.position.x - match.PlayerTwo.transform.position.x) + 4.5f;
            float size = Mathf.Max(TamanoMinimo, span / (2 * Mathf.Max(0.5f, view.aspect)));

            // Todo en tiempo sin escalar: durante la parada de fotograma el tiempo del
            // juego esta a cero y el empujon tiene que seguir viendose.
            empuje = Mathf.MoveTowards(empuje, 0f, Time.unscaledDeltaTime * 1.1f);
            view.orthographicSize = Mathf.Lerp(view.orthographicSize, Mathf.Max(1f, size - empuje), Time.unscaledDeltaTime * 9);
        }
    }
}
