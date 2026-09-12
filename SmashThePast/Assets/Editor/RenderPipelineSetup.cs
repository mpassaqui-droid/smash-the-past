using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SmashThePast.EditorTools
{
    // Crea el asset de URP, lo deja activo en todos los niveles de calidad y pasa
    // los materiales del shader Standard a Universal Render Pipeline/Lit.
    public static class RenderPipelineSetup
    {
        private const string CarpetaAjustes = "Assets/Settings";
        private const string RutaRenderer = CarpetaAjustes + "/UrpRenderer.asset";
        private const string RutaPipeline = CarpetaAjustes + "/UrpPipeline.asset";

        public static void Configurar()
        {
            CrearPipeline();
            ConvertirMateriales();
            LimpiarMateriales();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[render] pipeline y materiales listos");
            EditorApplication.Exit(0);
        }

        private static void CrearPipeline()
        {
            if (!Directory.Exists(CarpetaAjustes)) Directory.CreateDirectory(CarpetaAjustes);

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RutaRenderer);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RutaRenderer);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(RutaPipeline);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, RutaPipeline);
            }

            pipeline.supportsHDR = true;
            pipeline.msaaSampleCount = 4;
            pipeline.shadowDistance = 40f;
            pipeline.shadowCascadeCount = 2;
            pipeline.useSRPBatcher = true;

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int calidadOriginal = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(calidadOriginal, false);

            EditorUtility.SetDirty(pipeline);
            EditorUtility.SetDirty(rendererData);
            Debug.Log($"[render] pipeline activo: {RutaPipeline}");
        }


        // Cambiar el shader de un material deja dentro las propiedades del shader viejo.
        // El SRP Batcher agrupa por layout de constantes y con esa basura dentro pinta
        // todos los objetos con el color de un solo material (toda la escena dorada).
        // CopySerialized reescribe el asset entero manteniendo su GUID y sus referencias.
        private static void LimpiarMateriales()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;

            int limpiados = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
                if (material == null || material.shader != lit) continue;

                Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white;
                float metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
                float smoothness = material.HasProperty("_Smoothness") ? material.GetFloat("_Smoothness") : 0.25f;

                var limpio = new Material(lit);
                limpio.SetColor("_BaseColor", color);
                limpio.SetColor("_Color", color);
                limpio.SetFloat("_Metallic", metallic);
                limpio.SetFloat("_Smoothness", smoothness);
                limpio.name = material.name;

                EditorUtility.CopySerialized(limpio, material);
                Object.DestroyImmediate(limpio);
                EditorUtility.SetDirty(material);
                limpiados++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[render] materiales reescritos limpios: {limpiados}");
        }

        private static void ConvertirMateriales()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                Debug.LogError("[render] no se encontro el shader URP/Lit");
                EditorApplication.Exit(1);
                return;
            }

            var convertidos = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
                if (material == null || material.shader == null) continue;
                if (material.shader.name != "Standard") continue;

                Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
                float metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
                float smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;

                material.shader = lit;
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Metallic", metallic);
                material.SetFloat("_Smoothness", smoothness);
                material.SetFloat("_WorkflowMode", 1f);
                EditorUtility.SetDirty(material);
                convertidos.Add(Path.GetFileNameWithoutExtension(ruta));
            }

            Debug.Log($"[render] materiales convertidos a URP/Lit: {convertidos.Count}");
        }
    }
}
