using NUnit.Framework;
using SmashThePast.Audio;
using SmashThePast.Characters;
using SmashThePast.Core;
using SmashThePast.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace SmashThePast.Stages.Tests
{
    public class PrototypeSceneTests
    {
        [Test]
        public void PrototypeSceneHasConfiguredMatchCameraFloorAndUI()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            var match = Object.FindFirstObjectByType<MatchController>();
            var camera = Object.FindFirstObjectByType<ArenaCamera>();
            var ui = Object.FindFirstObjectByType<PrototypeUI>();
            var floor = GameObject.Find("SueloPrototipo");
            Assert.That(scene.IsValid, Is.True);
            Assert.That(match, Is.Not.Null);
            Assert.That(match.Roster, Has.Length.EqualTo(9));
            Assert.That(match.PlayerOne, Is.Not.Null);
            Assert.That(match.PlayerTwo, Is.Not.Null);
            Assert.That(match.PlayerOne.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(match.PlayerTwo.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(match.PlayerOne.GetComponent<Fighter>(), Is.Not.Null);
            Assert.That(match.PlayerTwo.GetComponent<Fighter>(), Is.Not.Null);
            Assert.That(match.PlayerOne.GetComponent<PlayerInputController>(), Is.Not.Null);
            Assert.That(match.PlayerTwo.GetComponent<PlayerInputController>(), Is.Not.Null);
            Assert.That(match.PlayerOne.GetComponent<HitDetector>().Opponent, Is.EqualTo(match.PlayerTwo));
            Assert.That(match.PlayerTwo.GetComponent<HitDetector>().Opponent, Is.EqualTo(match.PlayerOne));
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.GetComponent<Camera>().orthographic, Is.True);
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.GetComponent<UIDocument>().panelSettings, Is.Not.Null);
            Assert.That(floor, Is.Not.Null);
            Assert.That(floor.GetComponent<Collider>(), Is.Not.Null);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void EveryBaseCharacterHasTwoPlayerPrefabsAndVisuals()
        {
            string[] names = { "TupacAmaruII", "GenghisKhan", "MiyamotoMusashi", "Gandhi", "Rasputin", "NapoleonBonaparte", "Cleopatra", "AbrahamLincoln", "Socrates" };
            foreach (string name in names)
                for (int slot = 1; slot <= 2; slot++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Characters/Prefabs/{name}_P{slot}.prefab");
                    Assert.That(prefab, Is.Not.Null, name + " P" + slot);
                    Assert.That(prefab.GetComponent<FighterVisual>(), Is.Not.Null, name + " visual");
                    Assert.That(prefab.GetComponent<Combatant>(), Is.Not.Null, name + " special");
                    Assert.That(prefab.GetComponent<SpecialVisual>(), Is.Not.Null, name + " effect");
                }
            Assert.That(AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Characters/Portraits" }), Has.Length.EqualTo(9));
        }

        [Test]
        public void PrototypeSceneHasCombatAudioAndImpactFeedback()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            var audio = Object.FindFirstObjectByType<PrototypeAudio>();
            var feedback = Object.FindFirstObjectByType<CombatFeedback>();
            Assert.That(scene.IsValid, Is.True);
            Assert.That(audio, Is.Not.Null);
            Assert.That(audio.GetComponent<AudioSource>(), Is.Not.Null);
            Assert.That(feedback, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CameraShake>(), Is.Null);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }
}
