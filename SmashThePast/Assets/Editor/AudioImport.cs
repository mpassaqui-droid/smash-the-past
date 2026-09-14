using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// La musica es larga: Streaming y carga en segundo plano. Los efectos son cortos y
    /// tienen que sonar en el fotograma del golpe, asi que van descomprimidos en memoria.
    /// </summary>
    public static class AudioImport
    {
        public static void Configurar()
        {
            int tocados = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" }))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                var im = (AudioImporter)AssetImporter.GetAtPath(ruta);
                if (im == null) continue;
                bool musica = ruta.Contains("/Musica/") || ruta.Contains("/Pendientes/");
                var ajustes = im.defaultSampleSettings;
                var quiero = musica ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                bool cambia = ajustes.loadType != quiero || im.loadInBackground != musica;
                if (cambia)
                {
                    ajustes.loadType = quiero;
                    im.defaultSampleSettings = ajustes;
                    im.loadInBackground = musica;
                    im.SaveAndReimport();
                    tocados++;
                }
                Debug.Log($"[audio] {System.IO.Path.GetFileName(ruta),-30} {(musica ? "musica" : "efecto")} carga={im.defaultSampleSettings.loadType} fondo={im.loadInBackground}{(cambia ? "  (corregido)" : "")}");
            }
            Debug.Log($"[audio] AJUSTADOS {tocados}");
            EditorApplication.Exit(0);
        }
    }
}
