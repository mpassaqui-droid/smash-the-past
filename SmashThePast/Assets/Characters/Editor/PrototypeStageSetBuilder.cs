using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.Characters.Editor
{
    public static class PrototypeStageSetBuilder
    {
        private const string ThemePath = "Assets/Stages/Art/Themes";
        private const string BasePrefab = "Assets/Stages/Art/PatioAndino.prefab";
        private static readonly string[] Names = {
            "PatioAndino", "CampamentoMongol", "DojoMusashi", "AshramGandhi", "PalacioRasputin",
            "CampamentoNapoleon", "PalacioCleopatra", "LincolnSquare", "AgoraSocrates"
        };
        private static readonly Color[] Grounds = {
            new Color(0.27f, 0.47f, 0.24f), new Color(0.31f, 0.43f, 0.2f), new Color(0.28f, 0.34f, 0.25f),
            new Color(0.34f, 0.53f, 0.3f), new Color(0.22f, 0.32f, 0.22f), new Color(0.42f, 0.46f, 0.28f),
            new Color(0.47f, 0.57f, 0.28f), new Color(0.22f, 0.34f, 0.35f), new Color(0.38f, 0.48f, 0.3f)
        };
        private static readonly Color[] Stones = {
            new Color(0.48f, 0.53f, 0.52f), new Color(0.39f, 0.42f, 0.35f), new Color(0.35f, 0.39f, 0.42f),
            new Color(0.54f, 0.52f, 0.42f), new Color(0.34f, 0.35f, 0.34f), new Color(0.5f, 0.45f, 0.36f),
            new Color(0.58f, 0.48f, 0.34f), new Color(0.39f, 0.44f, 0.48f), new Color(0.48f, 0.46f, 0.38f)
        };
        private static readonly Color[] Fabrics = {
            new Color(0.73f, 0.09f, 0.12f), new Color(0.56f, 0.18f, 0.09f), new Color(0.18f, 0.34f, 0.48f),
            new Color(0.78f, 0.54f, 0.16f), new Color(0.42f, 0.1f, 0.16f), new Color(0.16f, 0.24f, 0.48f),
            new Color(0.1f, 0.46f, 0.45f), new Color(0.55f, 0.13f, 0.16f), new Color(0.25f, 0.25f, 0.48f)
        };

        public static void AddToScene()
        {
            BuildThemes();
            var oldAndes = GameObject.Find("PatioAndino");
            if (oldAndes != null) UnityEngine.Object.DestroyImmediate(oldAndes);
            var oldSet = GameObject.Find("Escenografia");
            if (oldSet != null) UnityEngine.Object.DestroyImmediate(oldSet);
            var holder = new GameObject("Escenografia");
            var controller = holder.AddComponent<SmashThePast.Stages.StageSetController>();
            var match = GameObject.Find("CombateLocal")?.GetComponent<SmashThePast.Stages.MatchController>();
            PrototypePrefabBuilder.SetObject(controller, "match", match);
            var serialized = new SerializedObject(controller);
            var array = serialized.FindProperty("stages");
            array.arraySize = Names.Length;
            for (int index = 0; index < Names.Length; index++)
            {
                string prefabPath = index == 0 ? BasePrefab : $"{ThemePath}/{Names[index]}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) throw new InvalidOperationException("Falta el prefab de escenario: " + prefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder.transform);
                instance.name = Names[index];
                instance.SetActive(index == 0);
                array.GetArrayElementAtIndex(index).objectReferenceValue = instance;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildThemes()
        {
            PrototypePrefabBuilder.EnsureFolder(ThemePath);
            for (int index = 1; index < Names.Length; index++)
            {
                string sourcePath = BasePrefab;
                string outputPath = $"{ThemePath}/{Names[index]}.prefab";
                var root = PrefabUtility.LoadPrefabContents(sourcePath);
                try
                {
                    root.name = Names[index];
                    Material ground = ThemeMaterial(index, "Suelo");
                    Material stone = ThemeMaterial(index, "Piedra");
                    Material fabric = ThemeMaterial(index, "Tela");
                    Material accent = ThemeMaterial(index, "Acento");
                    var paving = root.transform.Find("Pavimento");
                    var terraces = root.transform.Find("Terrazas");
                    var hills = root.transform.Find("Cordillera");
                    var banners = root.transform.Find("Estandartes");
                    Assign(paving, ground);
                    Assign(terraces, stone);
                    Assign(hills, Mix(Stones[index], Grounds[index], 0.45f, index, "Montana"));
                    Assign(banners, fabric);
                    AddProps(root.transform, index, accent, stone);
                    PrefabUtility.SaveAsPrefabAsset(root, outputPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void AddProps(Transform parent, int theme, Material accent, Material stone)
        {
            var props = new GameObject("Identidad" ).transform;
            props.SetParent(parent, false);
            if (theme == 1)
            {
                Primitive(props, "Yurta", PrimitiveType.Cylinder, new Vector3(0, 1.2f, 8), new Vector3(3.2f, 1.2f, 2.2f), stone);
                Primitive(props, "Asta", PrimitiveType.Cylinder, new Vector3(0, 3.6f, 8), new Vector3(0.1f, 2.4f, 0.1f), accent);
            }
            else if (theme == 2)
            {
                Torii(props, -4.8f, accent);
                Torii(props, 4.8f, accent);
            }
            else if (theme == 3)
            {
                for (int side = -1; side <= 1; side += 2)
                    Primitive(props, "Columna", PrimitiveType.Cylinder, new Vector3(side * 4.8f, 1.5f, 7), new Vector3(0.65f, 1.5f, 0.65f), stone);
            }
            else if (theme == 4 || theme == 5)
            {
                Primitive(props, "Torre", PrimitiveType.Cube, new Vector3(-6.2f, 1.5f, 7), new Vector3(1.2f, 3, 1.2f), stone);
                Primitive(props, "Torre", PrimitiveType.Cube, new Vector3(6.2f, 1.5f, 7), new Vector3(1.2f, 3, 1.2f), stone);
            }
            else if (theme == 6)
            {
                Primitive(props, "Obelisco", PrimitiveType.Cube, new Vector3(0, 2.3f, 8), new Vector3(0.75f, 4.6f, 0.75f), accent);
            }
            else if (theme == 7)
            {
                Primitive(props, "Columna", PrimitiveType.Cylinder, new Vector3(-5.8f, 1.5f, 7), new Vector3(0.8f, 1.5f, 0.8f), stone);
                Primitive(props, "Columna", PrimitiveType.Cylinder, new Vector3(5.8f, 1.5f, 7), new Vector3(0.8f, 1.5f, 0.8f), stone);
            }
            else
            {
                Primitive(props, "Podio", PrimitiveType.Cube, new Vector3(0, 0.7f, 7), new Vector3(2.8f, 1.4f, 1), stone);
            }
        }

        private static void Torii(Transform parent, float x, Material material)
        {
            Primitive(parent, "Pilar", PrimitiveType.Cube, new Vector3(x - 1.1f, 1.7f, 7), new Vector3(0.35f, 3.4f, 0.35f), material);
            Primitive(parent, "Pilar", PrimitiveType.Cube, new Vector3(x + 1.1f, 1.7f, 7), new Vector3(0.35f, 3.4f, 0.35f), material);
            Primitive(parent, "Viga", PrimitiveType.Cube, new Vector3(x, 3.3f, 7), new Vector3(2.8f, 0.3f, 0.4f), material);
        }

        private static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
            => PrototypePrefabBuilder.Part(parent, name, type, position, scale, material);

        private static void Assign(Transform group, Material material)
        {
            if (group == null) return;
            foreach (var renderer in group.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterial = material;
        }

        private static Material ThemeMaterial(int index, string suffix)
            => Mix(index == 0 ? Stones[0] : suffix == "Tela" ? Fabrics[index] : Stones[index],
                suffix == "Suelo" ? Grounds[index] : suffix == "Montana" ? Grounds[index] : Color.white,
                suffix == "Suelo" || suffix == "Montana" ? 0.35f : 0, index, suffix);

        private static Material Mix(Color primary, Color secondary, float amount, int index, string suffix)
        {
            string path = $"{ThemePath}/{Names[index]}_{suffix}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = Names[index] + suffix };
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = Color.Lerp(primary, secondary, amount);
            material.SetFloat("_Glossiness", 0);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
