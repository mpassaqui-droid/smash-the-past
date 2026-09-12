using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class RenderDebug
    {
        public static void Listar()
        {
            EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            foreach (var nombre in new[] { "TupacAmaruII_P1", "GenghisKhan_P2" })
            {
                var raiz = GameObject.Find(nombre);
                if (raiz == null) { Debug.Log($"[dbg] no existe {nombre}"); continue; }
                foreach (var r in raiz.GetComponentsInChildren<Renderer>(true))
                {
                    var m = r.sharedMaterial;
                    string color = m != null && m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").ToString("F2") : "sin _BaseColor";
                    Debug.Log($"[dbg] {nombre}/{r.name} mat={(m == null ? "NULL" : m.name)} shader={(m == null ? "-" : m.shader.name)} base={color}");
                }
            }
            EditorApplication.Exit(0);
        }
    }
}
