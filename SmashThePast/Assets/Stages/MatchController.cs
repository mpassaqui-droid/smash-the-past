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
        public float SecondsRemaining { get; private set; } = 90;
        public int Winner { get; private set; } = -1;
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
            Time.timeScale = 1;
            Spawn(first, second);
            SecondsRemaining = 90;
            Winner = -1;
            WinQuote = LoseQuote = "";
            Phase = MatchPhase.Fighting;
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
            Winner = winner;
            Phase = MatchPhase.Finished;
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
                fighter.GetComponent<PlayerInputController>().enabled = active;
                if (!active) fighter.SetBlocking(false);
            }
        }

        public void ShowSelection()
        {
            Time.timeScale = 1;
            Phase = MatchPhase.Selection;
            Preview(firstIndex, secondIndex);
        }

        public void Rematch() => BeginMatch(firstIndex, secondIndex);

        public void TogglePause()
        {
            if (Phase != MatchPhase.Fighting && Phase != MatchPhase.Paused) return;
            bool paused = Phase == MatchPhase.Fighting;
            Phase = paused ? MatchPhase.Paused : MatchPhase.Fighting;
            Time.timeScale = paused ? 0 : 1;
            SetCombat(!paused);
            Changed?.Invoke();
        }

        private void OnDestroy() => Time.timeScale = 1;
    }
}
