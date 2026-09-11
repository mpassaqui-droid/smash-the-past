using SmashThePast.Core;
using UnityEditor;
using UnityEngine;

namespace SmashThePast.Characters.Editor
{
    public static class PrototypePrefabBuilder
    {
        public static readonly string[] Names = { "TupacAmaruII", "GenghisKhan", "MiyamotoMusashi", "Gandhi", "Rasputin", "NapoleonBonaparte", "Cleopatra", "AbrahamLincoln", "Socrates" };
        private static readonly Color[] Colors = {
            new Color(0.8f, 0.16f, 0.17f), new Color(0.22f, 0.47f, 0.4f), new Color(0.35f, 0.3f, 0.55f),
            new Color(0.92f, 0.92f, 0.87f), new Color(0.42f, 0.13f, 0.19f), new Color(0.16f, 0.27f, 0.43f),
            new Color(0.1f, 0.6f, 0.49f), new Color(0.14f, 0.15f, 0.18f), new Color(0.77f, 0.84f, 0.87f)
        };

        public static void Build()
        {
            EnsureFolder("Assets/Characters/Prefabs");
            EnsureFolder("Assets/Characters/Materials");
            EnsureFolder("Assets/Characters/Meshes");
            Material dark = Material("Carbon", new Color(0.08f, 0.08f, 0.1f));
            Material gold = Material("Oro", new Color(0.95f, 0.68f, 0.18f));
            Material white = Material("Lino", new Color(0.91f, 0.91f, 0.88f));
            Material steel = Material("Acero", new Color(0.65f, 0.75f, 0.8f));
            Mesh headMesh = HeadMesh();
            for (int index = 0; index < Names.Length; index++)
            {
                Material cloth = Material(Names[index], Colors[index]);
                Material skin = Material(Names[index] + "Piel", index == 0 || index == 3 || index == 6
                    ? new Color(0.64f, 0.4f, 0.26f) : new Color(0.82f, 0.63f, 0.46f));
                for (int slot = 0; slot < 2; slot++)
                {
                    var root = new GameObject(Names[index] + "_P" + (slot + 1));
                    root.SetActive(false);
                    var controller = root.AddComponent<CharacterController>();
                    controller.height = 2.35f;
                    controller.center = new Vector3(0, 1.175f, 0);
                    controller.radius = 0.38f;
                    controller.skinWidth = 0.03f;
                    var fighter = root.AddComponent<Fighter>();
                    SetObject(fighter, "characterData", AssetDatabase.LoadAssetAtPath<CharacterData>($"Assets/Characters/{Names[index]}.asset"));
                    var input = root.AddComponent<PlayerInputController>();
                    var serializedInput = new SerializedObject(input);
                    serializedInput.FindProperty("playerSlot").enumValueIndex = slot;
                    serializedInput.ApplyModifiedPropertiesWithoutUndo();
                    root.AddComponent<HitDetector>();
                    var combat = root.AddComponent<Combatant>();
                    combat.Kind = (SpecialKind)index;
                    combat.PlayerIndex = slot;
                    root.AddComponent<SpecialVisual>();
                    root.AddComponent<SmashThePast.Stages.LaneConstraint>();
                    var visual = root.AddComponent<FighterVisual>();
                    Transform model = new GameObject("Modelo").transform;
                    model.SetParent(root.transform, false);
                    Part(model, "Torso", PrimitiveType.Cube, new Vector3(0, 1.35f, 0), new Vector3(0.58f, 0.8f, 0.45f), cloth);
                    Part(model, "Cinturon", PrimitiveType.Cube, new Vector3(0, 1.05f, 0), new Vector3(0.61f, 0.1f, 0.47f), gold);
                    var head = Part(model, "Cabeza", PrimitiveType.Sphere, new Vector3(0, 2, 0), Vector3.one * 0.65f, skin);
                    head.GetComponent<MeshFilter>().sharedMesh = headMesh;
                    for (int side = -1; side <= 1; side += 2)
                        Part(model, "Ojo", PrimitiveType.Cube, new Vector3(0.25f, 2.03f, side * 0.14f), new Vector3(0.08f, 0.055f, 0.07f), dark);
                    Transform arm1 = Limb(model, "BrazoIzquierdo", new Vector3(0, 1.65f, -0.32f), 0.55f, cloth, skin);
                    Transform arm2 = Limb(model, "BrazoDerecho", new Vector3(0, 1.65f, 0.32f), 0.55f, cloth, skin);
                    Transform leg1 = Limb(model, "PiernaIzquierda", new Vector3(0, 0.95f, -0.17f), 0.7f, cloth, dark);
                    Transform leg2 = Limb(model, "PiernaDerecha", new Vector3(0, 0.95f, 0.17f), 0.7f, cloth, dark);
                    if (index == 0 || index == 7)
                    {
                        Part(model, "AlaSombrero", PrimitiveType.Cylinder, new Vector3(0, 2.29f, 0), new Vector3(1.05f, 0.035f, 0.85f), dark);
                        Part(model, "Sombrero", PrimitiveType.Cylinder, new Vector3(0, index == 7 ? 2.53f : 2.4f, 0), new Vector3(0.6f, index == 7 ? 0.23f : 0.12f, 0.52f), dark);
                    }
                    if (index == 0)
                    {
                        Part(model, "Poncho", PrimitiveType.Cube, new Vector3(-0.04f, 1.55f, 0), new Vector3(0.75f, 0.48f, 0.75f), cloth);
                        Part(model, "FranjaPoncho", PrimitiveType.Cube, new Vector3(-0.04f, 1.42f, 0), new Vector3(0.77f, 0.08f, 0.77f), gold);
                    }
                    if (index == 1)
                    {
                        Part(model, "Casco", PrimitiveType.Sphere, new Vector3(0, 2.23f, 0), new Vector3(0.74f, 0.42f, 0.68f), steel);
                        Part(model, "Cresta", PrimitiveType.Cube, new Vector3(-0.05f, 2.49f, 0), new Vector3(0.18f, 0.3f, 0.14f), cloth);
                    }
                    if (index == 2)
                    {
                        Part(model, "Cabello", PrimitiveType.Sphere, new Vector3(-0.1f, 2.15f, 0), new Vector3(0.6f, 0.5f, 0.66f), dark);
                        Part(model, "Mono", PrimitiveType.Sphere, new Vector3(-0.25f, 2.39f, 0), Vector3.one * 0.25f, dark);
                        for (int side = -1; side <= 1; side += 2)
                        {
                            var sword = Part(model, "Katana", PrimitiveType.Cube, new Vector3(-0.35f, 1.2f, side * 0.22f), new Vector3(0.06f, 1.2f, 0.07f), steel);
                            sword.transform.localRotation = Quaternion.Euler(0, 0, side * 18);
                        }
                    }
                    if (index == 3)
                    {
                        Part(model, "Manto", PrimitiveType.Cube, new Vector3(0.08f, 1.6f, 0), new Vector3(0.64f, 0.28f, 0.52f), white);
                        Part(model, "Baston", PrimitiveType.Cylinder, new Vector3(0.5f, 0.92f, -0.4f), new Vector3(0.05f, 0.9f, 0.05f), gold);
                        Part(model, "Gafas", PrimitiveType.Cube, new Vector3(0.3f, 2.03f, 0), new Vector3(0.04f, 0.035f, 0.42f), dark);
                    }
                    if (index == 4 || index == 7 || index == 8)
                        Part(model, "Barba", PrimitiveType.Sphere, new Vector3(0.12f, index == 4 ? 1.71f : 1.78f, 0), new Vector3(0.5f, index == 4 ? 0.62f : 0.37f, 0.5f), index == 8 ? white : dark);
                    if (index == 4)
                        Part(model, "CabelloLargo", PrimitiveType.Cube, new Vector3(-0.23f, 2, 0), new Vector3(0.27f, 0.68f, 0.65f), dark);
                    if (index == 5)
                    {
                        Part(model, "Bicornio", PrimitiveType.Sphere, new Vector3(0, 2.37f, 0), new Vector3(1.2f, 0.48f, 0.45f), dark);
                        Part(model, "Escarapela", PrimitiveType.Sphere, new Vector3(0, 2.4f, -0.24f), Vector3.one * 0.15f, gold);
                        Part(model, "Pechera", PrimitiveType.Cube, new Vector3(0.3f, 1.43f, 0), new Vector3(0.04f, 0.55f, 0.21f), white);
                    }
                    if (index == 6)
                    {
                        Part(model, "Melena", PrimitiveType.Cube, new Vector3(-0.18f, 2.02f, 0), new Vector3(0.43f, 0.68f, 0.71f), dark);
                        Part(model, "Corona", PrimitiveType.Cylinder, new Vector3(0, 2.25f, 0), new Vector3(0.75f, 0.045f, 0.72f), gold);
                        Part(model, "Collar", PrimitiveType.Cylinder, new Vector3(0, 1.72f, 0), new Vector3(0.67f, 0.045f, 0.6f), gold);
                    }
                    if (index == 8)
                        Part(model, "Toga", PrimitiveType.Cube, new Vector3(0.06f, 1.56f, -0.05f), new Vector3(0.66f, 0.27f, 0.57f), white);
                    SetObject(visual, "model", model);
                    SetObject(visual, "leftArm", arm1);
                    SetObject(visual, "rightArm", arm2);
                    SetObject(visual, "leftLeg", leg1);
                    SetObject(visual, "rightLeg", leg2);
                    root.SetActive(true);
                    PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Characters/Prefabs/{Names[index]}_P{slot + 1}.prefab");
                    Object.DestroyImmediate(root);
                }
            }
            AssetDatabase.SaveAssets();
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        public static void SetObject(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform Limb(Transform parent, string name, Vector3 position, float length, Material cloth, Material end)
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            Part(pivot, "Extremidad", PrimitiveType.Cube, Vector3.down * length / 2, new Vector3(0.22f, length, 0.23f), cloth);
            Part(pivot, "Extremo", PrimitiveType.Cube, Vector3.down * length, new Vector3(0.3f, 0.19f, 0.28f), end);
            return pivot;
        }

        public static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, bool collision = false)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        public static Material Material(string name, Color color)
        {
            string path = $"Assets/Characters/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Glossiness", 0.08f);
            material.EnableKeyword("_EMISSION");
            return material;
        }

        private static Mesh HeadMesh()
        {
            const string path = "Assets/Characters/Meshes/LowPolyHead.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();
            for (int row = 0; row < 6; row++)
                for (int column = 0; column < 10; column++)
                {
                    Vector3 a = SpherePoint(row, column), b = SpherePoint(row + 1, column);
                    Vector3 c = SpherePoint(row + 1, column + 1), d = SpherePoint(row, column + 1);
                    foreach (Vector3 point in new[] { a, c, b, a, d, c })
                    {
                        triangles.Add(vertices.Count);
                        vertices.Add(point);
                    }
                }
            var mesh = new Mesh { name = "CabezaLowPoly" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Vector3 SpherePoint(int row, int column)
        {
            float latitude = Mathf.PI * row / 6;
            float longitude = Mathf.PI * 2 * column / 10;
            return new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude)) * 0.5f;
        }
    }
}
