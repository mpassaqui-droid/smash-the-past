using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Cambia la malla de un luchador sin tocar su logica. El prefab tiene un hijo
    /// "Modelo" que lleva el Animator y el controlador; aqui solo se reemplaza lo que
    /// cuelga de el y se le pone el avatar del modelo nuevo.
    ///
    /// Funciona porque los clips son Humanoid: pasan por el espacio de musculos del
    /// avatar, asi que valen para cualquier esqueleto que Unity acepte como humano.
    /// </summary>
    public static class CambiarModelo
    {
        // luchador -> modelo. Una linea por personaje, esto va a crecer hasta nueve.
        static readonly (string prefab, string fbx)[] MAPA =
        {
            ("Assets/Characters/Prefabs/TupacAmaruII_P1.prefab", "Assets/Characters/Modelos/Externos/Farmer.fbx"),
            ("Assets/Characters/Prefabs/TupacAmaruII_P2.prefab", "Assets/Characters/Modelos/Externos/Farmer.fbx"),
        };

        public static void Aplicar()
        {
            int hechos = 0;
            foreach (var (rutaPrefab, rutaFbx) in MAPA)
            {
                var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(rutaFbx);
                var avatar = AssetDatabase.LoadAllAssetsAtPath(rutaFbx).OfType<Avatar>().FirstOrDefault();
                if (modelo == null || avatar == null || !avatar.isHuman)
                {
                    Debug.LogError($"[cambio] {rutaFbx} no es un humanoide valido");
                    EditorApplication.Exit(1); return;
                }

                var raiz = PrefabUtility.LoadPrefabContents(rutaPrefab);
                var contenedor = raiz.transform.Find("Modelo");
                if (contenedor == null)
                {
                    Debug.LogError($"[cambio] {rutaPrefab} no tiene hijo 'Modelo'");
                    PrefabUtility.UnloadPrefabContents(raiz);
                    EditorApplication.Exit(1); return;
                }

                for (int i = contenedor.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(contenedor.GetChild(i).gameObject);

                var nuevo = (GameObject)PrefabUtility.InstantiatePrefab(modelo, contenedor);
                PrefabUtility.UnpackPrefabInstance(nuevo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                nuevo.transform.localPosition = Vector3.zero;
                nuevo.transform.localRotation = Quaternion.identity;
                nuevo.transform.localScale = Vector3.one;

                // El FBX trae su propio Animator; el que manda es el de "Modelo".
                var suyo = nuevo.GetComponent<Animator>();
                if (suyo != null) Object.DestroyImmediate(suyo);

                var animador = contenedor.GetComponent<Animator>();
                animador.avatar = avatar;

                // FighterVisual gira el modelo hacia el rival; sin esta referencia se
                // quedaba de espaldas a la camara.
                var visual = raiz.GetComponent<SmashThePast.Characters.FighterVisual>();
                if (visual != null)
                {
                    var so = new SerializedObject(visual);
                    so.FindProperty("model").objectReferenceValue = contenedor;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var piel = contenedor.GetComponentInChildren<SkinnedMeshRenderer>();
                Debug.Log($"[cambio] {System.IO.Path.GetFileName(rutaPrefab)} <- {System.IO.Path.GetFileName(rutaFbx)} " +
                          $"alto={(piel != null ? piel.bounds.size.y.ToString("F3") : "?")} " +
                          $"avatar={animador.avatar.name} controlador={(animador.runtimeAnimatorController != null ? animador.runtimeAnimatorController.name : "NINGUNO")}");

                PrefabUtility.SaveAsPrefabAsset(raiz, rutaPrefab);
                PrefabUtility.UnloadPrefabContents(raiz);
                hechos++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[cambio] MODELOS_CAMBIADOS {hechos}");
            EditorApplication.Exit(0);
        }
    }
}
