using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace SmashThePast.Diagnostics
{
    // Prueba de humo del player construido. Un build que compila no demuestra que el
    // juego arranque: el crash de "level0 is corrupted" pasaba la CI sin que nadie lo
    // viera. Con -humo <ruta.png> el player arranca, deja correr unos fotogramas,
    // guarda una captura de lo que se ve de verdad y se cierra con codigo 0.
    public static class ArranqueDeHumo
    {
        private const string Bandera = "-humo";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Arrancar()
        {
            string[] argumentos = Environment.GetCommandLineArgs();
            Debug.Log($"HUMO_ARGS: {string.Join(" | ", argumentos)}");


            string destino = LeerArgumento();
            if (string.IsNullOrEmpty(destino)) return;

            // Sin esto el player se congela en cuanto pierde el foco y la corrutina
            // nunca avanza: la prueba se quedaba colgada sin escribir la captura.
            Application.runInBackground = true;

            var objeto = new GameObject("PruebaDeHumo");
            UnityEngine.Object.DontDestroyOnLoad(objeto);
            objeto.AddComponent<PruebaDeHumo>().Destino = destino;
        }

        private static string LeerArgumento()
        {
            string[] argumentos = Environment.GetCommandLineArgs();
            for (int i = 0; i < argumentos.Length - 1; i++)
                if (argumentos[i] == Bandera) return argumentos[i + 1];
            return null;
        }
    }

    public sealed class PruebaDeHumo : MonoBehaviour
    {
        public string Destino;

        private IEnumerator Start()
        {
            for (int i = 0; i < 120; i++) yield return null;

            string carpeta = Path.GetDirectoryName(Destino);
            if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);
            ScreenCapture.CaptureScreenshot(Destino);

            for (int i = 0; i < 240 && !File.Exists(Destino); i++) yield return null;

            if (File.Exists(Destino))
            {
                Debug.Log($"HUMO_OK: {Destino}, {new FileInfo(Destino).Length} bytes, fotogramas {Time.frameCount}");
                yield return null;
                Application.Quit(0);
            }
            else
            {
                Debug.LogError($"HUMO_FALLO: no se escribio {Destino}");
                yield return null;
                Application.Quit(2);
            }
        }
    }
}
