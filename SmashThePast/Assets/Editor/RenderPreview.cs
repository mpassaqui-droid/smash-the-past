using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SmashThePast.EditorTools
{
    // Renderiza la escena a PNG sin abrir el editor, para poder mirar el resultado
    // en vez de fiarse de que "deberia verse mejor".
    // Unity -batchmode -executeMethod SmashThePast.EditorTools.RenderPreview.Capturar -- salida.png
    public static class RenderPreview
    {
        private const string RutaEscena = "Assets/Stages/PrototypeArena.unity";

        public static void Capturar()
        {
            string salida = LeerArgumento() ?? "/tmp/smash-preview.png";
            try
            {
                EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

                // Modo diagnostico: quita post proceso y deja una sola luz, para ver el albedo crudo.
                if (Environment.GetEnvironmentVariable("SMASH_CRUDO") == "1")
                {
                    var post = GameObject.Find("PostProceso");
                    if (post != null) post.SetActive(false);
                    foreach (var luz in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    {
                        luz.enabled = luz.name == "LuzClave";
                        if (luz.enabled) luz.intensity = 1f;
                    }
                    RenderSettings.fog = false;
                    Debug.Log("[preview] modo crudo: sin post proceso, una sola luz");
                }

                // En batchmode el SRP Batcher pinta toda la escena con las constantes de un
                // solo material (sale todo dorado). Es un problema de la captura headless, no
                // del proyecto: el juego se queda con el batcher activo. Aqui se apaga solo
                // para que la imagen represente los materiales de verdad.
                {
                    var urp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                    if (urp != null)
                    {
                        urp.useSRPBatcher = false;
                        Debug.Log("[preview] SRP Batcher desactivado solo para la captura");
                    }
                }
                Camera camara = Camera.main;
                if (camara == null)
                {
                    foreach (var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                    {
                        camara = c;
                        break;
                    }
                }

                if (camara == null)
                {
                    Debug.LogError("[preview] la escena no tiene camara");
                    EditorApplication.Exit(1);
                    return;
                }

                const int ancho = 1600;
                const int alto = 900;
                var destino = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 1,
                };
                var textura = new Texture2D(ancho, alto, TextureFormat.RGB24, false);

                camara.targetTexture = destino;

                // Camera.Render() es la ruta antigua: con URP activo dibuja sin los pases
                // del pipeline y sale todo gris. Hay que pasar por la peticion de render.
                var peticion = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest();
                if (RenderPipeline.SupportsRenderRequest(camara, peticion))
                {
                    peticion.destination = destino;
                    RenderPipeline.SubmitRenderRequest(camara, peticion);
                    Debug.Log("[preview] render por SingleCameraRequest (URP)");
                }
                else
                {
                    camara.Render();
                    Debug.LogWarning("[preview] sin soporte de peticion de render, ruta antigua");
                }

                RenderTexture anterior = RenderTexture.active;
                RenderTexture.active = destino;
                textura.ReadPixels(new Rect(0, 0, ancho, alto), 0, 0);
                textura.Apply();
                RenderTexture.active = anterior;
                camara.targetTexture = null;

                Directory.CreateDirectory(Path.GetDirectoryName(salida));
                File.WriteAllBytes(salida, textura.EncodeToPNG());
                Debug.Log($"[preview] escrito {salida} ({new FileInfo(salida).Length} bytes)");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogError($"[preview] fallo: {error}");
                EditorApplication.Exit(1);
            }
        }

        private static string LeerArgumento()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--") return args[i + 1];
            }
            return null;
        }
    }
}
