using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>Pone el fondo de video y los sonidos de menu en el objeto de la interfaz.</summary>
    public static class TituloSetup
    {
        public static void Poner()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity");
            var ui = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                           .FirstOrDefault(m => m != null && m.GetType().Name == "PrototypeUI");
            if (ui == null) { Debug.LogError("[titulo] no hay PrototypeUI en la escena"); EditorApplication.Exit(1); return; }

            var go = ui.gameObject;
            int puestos = 0;
            foreach (var tipo in new[] { "SmashThePast.UI.TituloDelMenu", "SmashThePast.UI.SonidoDeMenu" })
            {
                var t = System.AppDomain.CurrentDomain.GetAssemblies()
                          .Select(a => a.GetType(tipo)).FirstOrDefault(x => x != null);
                if (t == null) { Debug.LogError($"[titulo] no existe el tipo {tipo}"); EditorApplication.Exit(1); return; }
                if (go.GetComponent(t) == null) { go.AddComponent(t); puestos++; }
                Debug.Log($"[titulo] {tipo.Split('.').Last()} en '{go.name}'");
            }

            var clip = AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>("Assets/Resources/Video/TituloMenu.mp4");
            Debug.Log($"[titulo] clip={(clip != null ? $"{clip.width}x{clip.height} {clip.length:F2}s" : "NO CARGA")}");
            var musica = Resources.Load<AudioClip>("Musica/titulo");
            var mover = Resources.Load<AudioClip>("Interfaz/menu_mover");
            var aceptar = Resources.Load<AudioClip>("Interfaz/menu_aceptar");
            Debug.Log($"[titulo] musica={(musica != null ? musica.length.ToString("F1") + "s" : "NO")} mover={(mover != null ? "si" : "NO")} aceptar={(aceptar != null ? "si" : "NO")}");

            EditorUtility.SetDirty(go);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
            Debug.Log($"[titulo] COMPONENTES_ANADIDOS {puestos}");
            EditorApplication.Exit(clip != null && musica != null && mover != null && aceptar != null ? 0 : 1);
        }
    }
}
