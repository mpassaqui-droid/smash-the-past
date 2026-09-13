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
        private const string BanderaEscenario = "-escenario";
        private const string BanderaGolpe = "-golpe";
        private const string BanderaPelea = "-pelea";

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
            var prueba = objeto.AddComponent<PruebaDeHumo>();
            prueba.Destino = destino;
            prueba.Escenario = LeerEntero(BanderaEscenario);
            prueba.Golpe = LeerEntero(BanderaGolpe) > 0;
            prueba.SegundosDePelea = LeerEntero(BanderaPelea);
        }

        private static string LeerArgumento()
        {
            string[] argumentos = Environment.GetCommandLineArgs();
            for (int i = 0; i < argumentos.Length - 1; i++)
                if (argumentos[i] == Bandera) return argumentos[i + 1];
            return null;
        }

        private static int LeerEntero(string bandera)
        {
            string[] argumentos = Environment.GetCommandLineArgs();
            for (int i = 0; i < argumentos.Length - 1; i++)
                if (argumentos[i] == bandera && int.TryParse(argumentos[i + 1], out int valor)) return valor;
            return -1;
        }
    }

    public sealed class PruebaDeHumo : MonoBehaviour
    {
        public string Destino;
        public int Escenario = -1;
        public bool Golpe;
        public int SegundosDePelea = -1;

        private IEnumerator Start()
        {
            for (int i = 0; i < 30; i++) yield return null;

            // Con -escenario N se elige ese personaje para P1, que es lo que cambia de
            // arena. Asi la captura puede recorrer los nueve ambientes sin tocar teclas.
            if (Escenario >= 0)
            {
                var combate = UnityEngine.Object.FindFirstObjectByType<SmashThePast.Stages.MatchController>();
                if (combate != null && combate.Roster != null && Escenario < combate.Roster.Length)
                {
                    combate.Preview(Escenario, (Escenario + 1) % combate.Roster.Length);
                    Debug.Log($"HUMO_ESCENARIO: {Escenario}");
                }
                else Debug.LogWarning("HUMO_ESCENARIO: no se pudo cambiar de arena");
            }

            for (int i = 0; i < 90; i++) yield return null;

            // Con -pelea N se arranca un combate y se deja correr N segundos sin tocar
            // ninguna tecla. P1 no hace nada, asi que si pierde vida es que la maquina
            // esta peleando de verdad.
            if (SegundosDePelea > 0)
            {
                var combate = UnityEngine.Object.FindFirstObjectByType<SmashThePast.Stages.MatchController>();
                if (combate != null)
                {
                    combate.BeginMatch(0, 1);
                    float limite = Time.realtimeSinceStartup + SegundosDePelea;
                    while (Time.realtimeSinceStartup < limite) yield return null;

                    var uno = combate.PlayerOne;
                    var dos = combate.PlayerTwo;
                    Debug.Log($"HUMO_PELEA: P1 {(uno != null && uno.Health != null ? uno.Health.CurrentHealth : -1)} " +
                              $"P2 {(dos != null && dos.Health != null ? dos.Health.CurrentHealth : -1)} " +
                              $"fase {combate.Phase}");
                }
                else Debug.LogWarning("HUMO_PELEA: no hay MatchController");
            }

            // Con -golpe 1 se dispara el empujon de camara sin necesidad de pelear,
            // para poder comprobar en una captura que el zoom de impacto existe.
            if (Golpe)
            {
                var camara = UnityEngine.Object.FindFirstObjectByType<SmashThePast.Stages.ArenaCamera>();
                if (camara != null)
                {
                    camara.Golpe(0.26f);
                    Debug.Log("HUMO_GOLPE: empujon de camara disparado");
                    for (int i = 0; i < 3; i++) yield return null;
                }
                else Debug.LogWarning("HUMO_GOLPE: no hay ArenaCamera");
            }

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
