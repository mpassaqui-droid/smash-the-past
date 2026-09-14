using System.Linq;
using SmashThePast.Audio;
using SmashThePast.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    public static class MusicaSetup
    {
        public static void Configurar()
        {
            var escena = EditorSceneManager.OpenScene("Assets/Stages/PrototypeArena.unity", OpenSceneMode.Single);
            var combate = Object.FindFirstObjectByType<MatchController>();
            if (combate == null) { Debug.LogError("[musica] no hay MatchController"); EditorApplication.Exit(1); return; }

            var existente = GameObject.Find("Musica");
            var objeto = existente != null ? existente : new GameObject("Musica", typeof(AudioSource));
            var musica = objeto.GetComponent<MusicaDelJuego>();
            if (musica == null) musica = objeto.AddComponent<MusicaDelJuego>();

            var datos = new SerializedObject(musica);
            datos.FindProperty("match").objectReferenceValue = combate;
            datos.ApplyModifiedPropertiesWithoutUndo();

            bool rotos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(g => g.GetComponents<Component>().Any(c => c == null));
            if (rotos) { Debug.LogError("[musica] hay componentes sin script, no se guarda"); EditorApplication.Exit(1); return; }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[musica] objeto de musica anadido y cableado");
            EditorApplication.Exit(0);
        }
    }
}
