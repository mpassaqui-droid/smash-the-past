using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class PruebaGenerico
    {
        public static void Poner()
        {
            foreach (string ruta in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Characters/Modelos" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(ruta);
                var im = (ModelImporter)AssetImporter.GetAtPath(p);
                im.animationType = ModelImporterAnimationType.Generic;
                im.SaveAndReimport();
            }
            Debug.Log("[generico] puesto");
            EditorApplication.Exit(0);
        }
    }
}
