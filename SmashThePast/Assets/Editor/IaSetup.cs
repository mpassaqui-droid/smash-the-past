using System.Linq;
using SmashThePast.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class IaSetup
    {
        public static void Configurar()
        {
            var escena = EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            var combate = Object.FindFirstObjectByType<MatchController>();
            if (combate == null) { Debug.LogError("[ia] no hay MatchController"); EditorApplication.Exit(1); return; }

            var director = combate.GetComponent<DirectorIA>();
            if (director == null) director = combate.gameObject.AddComponent<DirectorIA>();
            var datos = new SerializedObject(director);
            datos.FindProperty("match").objectReferenceValue = combate;
            datos.ApplyModifiedPropertiesWithoutUndo();

            bool rotos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(g => g.GetComponents<Component>().Any(c => c == null));
            if (rotos) { Debug.LogError("[ia] hay componentes sin script, no se guarda"); EditorApplication.Exit(1); return; }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[ia] director anadido y cableado");
            EditorApplication.Exit(0);
        }
    }
}
