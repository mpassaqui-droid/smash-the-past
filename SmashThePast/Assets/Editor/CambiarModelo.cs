using UnityEditor;
using UnityEngine;
using System.IO;
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
    ///
    /// Es REVERSIBLE. La primera vez que se toca un prefab se guarda una copia intacta
    /// en Prefabs/Anteriores/, y `Volver` las restituye. Asi se puede probar Quaternius sin
    /// perder los personajes de cajas si al final no convencen.
    ///
    ///   -executeMethod SmashThePast.EditorTools.CambiarModelo.Aplicar
    ///   -executeMethod SmashThePast.EditorTools.CambiarModelo.Volver
    /// </summary>
    public static class CambiarModelo
    {
        const string PREFABS = "Assets/Characters/Prefabs";
        const string COPIAS = "Assets/Characters/Prefabs/Anteriores";
        const string MODELOS = "Assets/Characters/Modelos/Externos";
        const string CONTROLADOR = "Assets/Characters/Modelos/Luchador.controller";

        // luchador -> modelo del pack. Ninguno es un retrato fiel: son bases sobre las
        // que despues van las piezas de vestuario. Cambiar una linea y volver a lanzar.
        static readonly (string personaje, string modelo)[] MAPA =
        {
            ("TupacAmaruII",      "Farmer"),      // el unico del pack con sombrero de ala ancha
            ("GenghisKhan",       "Swat"),        // lo mas cercano a un guerrero con proteccion
            ("MiyamotoMusashi",   "Adventurer"),
            ("Gandhi",            "Beach"),       // el de ropa mas escasa, por el dhoti
            ("Rasputin",          "Punk"),        // el pelo revuelto es lo que mas se le parece
            ("NapoleonBonaparte", "King"),        // por el manto de la coronacion
            ("Cleopatra",         "Casual"),      // OJO: el pack es de hombres, aqui hace falta otra fuente
            ("AbrahamLincoln",    "Suit"),
            ("Socrates",          "Casual2"),     // provisional, la tunica hay que modelarla
        };

        public static void Aplicar()
        {
            if (!Directory.Exists(COPIAS)) { Directory.CreateDirectory(COPIAS); AssetDatabase.Refresh(); }

            int hechos = 0, guardados = 0;
            foreach (var (personaje, modelo) in MAPA)
            {
                string rutaFbx = $"{MODELOS}/{modelo}.fbx";
                var fuente = AssetDatabase.LoadAssetAtPath<GameObject>(rutaFbx);
                var avatar = AssetDatabase.LoadAllAssetsAtPath(rutaFbx).OfType<Avatar>().FirstOrDefault();
                if (fuente == null || avatar == null || !avatar.isHuman)
                {
                    Debug.LogError($"[cambio] {rutaFbx} no es un humanoide valido");
                    EditorApplication.Exit(1); return;
                }

                foreach (string lado in new[] { "P1", "P2" })
                {
                    string ruta = $"{PREFABS}/{personaje}_{lado}.prefab";
                    string copia = $"{COPIAS}/{personaje}_{lado}.prefab";

                    // La copia se hace UNA vez, antes del primer cambio. Si ya existe no
                    // se toca: si no, la segunda pasada guardaria el Quaternius encima
                    // del original de cajas y no habria vuelta atras.
                    if (!File.Exists(copia))
                    {
                        if (!AssetDatabase.CopyAsset(ruta, copia))
                        {
                            Debug.LogError($"[cambio] no se pudo copiar {ruta}");
                            EditorApplication.Exit(1); return;
                        }
                        guardados++;
                    }

                    if (!Sustituir(ruta, fuente, avatar, modelo)) { EditorApplication.Exit(1); return; }
                    hechos++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[cambio] MODELOS_CAMBIADOS {hechos} copias_nuevas {guardados}");
            EditorApplication.Exit(0);
        }

        public static void Volver()
        {
            int vueltos = 0;
            foreach (var (personaje, _) in MAPA)
                foreach (string lado in new[] { "P1", "P2" })
                {
                    string copia = $"{COPIAS}/{personaje}_{lado}.prefab";
                    if (!File.Exists(copia)) { Debug.Log($"[cambio] sin copia de {personaje}_{lado}, se deja como esta"); continue; }
                    AssetDatabase.DeleteAsset($"{PREFABS}/{personaje}_{lado}.prefab");
                    AssetDatabase.CopyAsset(copia, $"{PREFABS}/{personaje}_{lado}.prefab");
                    vueltos++;
                }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[cambio] VUELTOS_AL_ANTERIOR {vueltos}");
            EditorApplication.Exit(0);
        }

        static bool Sustituir(string rutaPrefab, GameObject modelo, Avatar avatar, string nombreModelo)
        {
            var raiz = PrefabUtility.LoadPrefabContents(rutaPrefab);
            var contenedor = raiz.transform.Find("Modelo");
            if (contenedor == null)
            {
                Debug.LogError($"[cambio] {rutaPrefab} no tiene hijo 'Modelo'");
                PrefabUtility.UnloadPrefabContents(raiz);
                return false;
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

            // Los personajes de cajas nunca tuvieron Animator: se movian a mano desde
            // FighterVisual. Al darles un esqueleto hay que ponerselo, con su controlador.
            var animador = contenedor.GetComponent<Animator>();
            if (animador == null) animador = contenedor.gameObject.AddComponent<Animator>();
            if (animador.runtimeAnimatorController == null)
            {
                var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLADOR);
                if (ctrl == null)
                {
                    Debug.LogError($"[cambio] falta {CONTROLADOR}");
                    PrefabUtility.UnloadPrefabContents(raiz);
                    return false;
                }
                animador.runtimeAnimatorController = ctrl;
            }
            animador.avatar = avatar;
            animador.applyRootMotion = false;

            // Y el puente que traduce lo que hace el luchador a parametros del Animator.
            if (raiz.GetComponent<SmashThePast.Characters.AnimadorDeLuchador>() == null)
                raiz.AddComponent<SmashThePast.Characters.AnimadorDeLuchador>();

            // FighterVisual gira el modelo hacia el rival; sin esta referencia se queda
            // de espaldas a la camara.
            var visual = raiz.GetComponent<SmashThePast.Characters.FighterVisual>();
            if (visual != null)
            {
                var so = new SerializedObject(visual);
                so.FindProperty("model").objectReferenceValue = contenedor;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // El booleano se calcula ANTES de descargar el prefab: UnloadPrefabContents
            // destruye los objetos, y a partir de ahi el operador != de Unity da falso
            // aunque la referencia exista. Guardarla y comprobarla despues siempre falla.
            int mallas = contenedor.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
            bool bien = mallas > 0;
            Debug.Log($"[cambio] {Path.GetFileNameWithoutExtension(rutaPrefab),-24} <- {nombreModelo,-12} " +
                      $"avatar={animador.avatar.name} ctrl={animador.runtimeAnimatorController.name} mallas={mallas}");

            PrefabUtility.SaveAsPrefabAsset(raiz, rutaPrefab);
            PrefabUtility.UnloadPrefabContents(raiz);
            return bien;
        }
    }
}
