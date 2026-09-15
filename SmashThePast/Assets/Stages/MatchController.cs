using System;
using SmashThePast.Characters;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmashThePast.Stages
{
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private CharacterData[] roster;
        [SerializeField] private Fighter[] playerOnePrefabs;
        [SerializeField] private Fighter[] playerTwoPrefabs;
        [SerializeField] private Fighter playerOne;
        [SerializeField] private Fighter playerTwo;
        [SerializeField] private bool startWithSelection;
        private int firstIndex;
        private int secondIndex = 1;

        public CharacterData[] Roster => roster;
        public Fighter PlayerOne => playerOne;
        public Fighter PlayerTwo => playerTwo;
        public MatchPhase Phase { get; private set; }

        /// <summary>
        /// Unico sitio donde cambia la fase. Ademas de guardarla, abre o cierra el paso
        /// a los luchadores: fuera del combate no obedecen ni al jugador ni a la maquina.
        /// Antes la maquina se movia y atacaba durante la seleccion de personaje.
        /// </summary>
        private void SetPhase(MatchPhase nueva)
        {
            Phase = nueva;
            SmashThePast.Core.Fighter.CombateEnCurso = nueva == MatchPhase.Fighting;
        }
        public float SecondsRemaining { get; private set; } = 90;
        public int Winner { get; private set; } = -1;

        // Asaltos al mejor de tres. Un combate a una sola vida se decide por un
        // despiste; con asaltos hay remontada, y es lo que espera cualquiera que haya
        // jugado a un juego de pelea.
        public const int AsaltosParaGanar = 2;
        public int AsaltosGanadosP1 { get; private set; }
        public int AsaltosGanadosP2 { get; private set; }
        public int NumeroDeAsalto { get; private set; } = 1;
        public int GanadorDelAsalto { get; private set; } = -1;
        public float SegundosDeAnuncio { get; private set; }
        private const float DuracionAnuncio = 1.6f;
        public string WinQuote { get; private set; } = "";
        public string LoseQuote { get; private set; } = "";
        public event Action Changed;

        private void Start()
        {
            if (startWithSelection) ShowSelection();
            else BeginMatch(0, 1);
        }

        private void Update()
        {
            bool pause = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            foreach (var pad in Gamepad.all) pause |= pad.startButton.wasPressedThisFrame;
            if (pause && (Phase == MatchPhase.Fighting || Phase == MatchPhase.Paused)) TogglePause();

            if (Phase == MatchPhase.RoundIntro)
            {
                SegundosDeAnuncio = Mathf.Max(0, SegundosDeAnuncio - Time.unscaledDeltaTime);
                if (SegundosDeAnuncio <= 0) EmpezarAsalto();
                return;
            }

            if (Phase != MatchPhase.Fighting) return;
            SecondsRemaining = Mathf.Max(0, SecondsRemaining - Time.deltaTime);
            if (SecondsRemaining <= 0)
            {
                int first = playerOne.Health.CurrentHealth;
                int second = playerTwo.Health.CurrentHealth;
                Finish(first == second ? -1 : first > second ? 0 : 1);
            }
        }

        public void Preview(int first, int second)
        {
            if (Phase != MatchPhase.Selection) return;
            Spawn(first, second);
            SetCombat(false);
            Changed?.Invoke();
        }

        public void BeginMatch(int first, int second)
        {
            AsaltosGanadosP1 = AsaltosGanadosP2 = 0;
            NumeroDeAsalto = 1;
            Winner = -1;
            WinQuote = LoseQuote = "";
            PrepararAsalto(first, second);
        }

        private void PrepararAsalto(int first, int second)
        {
            Time.timeScale = 1;
            Spawn(first, second);
            SecondsRemaining = 90;
            GanadorDelAsalto = -1;
            SegundosDeAnuncio = DuracionAnuncio;
            SetPhase(MatchPhase.RoundIntro);
            SetCombat(false);
            Changed?.Invoke();
        }

        private void EmpezarAsalto()
        {
            SetPhase(MatchPhase.Fighting);
            SetCombat(true);
            Changed?.Invoke();
        }

        private void Spawn(int first, int second)
        {
            if (roster == null || roster.Length == 0) throw new InvalidOperationException("Falta el roster de la arena.");
            firstIndex = Mathf.Clamp(first, 0, roster.Length - 1);
            secondIndex = Mathf.Clamp(second, 0, roster.Length - 1);
            Remove(playerOne);
            Remove(playerTwo);
            playerOne = Instantiate(playerOnePrefabs[firstIndex], new Vector3(-2.4f, 0.05f, 0), Quaternion.identity);
            playerTwo = Instantiate(playerTwoPrefabs[secondIndex], new Vector3(2.4f, 0.05f, 0), Quaternion.identity);
            playerOne.GetComponent<HitDetector>().SetOpponent(playerTwo);
            playerTwo.GetComponent<HitDetector>().SetOpponent(playerOne);
            playerOne.GetComponent<Combatant>().Opponent = playerTwo.GetComponent<Combatant>();
            playerTwo.GetComponent<Combatant>().Opponent = playerOne.GetComponent<Combatant>();
            playerOne.OnKnockedOut += FirstKnockedOut;
            playerTwo.OnKnockedOut += SecondKnockedOut;
        }

        private void Remove(Fighter fighter)
        {
            if (fighter == null) return;
            fighter.OnKnockedOut -= FirstKnockedOut;
            fighter.OnKnockedOut -= SecondKnockedOut;
            fighter.gameObject.SetActive(false);
            Destroy(fighter.gameObject);
        }

        private void FirstKnockedOut() => Finish(1);
        private void SecondKnockedOut() => Finish(0);

        private void Finish(int winner)
        {
            if (Phase != MatchPhase.Fighting) return;

            GanadorDelAsalto = winner;
            if (winner == 0) AsaltosGanadosP1++;
            else if (winner == 1) AsaltosGanadosP2++;

            bool hayCampeon = AsaltosGanadosP1 >= AsaltosParaGanar || AsaltosGanadosP2 >= AsaltosParaGanar;
            if (!hayCampeon && winner >= 0)
            {
                // Queda asalto: se repone a los dos y se anuncia el siguiente.
                NumeroDeAsalto++;
                SetCombat(false);
                PrepararAsalto(firstIndex, secondIndex);
                return;
            }

            if (AsaltosGanadosP1 > AsaltosGanadosP2) winner = 0;
            else if (AsaltosGanadosP2 > AsaltosGanadosP1) winner = 1;
            else winner = -1;

            Winner = winner;
            SetPhase(MatchPhase.Finished);
            SetCombat(false);
            if (winner >= 0)
            {
                var winning = winner == 0 ? playerOne.Data : playerTwo.Data;
                var losing = winner == 0 ? playerTwo.Data : playerOne.Data;
                WinQuote = winning.winQuotes[UnityEngine.Random.Range(0, winning.winQuotes.Length)];
                LoseQuote = losing.loseQuotes[UnityEngine.Random.Range(0, losing.loseQuotes.Length)];
            }
            Changed?.Invoke();
        }

        private void SetCombat(bool active)
        {
            foreach (var fighter in new[] { playerOne, playerTwo })
            {
                if (fighter == null) continue;
                fighter.GetComponent<HitDetector>().CombatEnabled = active;
                fighter.GetComponent<Combatant>().CombatEnabled = active;
                // Combatant centraliza el polling combinado de teclado y mandos.
                fighter.GetComponent<PlayerInputController>().enabled = false;
                if (!active) fighter.SetBlocking(false);
            }
        }

        public void ShowSelection()
        {
            Time.timeScale = 1;
            SetPhase(MatchPhase.Selection);
            Preview(firstIndex, secondIndex);
        }

        public void Rematch() => BeginMatch(firstIndex, secondIndex);

        public void TogglePause()
        {
            if (Phase != MatchPhase.Fighting && Phase != MatchPhase.Paused) return;
            bool paused = Phase == MatchPhase.Fighting;
            SetPhase(paused ? MatchPhase.Paused : MatchPhase.Fighting);
            Time.timeScale = paused ? 0 : 1;
            // La pausa congela el tiempo; desactivar combate cancelaria los specials en curso.
            if (paused)
            {
                playerOne.SetBlocking(false);
                playerTwo.SetBlocking(false);
            }
            Changed?.Invoke();
        }

        private void OnDestroy() => Time.timeScale = 1;
    }
}
