using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.Characters.Editor
{
    public static class PrototypeStageBuilder
    {
        private const string ArtPath = "Assets/Stages/Art";
        private const string PrefabPath = ArtPath + "/PatioAndino.prefab";

        [MenuItem("Smash the Past/Actualizar patio andino")]
        public static void ApplyToPrototype()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath);
            AddScenery();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static void AddScenery()
        {
            PrototypePrefabBuilder.EnsureFolder(ArtPath);
            var root = new GameObject("PatioAndino");
            try
            {
                Material stone = Material("PiedraClara", new Color(0.48f, 0.53f, 0.52f));
                Material darkStone = Material("PiedraOscura", new Color(0.31f, 0.38f, 0.38f));
                Material paving = Material("Losas", new Color(0.62f, 0.66f, 0.63f));
                Material grass = Material("Pasto", new Color(0.27f, 0.47f, 0.24f));
                Material nearHill = Material("MontanaCercana", new Color(0.22f, 0.40f, 0.31f));
                Material farHill = Material("MontanaLejana", new Color(0.39f, 0.56f, 0.57f));
                Material red = Material("TejidoRojo", new Color(0.73f, 0.09f, 0.12f));
                Material gold = Material("TejidoOcre", new Color(0.91f, 0.65f, 0.22f));
                Material white = Material("TejidoBlanco", new Color(0.89f, 0.90f, 0.83f));
                Transform floor = Group(root.transform, "Pavimento");
                for (int row = 0; row < 6; row++)
                    for (int column = 0; column < 20; column++)
                        Block(floor, "Losa", new Vector3(column - 9.5f, -0.018f, row - 2.5f),
                            new Vector3(0.97f, 0.04f, 0.97f), (row + column) % 4 == 0 ? stone : paving);

                Transform terraces = Group(root.transform, "Terrazas");
                for (int tier = 0; tier < 3; tier++)
                {
                    float z = 4.4f + tier * 1.9f;
                    float top = 0.55f + tier * 0.7f;
                    Block(terraces, "Anden", new Vector3(0, top / 2 - 0.1f, z), new Vector3(27, top + 0.2f, 1.85f), darkStone);
                    Block(terraces, "Pasto", new Vector3(0, top + 0.025f, z + 0.1f), new Vector3(27, 0.08f, 1.6f), grass);
                    for (int column = 0; column < 23; column++)
                        Block(terraces, "Mamposteria", new Vector3((column - 11) * 1.17f, top - 0.27f, z - 0.94f),
                            new Vector3(1.13f, 0.52f, 0.2f), column % 3 == 0 ? darkStone : stone);
                }

                Transform hills = Group(root.transform, "Cordillera");
                Mesh mesh = MountainMesh();
                for (int index = 0; index < 9; index++)
                {
                    bool distant = index < 5;
                    int slot = distant ? index : index - 5;
                    var hill = new GameObject("Cumbre", typeof(MeshFilter), typeof(MeshRenderer));
                    hill.transform.SetParent(hills, false);
                    hill.transform.localPosition = new Vector3((slot - (distant ? 2 : 1.5f)) * 9, 0, distant ? 25 : 15);
                    hill.transform.localScale = new Vector3(7 + index % 3, (distant ? 15 : 9) + index % 3 * 1.7f, 5);
                    hill.transform.localRotation = Quaternion.Euler(0, index * 39, 0);
                    hill.GetComponent<MeshFilter>().sharedMesh = mesh;
                    hill.GetComponent<MeshRenderer>().sharedMaterial = distant ? farHill : nearHill;
                }

                Transform banners = Group(root.transform, "Estandartes");
                foreach (int side in new[] { -1, 1 })
                {
                    float x = side * 7;
                    Block(banners, "Pedestal", new Vector3(x, 0.4f, 3.8f), new Vector3(1, 0.8f, 1), stone);
                    Block(banners, "Poste", new Vector3(x, 2.9f, 3.8f), new Vector3(0.13f, 4.3f, 0.13f), darkStone);
                    Block(banners, "Travesano", new Vector3(x, 4.95f, 3.8f), new Vector3(1.8f, 0.12f, 0.12f), gold);
                    Block(banners, "Tejido", new Vector3(x, 3.85f, 3.72f), new Vector3(1.5f, 2.1f, 0.06f), red);
                    foreach (float y in new[] { 2.9f, 3.05f, 4.65f, 4.8f })
                        Block(banners, "Franja", new Vector3(x, y, 3.675f), new Vector3(1.5f, 0.06f, 0.025f), white);
                    var diamond = Block(banners, "Rombo", new Vector3(x, 3.85f, 3.66f), new Vector3(0.59f, 0.59f, 0.025f), gold);
                    diamond.transform.localRotation = Quaternion.Euler(0, 0, 45);
                }

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                // Reemplazar solo la instancia propia, sin regenerar Fighters ni el suelo fisico.
                foreach (GameObject candidate in root.scene.GetRootGameObjects())
                    if (candidate != root && PrefabUtility.GetCorrespondingObjectFromSource(candidate) == prefab)
                        Object.DestroyImmediate(candidate);
                PrefabUtility.InstantiatePrefab(prefab);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform Group(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static GameObject Block(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
            => PrototypePrefabBuilder.Part(parent, name, PrimitiveType.Cube, position, scale, material);

        private static Material Material(string name, Color color)
        {
            string path = $"{ArtPath}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Glossiness", 0);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh MountainMesh()
        {
            string path = ArtPath + "/Cumbre.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var peak = new Vector3(0.12f, 1, 0.07f);
            for (int face = 0; face < 7; face++)
            {
                float a = face * Mathf.PI * 2 / 7;
                float b = (face + 1) * Mathf.PI * 2 / 7;
                Vector3 left = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Vector3 right = new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b));
                Vector3 shoulder = (left + right) * 0.36f + Vector3.up * (0.36f + face % 2 * 0.13f);
                foreach (Vector3 point in new[] { left, peak, shoulder, shoulder, peak, right, left, shoulder, right })
                {
                    triangles.Add(vertices.Count);
                    vertices.Add(point);
                }
            }
            mesh = new Mesh { name = "Cumbre" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
