using System.Linq;
using SmashThePast.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class ArcadeSetup
    {
        public static void Configurar()
        {
            var escena = EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            var combate = Object.FindFirstObjectByType<MatchController>();
            if (combate == null) { Debug.LogError("[arcade] no hay MatchController"); EditorApplication.Exit(1); return; }

            var arcade = combate.GetComponent<ModoArcade>();
            if (arcade == null) arcade = combate.gameObject.AddComponent<ModoArcade>();
            var datos = new SerializedObject(arcade);
            datos.FindProperty("match").objectReferenceValue = combate;
            datos.FindProperty("director").objectReferenceValue = combate.GetComponent<DirectorIA>();
            datos.ApplyModifiedPropertiesWithoutUndo();

            bool rotos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(g => g.GetComponents<Component>().Any(c => c == null));
            if (rotos) { Debug.LogError("[arcade] hay componentes sin script, no se guarda"); EditorApplication.Exit(1); return; }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[arcade] componente anadido y cableado");
            EditorApplication.Exit(0);
        }
    }
}
