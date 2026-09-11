using System;
using SmashThePast.Core;
using SmashThePast.Stages;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SmashThePast.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class PrototypeUI : MonoBehaviour
    {
        [SerializeField] public MatchController match;
        [SerializeField] public StyleSheet styleSheet;
        [SerializeField] public Texture2D[] portraits;

        private readonly int[] picks = { 0, 1 };
        private readonly bool[] ready = new bool[2];
        private readonly Gamepad[] pads = new Gamepad[2];
        private readonly Vector2Int[] held = new Vector2Int[2];
        private readonly float[] repeatAt = new float[2];
        private readonly Label[] pickNames = new Label[2], pickSpecials = new Label[2];
        private readonly Button[] pickButtons = new Button[2], readyButtons = new Button[2];
        private readonly Label[] hudNames = new Label[2], healthText = new Label[2], specialText = new Label[2];
        private readonly VisualElement[] healthFill = new VisualElement[2], specialFill = new VisualElement[2];
        private MatchController boundMatch;
        private VisualElement root, selection, grid, hud, overlay;
        private ScrollView resultScroll;
        private Label timer, resultTitle, winQuote, loseQuote;
        private Button primary, secondary;
        private Button[] tiles;
        private Label[] markers;
        private MatchPhase shownPhase = (MatchPhase)(-1);
        private int pointerPlayer, columns = 9, menuIndex;
        private int phaseFrame;

        private void OnEnable() => TryBuild();

        private void OnDisable()
        {
            if (boundMatch != null) boundMatch.Changed -= Refresh;
            boundMatch = null;
            root?.RemoveFromHierarchy();
            root = null;
            shownPhase = (MatchPhase)(-1);
            Array.Clear(pads, 0, pads.Length);
            Array.Clear(held, 0, held.Length);
        }

        private bool TryBuild()
        {
            if (root != null) return true;
            if (match == null || match.Roster == null || match.Roster.Length == 0) return false;
            var documentRoot = GetComponent<UIDocument>().rootVisualElement;
            if (documentRoot == null) return false;
            documentRoot.pickingMode = PickingMode.Ignore;
            root = Box(documentRoot, "prototype-ui");
            root.pickingMode = PickingMode.Ignore;
            if (styleSheet != null) root.styleSheets.Add(styleSheet);
            // La navegacion se procesa por jugador, sin el submit global de UI Toolkit.
            root.RegisterCallback<NavigationMoveEvent>(e => { e.PreventDefault(); e.StopImmediatePropagation(); }, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(e => { e.PreventDefault(); e.StopImmediatePropagation(); }, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationCancelEvent>(e => { e.PreventDefault(); e.StopImmediatePropagation(); }, TrickleDown.TrickleDown);
            BuildSelection();
            BuildHud();
            BuildOverlay();
            root.RegisterCallback<GeometryChangedEvent>(e => SetLayout(e.newRect.width));
            SetLayout(documentRoot.resolvedStyle.width);
            boundMatch = match;
            boundMatch.Changed += Refresh;
            Refresh();
            return true;
        }

        private void BuildSelection()
        {
            selection = Box(root, "selection");
            selection.pickingMode = PickingMode.Ignore;
            var header = Box(selection, "selection-header");
            Text(header, "SMASH THE PAST", "brand");
            Text(header, "ELIGE TU PERSONAJE", "eyebrow");
            var dock = Box(selection, "selection-dock");
            var players = Box(dock, "players");
            for (int p = 0; p < 2; p++)
            {
                int player = p;
                var side = Box(players, "player-choice p" + (p + 1));
                pickButtons[p] = ActionButton(side, "P" + (p + 1), () => SetPointerPlayer(player), "player-tab");
                var details = Box(side, "pick-details");
                pickNames[p] = Text(details, "", "pick-name");
                pickSpecials[p] = Text(details, "", "pick-special");
                readyButtons[p] = ActionButton(side, "LISTO", () => ToggleReady(player), "ready-button");
            }
            grid = Box(dock, "roster");
            int count = Mathf.Min(9, match.Roster.Length);
            tiles = new Button[count];
            markers = new Label[count];
            for (int i = 0; i < count; i++)
            {
                int index = i;
                tiles[i] = ActionButton(grid, "", () => Select(pointerPlayer, index), "portrait-tile");
                var portrait = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
                portrait.AddToClassList("portrait");
                if (portraits != null && i < portraits.Length) portrait.image = portraits[i];
                tiles[i].Add(portrait);
                var caption = Box(tiles[i], "tile-caption");
                caption.pickingMode = PickingMode.Ignore;
                Text(caption, Name(match.Roster[i]), "tile-name");
                markers[i] = Text(caption, "", "pick-marker");
                tiles[i].SetEnabled(match.Roster[i] != null);
            }
        }

        private void BuildHud()
        {
            hud = Box(root, "hud");
            hud.pickingMode = PickingMode.Ignore;
            for (int p = 0; p < 2; p++)
            {
                if (p == 1) timer = Text(hud, "90", "timer");
                var side = Box(hud, "hud-side p" + (p + 1));
                var names = Box(side, "hud-line");
                hudNames[p] = Text(names, "", "hud-name");
                healthText[p] = Text(names, "", "health-text");
                healthFill[p] = Box(Box(side, "health-track"), "health-fill");
                specialFill[p] = Box(Box(side, "special-track"), "special-fill");
                specialText[p] = Text(side, "", "special-status");
            }
        }

        private void BuildOverlay()
        {
            overlay = Box(root, "overlay");
            var panel = Box(overlay, "result-panel");
            resultScroll = new ScrollView(ScrollViewMode.Vertical);
            resultScroll.AddToClassList("result-scroll");
            panel.Add(resultScroll);
            resultTitle = Text(resultScroll.contentContainer, "", "result-title");
            winQuote = Text(resultScroll.contentContainer, "", "quote winner-quote");
            loseQuote = Text(resultScroll.contentContainer, "", "quote loser-quote");
            var actions = Box(panel, "result-actions");
            primary = ActionButton(actions, "", ActivatePrimary, "menu-button primary");
            secondary = ActionButton(actions, "ELEGIR PERSONAJES", () => match.ShowSelection(), "menu-button");
            primary.RegisterCallback<PointerEnterEvent>(_ => SetMenu(0));
            secondary.RegisterCallback<PointerEnterEvent>(_ => SetMenu(1));
        }

        private void SetLayout(float width)
        {
            columns = width < 1200 ? 3 : 9;
            root.EnableInClassList("compact", columns == 3);
        }

        private void Update()
        {
            if (!TryBuild()) return;
            if (match.Phase != shownPhase) Refresh();
            if (match.Phase == MatchPhase.Fighting || Time.frameCount == phaseFrame) return;
            AssignPads();
            for (int p = 0; p < 2; p++)
            {
                ReadKeyboard(p, out Vector2Int direction, out bool confirm);
                var pad = pads[p];
                if (pad != null)
                {
                    Vector2 axis = pad.dpad.ReadValue();
                    if (axis.sqrMagnitude < 0.2f) axis = pad.leftStick.ReadValue();
                    Vector2Int step = Vector2Int.zero;
                    if (Mathf.Max(Mathf.Abs(axis.x), Mathf.Abs(axis.y)) >= 0.55f)
                        step = Mathf.Abs(axis.x) >= Mathf.Abs(axis.y)
                            ? new Vector2Int(axis.x > 0 ? 1 : -1, 0)
                            : new Vector2Int(0, axis.y > 0 ? -1 : 1);
                    if (step != Vector2Int.zero && (step != held[p] || Time.unscaledTime >= repeatAt[p]))
                    {
                        direction = step;
                        repeatAt[p] = Time.unscaledTime + (step != held[p] ? 0.32f : 0.14f);
                    }
                    held[p] = step;
                    confirm |= pad.buttonSouth.wasPressedThisFrame;
                }
                if (match.Phase == MatchPhase.Selection)
                {
                    if (direction != Vector2Int.zero) MovePick(p, direction);
                    if (confirm) ToggleReady(p);
                }
                else if (match.Phase == MatchPhase.Paused || match.Phase == MatchPhase.Finished)
                {
                    if (direction != Vector2Int.zero) SetMenu(1 - menuIndex);
                    if (confirm)
                    {
                        if (menuIndex == 0) ActivatePrimary(); else match.ShowSelection();
                    }
                }
                if (Time.frameCount == phaseFrame) break;
            }
        }

        private void LateUpdate()
        {
            if (root == null || match == null || match.Phase != MatchPhase.Fighting) return;
            timer.text = Mathf.CeilToInt(match.SecondsRemaining).ToString("00");
            timer.EnableInClassList("urgent", match.SecondsRemaining <= 10);
            UpdateFighter(0, match.PlayerOne);
            UpdateFighter(1, match.PlayerTwo);
        }

        private void AssignPads()
        {
            for (int p = 0; p < 2; p++)
            {
                if (pads[p] != null && pads[p].added) continue;
                pads[p] = null;
                held[p] = Vector2Int.zero;
                foreach (var pad in Gamepad.all)
                    if (pad != pads[1 - p]) { pads[p] = pad; break; }
            }
        }

        private static void ReadKeyboard(int player, out Vector2Int direction, out bool confirm)
        {
            direction = Vector2Int.zero;
            confirm = false;
            var k = Keyboard.current;
            if (k == null) return;
            if (player == 0)
            {
                if (k.aKey.wasPressedThisFrame) direction.x = -1;
                if (k.dKey.wasPressedThisFrame) direction.x = 1;
                if (k.wKey.wasPressedThisFrame) direction.y = -1;
                if (k.sKey.wasPressedThisFrame) direction.y = 1;
                confirm = k.jKey.wasPressedThisFrame;
            }
            else
            {
                if (k.leftArrowKey.wasPressedThisFrame) direction.x = -1;
                if (k.rightArrowKey.wasPressedThisFrame) direction.x = 1;
                if (k.upArrowKey.wasPressedThisFrame) direction.y = -1;
                if (k.downArrowKey.wasPressedThisFrame) direction.y = 1;
                confirm = k.numpad1Key.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
            }
        }

        private void MovePick(int player, Vector2Int direction)
        {
            if (ready[player]) return;
            int delta = direction.x != 0 ? direction.x : direction.y * (columns == 9 ? 1 : columns);
            int index = picks[player];
            for (int n = 0; n < tiles.Length; n++)
            {
                index = (index + delta + tiles.Length) % tiles.Length;
                if (match.Roster[index] != null) { Select(player, index); break; }
            }
        }

        private void SetPointerPlayer(int player)
        {
            pointerPlayer = player;
            RefreshPicks();
        }

        private void Select(int player, int index)
        {
            if (match.Phase != MatchPhase.Selection || ready[player] || match.Roster[index] == null) return;
            pointerPlayer = player;
            if (picks[player] != index)
            {
                picks[player] = index;
                match.Preview(picks[0], picks[1]);
            }
            RefreshPicks();
        }

        private void ToggleReady(int player)
        {
            if (match.Phase != MatchPhase.Selection || match.Roster[picks[player]] == null) return;
            ready[player] = !ready[player];
            pointerPlayer = ready[player] ? 1 - player : player;
            RefreshPicks();
            if (ready[0] && ready[1]) match.BeginMatch(picks[0], picks[1]);
        }

        private void Refresh()
        {
            if (root == null || match == null) return;
            bool changedPhase = shownPhase != match.Phase;
            shownPhase = match.Phase;
            if (changedPhase) phaseFrame = Time.frameCount;
            Show(selection, shownPhase == MatchPhase.Selection);
            Show(hud, shownPhase == MatchPhase.Fighting);
            Show(overlay, shownPhase == MatchPhase.Paused || shownPhase == MatchPhase.Finished);
            if (shownPhase == MatchPhase.Selection)
            {
                if (changedPhase)
                {
                    for (int p = 0; p < 2; p++)
                    {
                        ready[p] = false;
                        var fighter = p == 0 ? match.PlayerOne : match.PlayerTwo;
                        int index = fighter != null ? Array.IndexOf(match.Roster, fighter.Data) : picks[p];
                        picks[p] = Mathf.Clamp(index, 0, tiles.Length - 1);
                    }
                    pointerPlayer = 0;
                }
                RefreshPicks();
            }
            else if (shownPhase == MatchPhase.Paused || shownPhase == MatchPhase.Finished)
            {
                bool paused = shownPhase == MatchPhase.Paused;
                primary.text = paused ? "CONTINUAR" : "REVANCHA";
                resultTitle.text = paused ? "PAUSA" : match.Winner < 0 ? "EMPATE" :
                    "GANA P" + (match.Winner + 1) + "\n" + Name((match.Winner == 0 ? match.PlayerOne : match.PlayerTwo)?.Data);
                winQuote.text = match.WinQuote ?? "";
                loseQuote.text = match.LoseQuote ?? "";
                Show(winQuote, !paused && !string.IsNullOrEmpty(winQuote.text));
                Show(loseQuote, !paused && !string.IsNullOrEmpty(loseQuote.text));
                if (changedPhase) { SetMenu(0); resultScroll.scrollOffset = Vector2.zero; }
            }
        }

        private void RefreshPicks()
        {
            for (int p = 0; p < 2; p++)
            {
                var data = match.Roster[picks[p]];
                pickNames[p].text = Name(data);
                pickSpecials[p].text = data != null ? data.specialName : "";
                readyButtons[p].text = ready[p] ? "LISTO / CAMBIAR" : "LISTO";
                readyButtons[p].EnableInClassList("is-ready", ready[p]);
                pickButtons[p].EnableInClassList("active-player", pointerPlayer == p);
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                bool first = picks[0] == i, second = picks[1] == i;
                tiles[i].EnableInClassList("picked-p1", first);
                tiles[i].EnableInClassList("picked-p2", second);
                markers[i].text = first && second ? "P1 + P2" : first ? "P1" : second ? "P2" : "";
            }
        }

        private void UpdateFighter(int p, Fighter fighter)
        {
            hudNames[p].text = "P" + (p + 1) + "  " + Name(fighter != null ? fighter.Data : null);
            int health = fighter?.Health?.CurrentHealth ?? 0;
            int max = fighter?.Health?.MaxHealth ?? 1;
            healthText[p].text = health + " / " + max;
            healthFill[p].style.width = Length.Percent(100f * health / Mathf.Max(1, max));
            specialFill[p].style.width = Length.Percent(100f * (fighter?.Special?.CurrentValue ?? 0) / SpecialMeter.MaxValue);
            bool charged = fighter?.Special?.IsReady ?? false;
            specialText[p].text = (fighter != null && fighter.Data != null ? fighter.Data.specialName : "") + (charged ? "  /  LISTO" : "");
            specialText[p].EnableInClassList("special-ready", charged);
        }

        private void SetMenu(int index)
        {
            menuIndex = index;
            primary.EnableInClassList("menu-selected", index == 0);
            secondary.EnableInClassList("menu-selected", index == 1);
        }

        private void ActivatePrimary()
        {
            if (match.Phase == MatchPhase.Paused) match.TogglePause();
            else if (match.Phase == MatchPhase.Finished) match.Rematch();
        }

        private static string Name(CharacterData data) => data != null ? data.characterName : "";
        private static void Show(VisualElement element, bool visible) => element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        private static VisualElement Box(VisualElement parent, string classes)
        {
            var element = new VisualElement();
            foreach (string c in classes.Split(' ')) element.AddToClassList(c);
            parent.Add(element);
            return element;
        }

        private static Label Text(VisualElement parent, string value, string classes)
        {
            var label = new Label(value) { pickingMode = PickingMode.Ignore, enableRichText = false };
            foreach (string c in classes.Split(' ')) label.AddToClassList(c);
            parent.Add(label);
            return label;
        }

        private static Button ActionButton(VisualElement parent, string value, Action action, string classes)
        {
            var button = new Button(action) { text = value, focusable = false };
            foreach (string c in classes.Split(' ')) button.AddToClassList(c);
            parent.Add(button);
            return button;
        }
    }
}
