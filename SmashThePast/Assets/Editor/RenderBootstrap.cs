using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    // Instala los paquetes de render. Se ejecuta una sola vez en batchmode:
    // Unity -batchmode -executeMethod SmashThePast.EditorTools.RenderBootstrap.InstalarPaquetes
    public static class RenderBootstrap
    {
        private static readonly string[] Paquetes =
        {
            "com.unity.render-pipelines.universal",
            "com.unity.cinemachine",
        };

        public static void InstalarPaquetes()
        {
            foreach (var paquete in Paquetes)
            {
                Debug.Log($"[render] anadiendo {paquete}");
                AddRequest peticion = Client.Add(paquete);
                while (!peticion.IsCompleted) Thread.Sleep(200);

                if (peticion.Status != StatusCode.Success)
                {
                    Debug.LogError($"[render] fallo al anadir {paquete}: {peticion.Error?.message}");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"[render] instalado {peticion.Result.name}@{peticion.Result.version}");
            }

            Debug.Log("[render] paquetes listos");
            EditorApplication.Exit(0);
        }
    }
}
