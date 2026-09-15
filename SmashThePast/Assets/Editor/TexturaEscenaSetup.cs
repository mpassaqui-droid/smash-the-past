using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    public static class TexturaEscenaSetup
    {
        public static void Poner()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity");
            // La raiz que agrupa los bloques: la que mas MeshRenderer cuelga de si.
            var raiz = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                             .Where(t => t.parent == null)
                             .OrderByDescending(t => t.GetComponentsInChildren<MeshRenderer>(true).Length)
                             .FirstOrDefault();
            if (raiz == null) { Debug.LogError("[textura] no hay raiz con mallas"); EditorApplication.Exit(1); return; }

            int mallas = raiz.GetComponentsInChildren<MeshRenderer>(true).Length;
            if (raiz.GetComponent<SmashThePast.Stages.EscalaDeTextura>() == null)
                raiz.gameObject.AddComponent<SmashThePast.Stages.EscalaDeTextura>();

            Debug.Log($"[textura] EscalaDeTextura en '{raiz.name}', {mallas} mallas debajo");
            EditorUtility.SetDirty(raiz.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
            EditorApplication.Exit(0);
        }
    }
}
