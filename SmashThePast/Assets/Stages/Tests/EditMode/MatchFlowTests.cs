using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SmashThePast.Characters;
using SmashThePast.Core;
using SmashThePast.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace SmashThePast.Stages.Tests
{
    public class MatchFlowTests
    {
        [UnityTest]
        public IEnumerator MatchPausePreservesAndResumesPoison()
        {
            EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;
            yield return null;
            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.AreEqual(MatchPhase.Selection, match.Phase);
            match.BeginMatch(6, 0);
            yield return EsperarAlAsalto(match);
            var source = match.PlayerOne.GetComponent<Combatant>();
            var target = match.PlayerTwo;
            target.GetComponent<CharacterController>().enabled = false;
            target.transform.position = source.transform.position + Vector3.right;
            target.GetComponent<CharacterController>().enabled = true;
            source.Fighter.Special.Fill(100);
            var targetCombatant = target.GetComponent<Combatant>();
            var generationField = typeof(Combatant).GetField("generation", BindingFlags.Instance | BindingFlags.NonPublic);
            int round = (int)generationField.GetValue(targetCombatant);
            Assert.IsTrue(source.TrySpecial());
            Assert.AreEqual(96, target.Health.CurrentHealth);
            match.TogglePause();
            yield return WaitFrames(0.6f);
            Assert.AreSame(source.Fighter, match.PlayerOne);
            Assert.AreEqual(MatchPhase.Paused, match.Phase);
            Assert.AreEqual(96, target.Health.CurrentHealth);
            match.TogglePause();
            Assert.IsTrue(source.CombatEnabled);
            yield return WaitFrames(0.6f);
            Assert.AreEqual(round, generationField.GetValue(targetCombatant), "La pausa cambio la generacion del rival");
            Assert.IsTrue((bool)typeof(Combatant).GetMethod("ValidTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(source, new object[] { targetCombatant, round, float.PositiveInfinity }), "El rival dejo de ser valido");
            Assert.AreEqual(91, target.Health.CurrentHealth);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator SelectionCombatPauseKnockoutAndRematchShareOneInterface()
        {
            EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;
            yield return null;
            var match = Object.FindFirstObjectByType<MatchController>();
            var ui = Object.FindFirstObjectByType<PrototypeUI>();
            var root = ui.GetComponent<UIDocument>().rootVisualElement;

            Assert.That(root.Q(className: "prototype-ui"), Is.Not.Null);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Selection));
            Assert.That(root.Query<Button>(className: "portrait-tile").ToList(), Has.Count.EqualTo(9));
            typeof(PrototypeUI).GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(ui, new object[] { 0, 1 });
            Assert.That(match.PlayerOne.Data, Is.EqualTo(match.Roster[1]));
            ui.SendMessage("ToggleReady", (object)0);
            ui.SendMessage("ToggleReady", 1);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundIntro), "el combate abre anunciando el asalto");
            yield return EsperarAlAsalto(match);
            Assert.That(root.Q(className: "hud").style.display.value, Is.EqualTo(DisplayStyle.Flex));

            var attacker = match.PlayerOne;
            var target = match.PlayerTwo;
            var controller = target.GetComponent<CharacterController>();
            controller.enabled = false;
            target.transform.position = attacker.transform.position + Vector3.right;
            controller.enabled = true;
            attacker.PerformLightAttack();
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(target.Data.maxHealth - attacker.LightAttackDamage));

            match.TogglePause();
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Paused));
            float remaining = match.SecondsRemaining;
            int health = target.Health.CurrentHealth;
            attacker.GetComponent<HitDetector>().ResetAttack();
            attacker.PerformHeavyAttack();
            yield return null;
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(health));
            Assert.That(match.SecondsRemaining, Is.EqualTo(remaining));
            Assert.That(root.Q<Label>(className: "result-title").text, Is.EqualTo("PAUSA"));
            ui.SendMessage("ActivatePrimary");
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Fighting));
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(health));

            // Un KO ya no termina el combate: gana el asalto. Al mejor de tres hacen
            // falta dos, y entre medias se repone a los dos y se anuncia el siguiente.
            target.ReceiveHit(1000);
            yield return null;
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundIntro), "un KO abre el asalto siguiente");
            Assert.That(match.AsaltosGanadosP1, Is.EqualTo(1));
            Assert.That(match.NumeroDeAsalto, Is.EqualTo(2));
            yield return EsperarAlAsalto(match);

            match.PlayerTwo.ReceiveHit(1000);
            yield return null;
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Finished));
            Assert.That(match.Winner, Is.EqualTo(0));
            Assert.That(match.AsaltosGanadosP1, Is.EqualTo(2));
            Assert.That(root.Q<Label>(className: "winner-quote").text, Is.Not.Empty);
            ui.SendMessage("ActivatePrimary");
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundIntro), "la revancha vuelve a anunciar el asalto 1");
            Assert.That(match.NumeroDeAsalto, Is.EqualTo(1));
            yield return EsperarAlAsalto(match);
            Assert.That(match.PlayerTwo.Health.CurrentHealth, Is.EqualTo(match.PlayerTwo.Data.maxHealth));
            Assert.That(match.SecondsRemaining, Is.GreaterThan(85));

            match.ShowSelection();
            yield return null;
            ui.SendMessage("ToggleReady", (object)0);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Selection));
            ui.SendMessage("ToggleReady", 1);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundIntro), "el combate abre anunciando el asalto");
            yield return EsperarAlAsalto(match);
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // Desde que hay asaltos, empezar un combate abre con el anuncio "ASALTO 1" y el
        // combate no arranca hasta que termina. Las pruebas que quieren pegar esperan.
        private static IEnumerator EsperarAlAsalto(MatchController match)
        {
            float limite = Time.realtimeSinceStartup + 5f;
            while (match.Phase == MatchPhase.RoundIntro && Time.realtimeSinceStartup < limite) yield return null;
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Fighting), "el anuncio del asalto no termino");
        }

        private static IEnumerator WaitFrames(float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
