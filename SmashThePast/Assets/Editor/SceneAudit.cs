using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class SceneAudit
    {
        public static void Listar()
        {
            EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            foreach (var objeto in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var componentes = objeto.GetComponents<Component>();
                for (int i = 0; i < componentes.Length; i++)
                {
                    if (componentes[i] == null)
                        Debug.Log($"[audit] {objeto.name} componente {i} de {componentes.Length} es nulo");
                }
            }
            EditorApplication.Exit(0);
        }
    }
}
