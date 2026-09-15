using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Sube el sol para acortar la sombra. A 45 grados la sombra mide lo mismo que el
    /// personaje y se estira por medio escenario, que es lo que canto Diego. A 62 la
    /// sombra baja a la mitad de su altura y sigue dando volumen.
    /// </summary>
    public static class LuzSetup
    {
        public static void Poner()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity");
            // Solo la luz CLAVE: es la que proyecta la sombra larga. El relleno y el
            // contraluz tienen sus angulos por una razon y moverlos apaga el volumen.
            var originales = new System.Collections.Generic.Dictionary<string, float>
            {
                ["LuzClave"] = 62f,     // era 48: a 48 la sombra mide lo mismo que el personaje
                ["LuzRelleno"] = 22f,   // sin tocar
                ["LuzContra"] = 348f,   // sin tocar
            };

            var luces = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                              .Where(l => l.type == LightType.Directional).ToArray();
            if (luces.Length == 0) { Debug.LogError("[luz] no hay luz direccional"); EditorApplication.Exit(1); return; }

            int tocadas = 0;
            foreach (var luz in luces)
            {
                if (!originales.TryGetValue(luz.name, out float elevacion))
                {
                    Debug.Log($"[luz] '{luz.name}' no esta en la lista, se deja como esta");
                    continue;
                }
                var antes = luz.transform.rotation.eulerAngles;
                luz.transform.rotation = Quaternion.Euler(elevacion, antes.y, 0f);
                if (luz.name == "LuzClave") luz.shadowStrength = Mathf.Min(luz.shadowStrength, 0.72f);
                if (Mathf.Abs(Mathf.DeltaAngle(antes.x, elevacion)) > 0.5f) tocadas++;
                Debug.Log($"[luz] '{luz.name}' elevacion {antes.x:F0} -> {elevacion:F0}, giro {antes.y:F0}");
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
            Debug.Log($"[luz] LUCES_MOVIDAS {tocadas}");
            EditorApplication.Exit(0);
        }
    }
}
