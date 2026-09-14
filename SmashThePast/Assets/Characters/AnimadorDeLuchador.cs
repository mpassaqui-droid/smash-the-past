using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    /// <summary>
    /// Traduce lo que hace el luchador a parametros del Animator. Sustituye al
    /// movimiento a mano de FighterAnimation: aquel empujaba transforms, este dispara
    /// clips de verdad hechos sobre el esqueleto.
    /// </summary>
    [RequireComponent(typeof(Fighter))]
    public sealed class AnimadorDeLuchador : MonoBehaviour
    {
        [SerializeField] private Animator animador;

        private Fighter luchador;
        private Combatant combatiente;
        private Vector3 posicionAnterior;

        private static readonly int Velocidad = Animator.StringToHash("velocidad");
        private static readonly int Guardia = Animator.StringToHash("guardia");
        private static readonly int Ligero = Animator.StringToHash("ligero");
        private static readonly int Fuerte = Animator.StringToHash("fuerte");
        private static readonly int Encajar = Animator.StringToHash("encajar");
        private static readonly int Ko = Animator.StringToHash("ko");

        private void Awake()
        {
            luchador = GetComponent<Fighter>();
            combatiente = GetComponent<Combatant>();
            if (animador == null) animador = GetComponentInChildren<Animator>();
            posicionAnterior = transform.position;
        }

        private void OnEnable()
        {
            luchador = GetComponent<Fighter>();
            luchador.OnLightAttackPerformed += GolpeLigero;
            luchador.OnHeavyAttackPerformed += GolpeFuerte;
            luchador.OnStunned += Aturdido;
            luchador.OnKnockedOut += Caido;
        }

        private void OnDisable()
        {
            if (luchador == null) return;
            luchador.OnLightAttackPerformed -= GolpeLigero;
            luchador.OnHeavyAttackPerformed -= GolpeFuerte;
            luchador.OnStunned -= Aturdido;
            luchador.OnKnockedOut -= Caido;
        }

        private void Update()
        {
            if (animador == null) return;

            // La velocidad sale del movimiento real, no del input: asi vale igual para el
            // jugador y para la maquina.
            float recorrido = Mathf.Abs(transform.position.x - posicionAnterior.x);
            posicionAnterior = transform.position;
            float velocidad = Time.deltaTime > 0f ? recorrido / Time.deltaTime : 0f;
            animador.SetFloat(Velocidad, velocidad);
            animador.SetBool(Guardia, luchador.IsBlocking);
        }

        private void GolpeLigero() => Disparar(Ligero);
        private void GolpeFuerte() => Disparar(Fuerte);
        private void Caido() => Disparar(Ko);

        // Solo el aturdimiento de encajar un golpe merece animacion; el del bloqueo es
        // demasiado corto y parpadearia.
        private void Aturdido(float segundos)
        {
            if (segundos >= Fighter.LightHitStunSeconds - 0.01f) Disparar(Encajar);
        }

        private void Disparar(int parametro)
        {
            if (animador != null && animador.isActiveAndEnabled) animador.SetTrigger(parametro);
        }
    }
}
