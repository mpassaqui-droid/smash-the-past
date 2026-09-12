using SmashThePast.Core;
using SmashThePast.Stages;
using UnityEngine;

namespace SmashThePast.UI
{
    public sealed class PrototypeUIFallback : MonoBehaviour
    {
        [SerializeField] public MatchController match;
        private readonly int[] picks = { 0, 1 };
        private readonly bool[] ready = new bool[2];
        private GUIStyle title, label, button, small;

        private void OnGUI()
        {
            if (match == null || match.Roster == null || match.Roster.Length == 0) return;
            EnsureStyles();
            if (match.Phase == MatchPhase.Selection) DrawSelection();
            else if (match.Phase == MatchPhase.Fighting) DrawHud();
            else { DrawHud(); DrawResult(); }
        }

        private void DrawSelection()
        {
            float width = Screen.width, height = Screen.height;
            GUI.Box(new Rect(0, 0, width, height), GUIContent.none);
            GUI.Label(new Rect(24, 18, width - 48, 42), "SMASH THE PAST", title);
            GUI.Label(new Rect(24, 58, width - 48, 24), "ELIGE TU PERSONAJE. P1: A/D/W/S + J. P2: FLECHAS + ENTER.", small);
            float margin = 24, top = 102, gap = 10;
            float tileWidth = (width - margin * 2 - gap * 8) / 9f;
            float tileHeight = Mathf.Min(230, height * 0.43f);
            for (int index = 0; index < Mathf.Min(9, match.Roster.Length); index++)
            {
                float x = margin + index * (tileWidth + gap);
                string text = match.Roster[index].characterName + "\n\n" + match.Roster[index].specialName;
                if (GUI.Button(new Rect(x, top, tileWidth, tileHeight), text, button)) Select(index);
                if (picks[0] == index) GUI.Label(new Rect(x + 4, top + tileHeight - 28, tileWidth - 8, 22), "P1", label);
                if (picks[1] == index) GUI.Label(new Rect(x + tileWidth - 34, top + tileHeight - 28, 30, 22), "P2", label);
            }
            float panelTop = top + tileHeight + 24;
            GUI.Box(new Rect(margin, panelTop, width - margin * 2, height - panelTop - 24), GUIContent.none);
            GUI.Label(new Rect(margin + 18, panelTop + 14, width * 0.42f, 30), "P1  " + Name(picks[0]), label);
            GUI.Label(new Rect(width * 0.52f, panelTop + 14, width * 0.42f, 30), "P2  " + Name(picks[1]), label);
            if (GUI.Button(new Rect(margin + 18, panelTop + 54, 180, 42), "P1 LISTO", button)) ToggleReady(0);
            if (GUI.Button(new Rect(width - margin - 198, panelTop + 54, 180, 42), "P2 LISTO", button)) ToggleReady(1);
        }

        private void DrawHud()
        {
            float width = Screen.width;
            GUI.Box(new Rect(18, 16, width - 36, 106), GUIContent.none);
            DrawFighterHud(match.PlayerOne, new Rect(34, 30, width * 0.38f, 80), new Color(0.95f, 0.25f, 0.2f));
            DrawFighterHud(match.PlayerTwo, new Rect(width * 0.58f, 30, width * 0.38f, 80), new Color(0.2f, 0.85f, 0.75f));
            GUI.Label(new Rect(width * 0.45f, 34, width * 0.1f, 36), Mathf.CeilToInt(match.SecondsRemaining).ToString("00"), title);
        }

        private void DrawFighterHud(Fighter fighter, Rect area, Color accent)
        {
            if (fighter == null) return;
            GUI.Label(new Rect(area.x, area.y, area.width, 25), fighter.Data.characterName, label);
            int health = fighter.Health.CurrentHealth, max = Mathf.Max(1, fighter.Health.MaxHealth);
            GUI.Box(new Rect(area.x, area.y + 28, area.width, 18), GUIContent.none);
            GUI.color = accent;
            GUI.Box(new Rect(area.x, area.y + 28, area.width * health / max, 18), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(area.x, area.y + 50, area.width, 24), health + " / " + max + "    " + fighter.Data.specialName, small);
        }

        private void DrawResult()
        {
            float width = Screen.width, height = Screen.height;
            GUI.Box(new Rect(width * 0.25f, height * 0.28f, width * 0.5f, height * 0.4f), GUIContent.none);
            string result = match.Winner < 0 ? "EMPATE" : "GANA P" + (match.Winner + 1) + "  " + Name(match.Winner == 0 ? match.PlayerOne.Data : match.PlayerTwo.Data);
            GUI.Label(new Rect(width * 0.28f, height * 0.33f, width * 0.44f, 55), result, title);
            GUI.Label(new Rect(width * 0.29f, height * 0.43f, width * 0.42f, 80), match.WinQuote + "\n\n" + match.LoseQuote, label);
            if (GUI.Button(new Rect(width * 0.34f, height * 0.58f, width * 0.14f, 42), "REVANCHA", button)) match.Rematch();
            if (GUI.Button(new Rect(width * 0.52f, height * 0.58f, width * 0.14f, 42), "ELEGIR", button)) match.ShowSelection();
        }

        private void Select(int index)
        {
            int player = ready[0] ? 1 : 0;
            if (!ready[player]) { picks[player] = index; match.Preview(picks[0], picks[1]); }
        }

        private void ToggleReady(int player)
        {
            ready[player] = !ready[player];
            if (ready[0] && ready[1]) match.BeginMatch(picks[0], picks[1]);
        }

        private string Name(int index) => index >= 0 && index < match.Roster.Length ? match.Roster[index].characterName : "";
        private static string Name(CharacterData data) => data != null ? data.characterName : "";

        private void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            label = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            small = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { fontSize = 13, wordWrap = true };
        }
    }
}
