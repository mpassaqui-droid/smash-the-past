using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>Configura los FBX de animacion y comprueba que Unity ve los clips.</summary>
    public static class ClipsSetup
    {
        public static void Configurar()
        {
            var rutas = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Characters/Modelos" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(r => r.Contains("@"))
                .OrderBy(r => r)
                .ToArray();

            if (rutas.Length == 0)
            {
                Debug.LogError("[clips] no hay FBX de animacion");
                EditorApplication.Exit(1);
                return;
            }

            int total = 0;
            foreach (string ruta in rutas)
            {
                var importador = (ModelImporter)AssetImporter.GetAtPath(ruta);
                importador.animationType = ModelImporterAnimationType.Human;
                // Los clips usan el avatar del modelo base: asi todos comparten esqueleto.
                importador.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importador.sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Characters/Modelos/TupacAmaruII.fbx");
                importador.importAnimation = true;
                importador.SaveAndReimport();

                var clips = AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<AnimationClip>()
                    .Where(c => !c.name.StartsWith("__preview")).ToArray();
                total += clips.Length;
                foreach (var clip in clips)
                    Debug.Log($"[clips] {System.IO.Path.GetFileNameWithoutExtension(ruta)} -> {clip.name} {clip.length:0.00}s");
            }

            Debug.Log($"[clips] CLIPS_TOTALES {total}");
            EditorApplication.Exit(total > 0 ? 0 : 1);
        }
    }
}
