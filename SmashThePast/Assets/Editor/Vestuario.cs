using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Pinta cada luchador con su paleta. Los modelos de Quaternius vienen todos en los
    /// mismos beiges, asi que nueve personajes distintos parecen el mismo. Aqui se les
    /// clona el material y se les cambia el color, sin tocar el FBX: el mismo modelo
    /// puede servir a dos personajes con aspectos distintos.
    ///
    /// El reparto no es por nombre de material (cada modelo usa los suyos) sino por
    /// orden: el primer material que no sea piel ni ojos recibe el color principal, el
    /// siguiente el secundario, y asi. Es tosco, pero da nueve siluetas reconocibles.
    /// </summary>
    public static class Vestuario
    {
        const string PREFABS = "Assets/Characters/Prefabs";
        const string MATERIALES = "Assets/Characters/Materiales";

        // personaje -> piel + colores de ropa por orden de aparicion
        static readonly Dictionary<string, (Color piel, Color[] ropa)> PALETAS = new()
        {
            ["TupacAmaruII"] = (C(0.58f,0.36f,0.23f), new[]{ C(0.62f,0.09f,0.10f), C(0.88f,0.62f,0.14f), C(0.10f,0.09f,0.08f), C(0.45f,0.07f,0.08f) }),
            ["GenghisKhan"] = (C(0.72f,0.55f,0.36f), new[]{ C(0.20f,0.28f,0.18f), C(0.45f,0.33f,0.16f), C(0.12f,0.12f,0.12f), C(0.62f,0.48f,0.22f) }),
            ["MiyamotoMusashi"] = (C(0.80f,0.63f,0.47f), new[]{ C(0.14f,0.16f,0.24f), C(0.75f,0.73f,0.68f), C(0.35f,0.10f,0.12f), C(0.20f,0.20f,0.24f) }),
            ["Gandhi"] = (C(0.55f,0.38f,0.24f), new[]{ C(0.92f,0.90f,0.85f), C(0.85f,0.83f,0.78f), C(0.70f,0.68f,0.64f), C(0.95f,0.94f,0.90f) }),
            ["Rasputin"] = (C(0.78f,0.62f,0.50f), new[]{ C(0.12f,0.11f,0.14f), C(0.30f,0.22f,0.28f), C(0.55f,0.45f,0.30f), C(0.18f,0.16f,0.20f) }),
            ["NapoleonBonaparte"] = (C(0.82f,0.66f,0.52f), new[]{ C(0.10f,0.14f,0.35f), C(0.85f,0.80f,0.72f), C(0.80f,0.65f,0.20f), C(0.60f,0.12f,0.14f) }),
            ["Cleopatra"] = (C(0.68f,0.48f,0.30f), new[]{ C(0.90f,0.88f,0.82f), C(0.85f,0.65f,0.18f), C(0.10f,0.30f,0.42f), C(0.80f,0.75f,0.65f) }),
            ["AbrahamLincoln"] = (C(0.78f,0.63f,0.50f), new[]{ C(0.12f,0.12f,0.14f), C(0.25f,0.24f,0.26f), C(0.90f,0.90f,0.88f), C(0.16f,0.16f,0.18f) }),
            ["Socrates"] = (C(0.72f,0.55f,0.38f), new[]{ C(0.88f,0.86f,0.78f), C(0.78f,0.74f,0.64f), C(0.60f,0.40f,0.22f), C(0.92f,0.90f,0.84f) }),
        };

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        public static void Pintar()
        {
            int piezas = 0;
            foreach (var (personaje, paleta) in PALETAS.Select(k => (k.Key, k.Value)))
            {
                string carpeta = $"{MATERIALES}/{personaje}";
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                AssetDatabase.Refresh();

                foreach (string lado in new[] { "P1", "P2" })
                {
                    string ruta = $"{PREFABS}/{personaje}_{lado}.prefab";
                    var raiz = PrefabUtility.LoadPrefabContents(ruta);
                    int siguiente = 0;
                    var yaHechos = new Dictionary<string, Material>();

                    foreach (var r in raiz.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        var mats = r.sharedMaterials;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            if (mats[i] == null) continue;
                            string original = mats[i].name;
                            if (!yaHechos.TryGetValue(original, out var nuevo))
                            {
                                bool piel = original.Contains("Skin");
                                bool ojo = original.Contains("Eye");
                                Color color = piel ? paleta.piel
                                            : ojo ? mats[i].color
                                            : paleta.ropa[siguiente++ % paleta.ropa.Length];

                                nuevo = new Material(mats[i]) { name = $"{personaje}_{original}", color = color };
                                if (nuevo.HasProperty("_BaseColor")) nuevo.SetColor("_BaseColor", color);
                                AssetDatabase.CreateAsset(nuevo, $"{carpeta}/{personaje}_{original}.mat");
                                yaHechos[original] = nuevo;
                                piezas++;
                            }
                            mats[i] = nuevo;
                        }
                        r.sharedMaterials = mats;
                    }
                    PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
                    PrefabUtility.UnloadPrefabContents(raiz);
                    Debug.Log($"[pintar] {personaje}_{lado} materiales={yaHechos.Count}");
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[pintar] MATERIALES_CREADOS {piezas}");
            EditorApplication.Exit(0);
        }
    }
}
