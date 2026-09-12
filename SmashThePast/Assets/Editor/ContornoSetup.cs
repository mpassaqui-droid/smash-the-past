using System.Linq;
using SmashThePast.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class ContornoSetup
    {
        private const string RutaEscena = "Assets/Stages/PrototypeArena.unity";

        public static void Configurar()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            var combate = Object.FindFirstObjectByType<MatchController>();
            if (combate == null)
            {
                Debug.LogError("[contorno] no hay MatchController en la escena");
                EditorApplication.Exit(1);
                return;
            }

            var contorno = combate.GetComponent<ContornoToon>();
            if (contorno == null) contorno = combate.gameObject.AddComponent<ContornoToon>();

            var datos = new SerializedObject(contorno);
            datos.FindProperty("match").objectReferenceValue = combate;
            datos.FindProperty("grosor").floatValue = 0.022f;
            datos.ApplyModifiedPropertiesWithoutUndo();

            bool rotos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(g => g.GetComponents<Component>().Any(c => c == null));
            if (rotos)
            {
                Debug.LogError("[contorno] hay componentes sin script, no se guarda");
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[contorno] componente anadido y cableado");
            EditorApplication.Exit(0);
        }
    }
}
