using System.Linq;
using SmashThePast.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    // El objeto ContenidoDLC de la escena tiene un MonoBehaviour cuyo m_Script apunta a un
    // fileID local sin guid, asi que Unity no resuelve el script y el componente queda roto.
    // El servicio de entitlement no existe en ejecucion aunque los tests de la clase pasen.
    public static class DlcRewire
    {
        private const string RutaEscena = "Assets/Stages/PrototypeArena.unity";
        private const string RutaOferta = "Assets/Characters/HitlerDlc.asset";

        public static void Reparar()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            var objeto = GameObject.Find("ContenidoDLC");
            if (objeto == null)
            {
                Debug.LogError("[dlc] no existe ContenidoDLC en la escena");
                EditorApplication.Exit(1);
                return;
            }

            int quitados = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(objeto);
            Debug.Log($"[dlc] componentes rotos quitados: {quitados}");

            var servicio = objeto.GetComponent<DlcEntitlementService>();
            if (servicio == null) servicio = objeto.AddComponent<DlcEntitlementService>();

            var oferta = AssetDatabase.LoadAssetAtPath<DlcOffer>(RutaOferta);
            if (oferta == null)
            {
                Debug.LogError($"[dlc] falta {RutaOferta}");
                EditorApplication.Exit(1);
                return;
            }

            var datos = new SerializedObject(servicio);
            var lista = datos.FindProperty("offers");
            lista.arraySize = 1;
            lista.GetArrayElementAtIndex(0).objectReferenceValue = oferta;
            datos.ApplyModifiedPropertiesWithoutUndo();

            bool rotos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(g => g.GetComponents<Component>().Any(c => c == null));
            if (rotos)
            {
                Debug.LogError("[dlc] siguen quedando scripts rotos, no se guarda");
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log($"[dlc] servicio recableado con {oferta.name}");
            EditorApplication.Exit(0);
        }
    }
}
