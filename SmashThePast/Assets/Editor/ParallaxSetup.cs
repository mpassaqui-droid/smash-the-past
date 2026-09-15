using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace SmashThePast.EditorTools
{
    public static class ParallaxSetup
    {
        public static void Poner()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity");
            var camara = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c => c.CompareTag("MainCamera"))
                      ?? Object.FindFirstObjectByType<Camera>();
            var cordillera = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Cordillera");
            if (camara == null || cordillera == null) { Debug.LogError("[parallax] falta camara o Cordillera"); EditorApplication.Exit(1); return; }

            // Sin reparentar nada: se toma cada cumbre por separado y se le da el
            // factor que le toca por su z (25 las lejanas, 15 las cercanas). Mover
            // objetos entre padres en una escena ya construida fallaba en silencio.
            var cumbres = cordillera.Cast<Transform>().ToArray();
            if (cumbres.Length == 0) { Debug.LogError("[parallax] Cordillera no tiene cumbres"); EditorApplication.Exit(1); return; }

            var capas = new List<SmashThePast.Stages.Parallax.Capa>();
            int lejanas = 0, cercanas = 0;
            foreach (var cumbre in cumbres)
            {
                bool lejos = cumbre.localPosition.z > 20f;
                if (lejos) lejanas++; else cercanas++;
                capas.Add(new SmashThePast.Stages.Parallax.Capa
                {
                    objetivo = cumbre,
                    factor = lejos ? 0.82f : 0.55f,
                });
            }

            // Limpiar los grupos vacios de un intento anterior.
            foreach (var sobra in new[] { "CumbresLejanas", "CumbresCercanas" })
            {
                var go = GameObject.Find(sobra);
                if (go != null && go.transform.childCount == 0) Object.DestroyImmediate(go);
            }

            var raiz = cordillera.parent;
            var par = raiz.GetComponent<SmashThePast.Stages.Parallax>() ?? raiz.gameObject.AddComponent<SmashThePast.Stages.Parallax>();
            par.Configurar(capas.ToArray(), camara);

            Debug.Log($"[parallax] {lejanas} cumbres lejanas (factor 0,82) y {cercanas} cercanas (0,55), camara '{camara.name}' ortografica={camara.orthographic}");
            EditorUtility.SetDirty(raiz.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
            EditorApplication.Exit(0);
        }
    }
}
