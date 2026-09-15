using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Enciende lo que estaba apagado: la oclusion ambiental del renderizador y los
    /// efectos del volumen de post proceso. Los dos existian vacios, asi que esto no
    /// sustituye nada, solo deja de desaprovecharlo.
    ///
    /// La oclusion ambiental es lo que hace que un personaje parezca APOYADO en el
    /// suelo en vez de pegado encima. El tonemapping y el etalonaje son lo que separa
    /// una imagen de juego de un render crudo.
    /// </summary>
    public static class RenderSetup2
    {
        const string RENDERER = "Assets/Settings/UrpRenderer.asset";
        const string PERFIL = "Assets/Settings/PostProceso.asset";

        public static void Encender()
        {
            var datos = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RENDERER);
            if (datos == null) { Debug.LogError($"[render] no carga {RENDERER}"); EditorApplication.Exit(1); return; }

            if (!datos.rendererFeatures.Any(f => f is ScreenSpaceAmbientOcclusion))
            {
                var ssao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ssao.name = "OclusionAmbiental";
                AssetDatabase.AddObjectToAsset(ssao, datos);
                datos.rendererFeatures.Add(ssao);
                EditorUtility.SetDirty(datos);
                Debug.Log("[render] oclusion ambiental anadida");
            }
            else Debug.Log("[render] la oclusion ambiental ya estaba");

            // SIN ESTO NO HAY POST PROCESO, y URP no avisa: el campo estaba a nulo y
            // el volumen entero se ignoraba en silencio. Aqui viven los shaders que
            // necesita el efecto, y vienen con el propio paquete.
            if (datos.postProcessData == null)
            {
                const string RUTA_PPD = "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";
                var ppd = AssetDatabase.LoadAssetAtPath<PostProcessData>(RUTA_PPD);
                if (ppd == null) { Debug.LogError($"[render] no carga {RUTA_PPD}"); EditorApplication.Exit(1); return; }
                datos.postProcessData = ppd;
                EditorUtility.SetDirty(datos);
                Debug.Log("[render] PostProcessData asignado (estaba vacio)");
            }
            else Debug.Log("[render] PostProcessData ya estaba");

            var perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PERFIL);
            if (perfil == null) { Debug.LogError($"[render] no carga {PERFIL}"); EditorApplication.Exit(1); return; }

            // Tonemapping: sin el, los brillos se recortan a blanco plano.
            var tono = Obtener<Tonemapping>(perfil);
            tono.mode.overrideState = true;
            tono.mode.value = TonemappingMode.ACES;

            // Etalonaje: un punto de contraste y algo de saturacion atan la paleta.
            var color = Obtener<ColorAdjustments>(perfil);
            color.postExposure.overrideState = true; color.postExposure.value = 0.15f;
            color.contrast.overrideState = true;     color.contrast.value = 12f;
            color.saturation.overrideState = true;   color.saturation.value = 6f;

            // Bloom discreto: solo en lo que de verdad brilla, no una neblina general.
            var brillo = Obtener<Bloom>(perfil);
            brillo.threshold.overrideState = true; brillo.threshold.value = 1.05f;
            brillo.intensity.overrideState = true; brillo.intensity.value = 0.55f;
            brillo.scatter.overrideState = true;   brillo.scatter.value = 0.62f;

            // Vineta: empuja la mirada al centro, que es donde pelean.
            var vineta = Obtener<Vignette>(perfil);
            vineta.intensity.overrideState = true; vineta.intensity.value = 0.26f;
            vineta.smoothness.overrideState = true; vineta.smoothness.value = 0.45f;

            EditorUtility.SetDirty(perfil);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[render] efectos en el perfil: {string.Join(", ", perfil.components.Select(c => c.GetType().Name))}");
            Debug.Log($"[render] caracteristicas del renderizador: {datos.rendererFeatures.Count}");
            Debug.Log("[render] LISTO");
            EditorApplication.Exit(0);
        }

        /// <summary>
        /// `VolumeProfile.Add` crea el componente en memoria pero NO lo mete en el
        /// asset: al guardar se pierde y el volumen queda vacio. Hay que anadirlo como
        /// sub-asset a mano, que es lo que hace el inspector de Unity por dentro.
        /// </summary>
        static T Obtener<T>(VolumeProfile perfil) where T : VolumeComponent
        {
            if (perfil.TryGet<T>(out var existente)) return existente;
            var nuevo = perfil.Add<T>(true);
            nuevo.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(nuevo, perfil);
            return nuevo;
        }
    }
}
