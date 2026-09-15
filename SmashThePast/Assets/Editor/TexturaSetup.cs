using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Pone una textura a cada material del escenario. Eran colores planos, que es lo
    /// que mas se lee como "prototipo". El color del material se conserva y pasa a ser
    /// el TINTE de la textura, asi que la paleta de cada arena no cambia.
    /// </summary>
    public static class TexturaSetup
    {
        // material -> textura, y cuantas veces se repite por metro
        static readonly (string material, string textura, float tiling)[] MAPA =
        {
            ("Losas",          "cobblestone_04",  0.55f),
            ("PiedraClara",    "grey_plaster",    0.30f),
            ("PiedraOscura",   "granite_wall",    0.30f),
            ("Pasto",          "grass_path_2",    0.45f),
            ("MontanaCercana", "aerial_rocks_02", 0.10f),
            ("MontanaLejana",  "marble_cliff_01", 0.06f),
            ("TejidoRojo",     "rough_linen",     1.20f),
            ("TejidoBlanco",   "rough_linen",     1.20f),
            ("TejidoOcre",     "rough_linen",     1.20f),
        };

        public static void Poner()
        {
            int hechos = 0;
            foreach (var (material, textura, tiling) in MAPA)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Stages/Art/{material}.mat");
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Stages/Art/Texturas/{textura}.jpg");
                if (mat == null || tex == null)
                {
                    Debug.LogError($"[textura] falta {material}.mat o {textura}.jpg");
                    EditorApplication.Exit(1); return;
                }

                // El color que ya tenia se queda como tinte. Se aclara un poco porque
                // multiplicar un color medio por una textura media oscurece el doble.
                Color tinte = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                // Las texturas van desaturadas y aclaradas a proposito: aportan el GRANO y el
                // color lo pone el material. Asi cada arena conserva su paleta en vez de
                // volverse parda, que es lo que pasaba con las fotos tal cual.
                Color aclarado = Color.Lerp(tinte, Color.white, 0.30f);

                if (mat.HasProperty("_BaseMap")) { mat.SetTexture("_BaseMap", tex); }
                if (mat.HasProperty("_MainTex")) { mat.SetTexture("_MainTex", tex); }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", aclarado);
                mat.color = aclarado;

                EditorUtility.SetDirty(mat);
                Debug.Log($"[textura] {material,-16} <- {textura,-18} repeticion {tiling:F2}  tinte {ColorUtility.ToHtmlStringRGB(tinte)} -> {ColorUtility.ToHtmlStringRGB(aclarado)}");
                hechos++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[textura] MATERIALES_TEXTURADOS {hechos}");
            EditorApplication.Exit(0);
        }
    }
}
