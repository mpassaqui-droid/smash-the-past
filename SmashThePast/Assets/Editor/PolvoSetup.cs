using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Quita el polvo en suspension.
    ///
    /// Se intento cuatro veces y nunca se vio como polvo: cada particula salia como un
    /// CUADRADO blanco. Sin textura un quad es un cuadrado, pero ponerle un disco de
    /// bordes suaves tampoco arreglo nada, porque el material creado por codigo se
    /// queda en opaco: `_Surface` y `_Blend` por si solos no reconfiguran la mezcla en
    /// URP, y dar `_SrcBlend`, `_DstBlend`, `_ZWrite` y las palabras clave a mano
    /// tampoco basto en una build.
    ///
    /// Un efecto que se supone invisible y acaba siendo lo primero que se ve, sobra.
    /// Si se retoma: crear el material como ASSET desde el editor de materiales, no
    /// por codigo, y comprobarlo en la build antes de darlo por bueno.
    /// </summary>
    public static class PolvoSetup
    {
        public static void Quitar()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity");
            var go = GameObject.Find("PolvoEnSuspension");
            if (go != null) { Object.DestroyImmediate(go); Debug.Log("[polvo] sistema quitado de la escena"); }
            else Debug.Log("[polvo] no habia nada que quitar");

            AssetDatabase.DeleteAsset("Assets/Stages/Art/PolvoEnSuspension.mat");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
            Debug.Log("[polvo] LISTO");
            EditorApplication.Exit(0);
        }
    }
}
