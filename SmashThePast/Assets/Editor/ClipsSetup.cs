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
                importador.animationType = ModelImporterAnimationType.Generic;
                // Los clips usan el avatar del modelo base: asi todos comparten esqueleto.
                importador.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importador.sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Characters/Modelos/TupacAmaruII.fbx");
                importador.importAnimation = true;

                // Sin esto el personaje se dibujaba siete metros por encima del suelo: un
                // clip Humanoid lleva la posicion de la cadera horneada, y Unity la aplica
                // encima de la posicion del objeto. Se ancla la raiz a su sitio original.
                var ajustes = importador.defaultClipAnimations;
                if (ajustes != null && ajustes.Length > 0)
                {
                    foreach (var clip in ajustes)
                    {
                        clip.lockRootRotation = true;
                        clip.keepOriginalOrientation = true;
                        clip.lockRootHeightY = true;
                        clip.keepOriginalPositionY = true;
                        clip.lockRootPositionXZ = true;
                        clip.keepOriginalPositionXZ = true;
                        clip.loopTime = clip.name == "idle" || clip.name == "andar";
                    }
                    importador.clipAnimations = ajustes;
                }

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
