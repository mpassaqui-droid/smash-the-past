using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class ModeloDebug
    {
        public static void Medir()
        {
            foreach (string ruta in new[] { "Assets/Characters/Modelos/TupacAmaruII.fbx",
                                            "Assets/Characters/Prefabs/TupacAmaruII_P1.prefab" })
            {
                var fuente = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
                if (fuente == null) { Debug.Log($"[medir] {ruta} NO CARGA"); continue; }
                var copia = Object.Instantiate(fuente);
                var piel = copia.GetComponentInChildren<SkinnedMeshRenderer>();
                var normal = copia.GetComponentInChildren<MeshRenderer>();
                Debug.Log($"[medir] {System.IO.Path.GetFileName(ruta)} escala={copia.transform.localScale} " +
                          $"skinned={(piel != null ? piel.bounds.size.ToString("F3") : "ninguno")} " +
                          $"mesh={(normal != null ? normal.bounds.size.ToString("F3") : "ninguno")} " +
                          $"materiales={(piel != null && piel.sharedMaterials.Length > 0 ? piel.sharedMaterials[0]?.shader?.name : "-")}");
                if (piel != null)
                    Debug.Log($"[medir]   escala del modelo={piel.transform.root.localScale} pos={piel.transform.position}");
                Object.DestroyImmediate(copia);
            }
            var importador = (ModelImporter)AssetImporter.GetAtPath("Assets/Characters/Modelos/TupacAmaruII.fbx");
            Debug.Log($"[medir] factor de escala del importador={importador.globalScale} usa unidades={importador.useFileScale}");
            EditorApplication.Exit(0);
        }
    }
}
