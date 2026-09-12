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
        public void AndeanSceneryHasMeshesAndDoesNotChangeTheCollisionFloor()
        {
            EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            try
            {
                var scenery = GameObject.Find("PatioAndino");
                Assert.That(scenery, Is.Not.Null);
                Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(scenery),
                    Is.EqualTo("Assets/Stages/Art/PatioAndino.prefab"));
                Assert.That(scenery.GetComponentsInChildren<Collider>(), Is.Empty);
                foreach (string group in new[] { "Pavimento", "Terrazas", "Cordillera", "Estandartes" })
                    Assert.That(scenery.transform.Find(group), Is.Not.Null, group);
                var renderers = scenery.GetComponentsInChildren<MeshRenderer>();
                Assert.That(renderers.Length, Is.GreaterThan(0));
                foreach (var renderer in renderers)
                {
                    Assert.That(renderer.sharedMaterial, Is.Not.Null, renderer.name);
                    Assert.That(renderer.sharedMaterial.shader, Is.Not.Null, renderer.name);
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    Assert.That(mesh, Is.Not.Null, renderer.name);
                    Assert.That(mesh.vertexCount, Is.GreaterThan(0), renderer.name);
                }
                var floor = GameObject.Find("SueloPrototipo");
                Assert.That(floor.transform.position, Is.EqualTo(new Vector3(0, -0.3f, 0)));
                Assert.That(floor.transform.localScale, Is.EqualTo(new Vector3(20, 0.6f, 7)));
                Assert.That(floor.GetComponent<BoxCollider>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

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
            var panel = new SerializedObject(ui.GetComponent<UIDocument>().panelSettings);
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/PrototypeTheme.tss");
            Assert.That(theme, Is.Not.Null);
            Assert.That(panel.FindProperty("themeUss").objectReferenceValue, Is.EqualTo(theme));
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
