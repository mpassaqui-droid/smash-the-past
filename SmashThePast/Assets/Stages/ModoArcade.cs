using System.Collections.Generic;
using System.Linq;
using SmashThePast.Characters;
using UnityEngine;

namespace SmashThePast.Stages
{
    /// <summary>
    /// Escalera de rivales contra la maquina. Sin esto, quien compra el juego y no tiene
    /// a nadie al lado no tiene razon para volver a abrirlo.
    /// La escalera y la dificultad son funciones puras: se prueban sin abrir el juego.
    /// </summary>
    public sealed class ModoArcade : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private DirectorIA director;

        private int[] rivales = System.Array.Empty<int>();

        public bool Activo { get; private set; }
        public int Paso { get; private set; }
        public int Personaje { get; private set; }
        public int Total => rivales.Length;
        public bool Terminado => Activo && Paso >= rivales.Length;
        public int RivalActual => Activo && Paso < rivales.Length ? rivales[Paso] : -1;

        public const string ClaveArcadeSuperado = "arcade_superado";

        /// <summary>Los demas del roster en orden, empezando por el siguiente al elegido:
        /// asi nadie se pelea consigo mismo y cada personaje tiene su propia escalera.</summary>
        public static int[] Escalera(int elegido, int total)
        {
            if (total <= 1) return System.Array.Empty<int>();
            var orden = new List<int>();
            for (int paso = 1; paso < total; paso++) orden.Add((elegido + paso) % total);
            return orden.ToArray();
        }

        /// <summary>La dificultad sube por tramos: el primer tercio facil, el segundo
        /// normal, el ultimo dificil. El final siempre se pelea en dificil.</summary>
        public static NivelIA DificultadDelPaso(int paso, int total)
        {
            if (total <= 1) return NivelIA.Dificil;
            if (paso >= total - 1) return NivelIA.Dificil;
            float avance = (float)paso / (total - 1);
            return avance < 0.34f ? NivelIA.Facil : avance < 0.67f ? NivelIA.Normal : NivelIA.Dificil;
        }

        public static bool Superado => PlayerPrefs.GetInt(ClaveArcadeSuperado, 0) == 1;

        public void Empezar(int personaje)
        {
            Personaje = personaje;
            rivales = Escalera(personaje, match != null && match.Roster != null ? match.Roster.Length : 0);
            Paso = 0;
            Activo = rivales.Length > 0;
            if (Activo) LanzarCombate();
        }

        public void Abandonar()
        {
            Activo = false;
            Paso = 0;
        }

        /// <summary>Lo llama la interfaz cuando termina un combate del arcade.</summary>
        public void ResolverCombate(bool ganoElJugador)
        {
            if (!Activo) return;
            if (!ganoElJugador) { Abandonar(); return; }

            Paso++;
            if (Paso >= rivales.Length)
            {
                PlayerPrefs.SetInt(ClaveArcadeSuperado, 1);
                PlayerPrefs.Save();
                return;
            }

            LanzarCombate();
        }

        private void LanzarCombate()
        {
            if (match == null) return;
            if (director != null)
            {
                director.Activar(true);
                director.FijarNivel(DificultadDelPaso(Paso, rivales.Length));
            }
            match.BeginMatch(Personaje, rivales[Paso]);
        }
    }
}
