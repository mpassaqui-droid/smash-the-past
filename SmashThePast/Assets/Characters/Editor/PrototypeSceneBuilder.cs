using System.Linq;
using SmashThePast.Core;
using SmashThePast.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace SmashThePast.Characters.Editor
{
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/Stages/PrototypeArena.unity";

        [MenuItem("Smash the Past/Generar prototipo")]
        public static void Build()
        {
            PrototypePrefabBuilder.Build();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var match = new GameObject("CombateLocal").AddComponent<MatchController>();
            var audio = match.gameObject.AddComponent<SmashThePast.Audio.PrototypeAudio>();
            var audioData = new SerializedObject(audio);
            audioData.FindProperty("match").objectReferenceValue = match;
            audioData.ApplyModifiedPropertiesWithoutUndo();
            var data = new SerializedObject(match);
            data.FindProperty("startWithSelection").boolValue = true;
            FillArray(data, "roster", PrototypePrefabBuilder.Names.Select(name => (Object)AssetDatabase.LoadAssetAtPath<CharacterData>($"Assets/Characters/{name}.asset")).ToArray());
            FillArray(data, "playerOnePrefabs", PrototypePrefabBuilder.Names.Select(name => (Object)LoadFighter(name, 1)).ToArray());
            FillArray(data, "playerTwoPrefabs", PrototypePrefabBuilder.Names.Select(name => (Object)LoadFighter(name, 2)).ToArray());
            for (int slot = 0; slot < 2; slot++)
            {
                var fighter = (GameObject)PrefabUtility.InstantiatePrefab(LoadFighter(PrototypePrefabBuilder.Names[slot], slot + 1).gameObject);
                fighter.transform.position = new Vector3(slot == 0 ? -2.4f : 2.4f, 0.05f, 0);
                data.FindProperty(slot == 0 ? "playerOne" : "playerTwo").objectReferenceValue = fighter.GetComponent<Fighter>();
            }
            var scenePlayerOne = GameObject.Find("TupacAmaruII_P1").GetComponent<Fighter>();
            var scenePlayerTwo = GameObject.Find("GenghisKhan_P2").GetComponent<Fighter>();
            scenePlayerOne.GetComponent<HitDetector>().SetOpponent(scenePlayerTwo);
            scenePlayerTwo.GetComponent<HitDetector>().SetOpponent(scenePlayerOne);
            data.ApplyModifiedPropertiesWithoutUndo();
            var cameraObject = new GameObject("Camara", typeof(Camera), typeof(AudioListener), typeof(ArenaCamera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 5.3f, -12);
            cameraObject.transform.rotation = Quaternion.Euler(15, 0, 0);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.34f, 0.52f, 0.56f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100;
            PrototypePrefabBuilder.SetObject(cameraObject.GetComponent<ArenaCamera>(), "match", match);
            AddInterface(match);
            var sun = new GameObject("LuzPrincipal").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.69f);
            RenderSettings.sun = sun;
            PrototypePrefabBuilder.Part(null, "SueloPrototipo", PrimitiveType.Cube, new Vector3(0, -0.3f, 0), new Vector3(20, 0.6f, 7), PrototypePrefabBuilder.Material("Piedra", new Color(0.43f, 0.47f, 0.49f)), true);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("PROTOTYPE_SCENE: 9 personajes, 18 prefabs, 2 Fighters, escena registrada.");
        }

        private static void AddInterface(MatchController match)
        {
            var panelAsset = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/PrototypePanelSettings.asset");
            if (panelAsset == null)
            {
                panelAsset = ScriptableObject.CreateInstance<PanelSettings>();
                panelAsset.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelAsset.referenceResolution = new Vector2Int(1600, 900);
                AssetDatabase.CreateAsset(panelAsset, "Assets/UI/PrototypePanelSettings.asset");
            }
            var uiObject = new GameObject("Interfaz", typeof(UIDocument), typeof(SmashThePast.UI.PrototypeUI));
            var document = uiObject.GetComponent<UIDocument>();
            document.panelSettings = panelAsset;
            var ui = uiObject.GetComponent<SmashThePast.UI.PrototypeUI>();
            var uiData = new SerializedObject(ui);
            uiData.FindProperty("match").objectReferenceValue = match;
            uiData.FindProperty("styleSheet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/PrototypeUI.uss");
            var portraits = CreatePortraits();
            SerializedProperty portraitArray = uiData.FindProperty("portraits");
            portraitArray.arraySize = portraits.Length;
            for (int index = 0; index < portraits.Length; index++)
                portraitArray.GetArrayElementAtIndex(index).objectReferenceValue = portraits[index];
            uiData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Texture2D[] CreatePortraits()
        {
            PrototypePrefabBuilder.EnsureFolder("Assets/Characters/Portraits");
            var portraits = new Texture2D[PrototypePrefabBuilder.Names.Length];
            for (int index = 0; index < portraits.Length; index++)
            {
                string path = $"Assets/Characters/Portraits/{PrototypePrefabBuilder.Names[index]}.asset";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null)
                {
                    texture = new Texture2D(160, 160, TextureFormat.RGBA32, false) { name = PrototypePrefabBuilder.Names[index] + "Portrait" };
                    Color[] pixels = new Color[160 * 160];
                    Color background = new Color(0.08f, 0.09f, 0.1f);
                    Color foreground = new[] {
                        new Color(0.8f, 0.16f, 0.17f), new Color(0.22f, 0.47f, 0.4f), new Color(0.35f, 0.3f, 0.55f),
                        new Color(0.92f, 0.92f, 0.87f), new Color(0.42f, 0.13f, 0.19f), new Color(0.16f, 0.27f, 0.43f),
                        new Color(0.1f, 0.6f, 0.49f), new Color(0.14f, 0.15f, 0.18f), new Color(0.77f, 0.84f, 0.87f)
                    }[index];
                    for (int y = 0; y < 160; y++)
                        for (int x = 0; x < 160; x++)
                        {
                            float dx = x - 80, dy = y - 82;
                            bool head = dx * dx / 2300 + dy * dy / 2900 < 1;
                            bool body = y > 104 && y < 158 && Mathf.Abs(x - 80) < 49 - (y - 104) * 0.15f;
                            bool eye = y > 75 && y < 84 && ((x > 57 && x < 68) || (x > 92 && x < 103));
                            pixels[y * 160 + x] = eye ? Color.white : head || body ? foreground : background;
                        }
                    texture.SetPixels(pixels);
                    texture.Apply();
                    AssetDatabase.CreateAsset(texture, path);
                }
                portraits[index] = texture;
            }
            return portraits;
        }

        public static Fighter LoadFighter(string name, int slot) => AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Characters/Prefabs/{name}_P{slot}.prefab").GetComponent<Fighter>();

        private static void FillArray(SerializedObject data, string name, Object[] values)
        {
            SerializedProperty array = data.FindProperty(name);
            array.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++) array.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }
    }
}
