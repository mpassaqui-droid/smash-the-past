using System.Collections.Generic;
using System.Linq;
using SmashThePast.Characters.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    // Importa los retratos de la pantalla de seleccion y los enchufa en PrototypeUI
    // en el mismo orden que el roster. No inventa el orden: usa PrototypePrefabBuilder.Names.
    public static class PortraitSetup
    {
        private const string Carpeta = "Assets/UI/Portraits";
        private const string RutaEscena = "Assets/Stages/PrototypeArena.unity";

        public static void Configurar()
        {
            Importar();
            if (!Asignar()) { EditorApplication.Exit(1); return; }
            EditorApplication.Exit(0);
        }

        private static void Importar()
        {
            foreach (string nombre in PrototypePrefabBuilder.Names)
            {
                string ruta = $"{Carpeta}/{nombre}.png";
                var importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importador == null) continue;

                importador.textureType = TextureImporterType.Default;
                
                importador.mipmapEnabled = false;
                importador.maxTextureSize = 1024;
                importador.textureCompression = TextureImporterCompression.Compressed;
                importador.SaveAndReimport();
            }
        }

        private static bool Asignar()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            var interfaz = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c != null && c.GetType().Name == "PrototypeUI");
            if (interfaz == null)
            {
                Debug.LogError("[retratos] no se encontro PrototypeUI en la escena");
                return false;
            }

            var datos = new SerializedObject(interfaz);
            var lista = datos.FindProperty("portraits");
            if (lista == null)
            {
                Debug.LogError("[retratos] PrototypeUI no expone el campo portraits");
                return false;
            }

            var nombres = PrototypePrefabBuilder.Names;
            lista.arraySize = nombres.Length;
            int puestos = 0;
            var faltan = new List<string>();
            for (int i = 0; i < nombres.Length; i++)
            {
                var textura = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Carpeta}/{nombres[i]}.png");
                lista.GetArrayElementAtIndex(i).objectReferenceValue = textura;
                if (textura != null) puestos++; else faltan.Add(nombres[i]);
            }
            datos.ApplyModifiedPropertiesWithoutUndo();

            if (!SinScriptsRotos()) return false;

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log($"[retratos] asignados {puestos}/{nombres.Length}" + (faltan.Count > 0 ? $", faltan: {string.Join(", ", faltan)}" : ""));
            return true;
        }

        // Guarda de seguridad: guardar una escena desde batchmode con un componente cuyo
        // script no resuelve escribe una referencia nula y deja la escena rota (el player
        // muere con "level0 is corrupted"). Si eso pasa, no se guarda nada.
        private static bool SinScriptsRotos()
        {
            var rotos = new List<string>();
            foreach (var objeto in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var componente in objeto.GetComponents<Component>())
                {
                    if (componente == null) rotos.Add(objeto.name);
                }
            }

            if (rotos.Count == 0) return true;
            Debug.LogError($"[retratos] escena con scripts rotos, no se guarda: {string.Join(", ", rotos.Distinct())}");
            return false;
        }
    }
}
