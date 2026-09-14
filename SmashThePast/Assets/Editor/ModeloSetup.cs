using UnityEditor;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Configura un FBX como Humanoid y comprueba que Unity acepta el esqueleto. Esta es
    /// la prueba que decide si el camino por Blender sirve: si el avatar no es valido,
    /// las animaciones de Mixamo no valen y el modelo es solo una figura bonita.
    /// </summary>
    public static class ModeloSetup
    {
        public static void Configurar()
        {
            string ruta = "Assets/Characters/Modelos/TupacAmaruII.fbx";
            var importador = AssetImporter.GetAtPath(ruta) as ModelImporter;
            if (importador == null)
            {
                Debug.LogError($"[modelo] no se pudo importar {ruta}");
                EditorApplication.Exit(1);
                return;
            }

            importador.animationType = ModelImporterAnimationType.Human;
            importador.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importador.importAnimation = false;
            importador.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importador.SaveAndReimport();

            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(ruta);
            var raiz = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);

            if (avatar == null)
            {
                Debug.LogError("[modelo] Unity no creo ningun avatar: el esqueleto no le vale");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[modelo] avatar valido={avatar.isValid} humano={avatar.isHuman}");
            if (raiz != null)
            {
                var renderizador = raiz.GetComponentInChildren<SkinnedMeshRenderer>();
                Debug.Log($"[modelo] huesos={(renderizador != null ? renderizador.bones.Length : 0)} " +
                          $"vertices={(renderizador != null && renderizador.sharedMesh != null ? renderizador.sharedMesh.vertexCount : 0)}");
            }

            if (!avatar.isValid || !avatar.isHuman)
            {
                Debug.LogError("[modelo] el avatar no es humano valido");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[modelo] MIXAMO_COMPATIBLE");
            EditorApplication.Exit(0);
        }
    }
}
