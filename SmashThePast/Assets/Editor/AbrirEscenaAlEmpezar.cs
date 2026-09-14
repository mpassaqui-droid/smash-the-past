using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Al abrir el proyecto por primera vez, Unity arranca con una escena vacia y quien
    /// llega nuevo ve una pantalla en negro y cree que el juego esta roto. Esto abre la
    /// arena la primera vez, y solo si no hay ninguna escena abierta.
    /// </summary>
    [InitializeOnLoad]
    public static class AbrirEscenaAlEmpezar
    {
        private const string Escena = "Assets/Stages/PrototypeArena.unity";

        static AbrirEscenaAlEmpezar()
        {
            // Ni en batchmode (las pruebas y los builds abren sus propias escenas) ni
            // mientras se juega.
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.delayCall += Abrir;
        }

        private static void Abrir()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;

            var actual = EditorSceneManager.GetActiveScene();
            // Una escena sin ruta es la vacia que Unity crea al abrir el proyecto.
            if (!string.IsNullOrEmpty(actual.path)) return;
            if (actual.isDirty) return;
            if (!System.IO.File.Exists(Escena)) return;

            EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);
            Debug.Log("Abierta la arena de combate. Pulsa Play para jugar.");
        }
    }
}
