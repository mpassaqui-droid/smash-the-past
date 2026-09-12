using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SmashThePast.EditorTools
{
    // Aplica la direccion visual a la escena: luces, ambiente, post proceso y encuadre.
    // Se ejecuta sobre la escena ya generada, no la vuelve a construir.
    public static class RenderLook
    {
        private const string RutaEscena = "Assets/Stages/PrototypeArena.unity";
        private const string RutaPerfil = "Assets/Settings/PostProceso.asset";
        private const string RutaSuelo = "Assets/Settings/SueloArena.mat";

        public static void Aplicar()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            Suelo();
            Luces();
            Ambiente();
            VolumenGlobal();
            Camara();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[look] escena actualizada");
            EditorApplication.Exit(0);
        }


        // El suelo original solo cubre la franja de combate: la camara veia su canto y
        // el vacio del fondo debajo. Se alarga hacia la camara para que el primer plano
        // sea arena y no un borde gris.
        private static void Suelo()
        {
            var suelo = GameObject.Find("SueloPrototipo");
            if (suelo == null) return;
            suelo.transform.position = new Vector3(0f, -2.1f, -2.6f);
            suelo.transform.localScale = new Vector3(26f, 4.2f, 13f);

            // El suelo compartia material con el muro: todo el cuadro salia del mismo gris.
            // Un material propio de tierra batida separa primer plano y fondo.
            var renderizador = suelo.GetComponent<Renderer>();
            if (renderizador != null)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(RutaSuelo);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(material, RutaSuelo);
                }
                material.SetColor("_BaseColor", new Color(0.31f, 0.24f, 0.19f));
                material.SetColor("_Color", new Color(0.31f, 0.24f, 0.19f));
                material.SetFloat("_Smoothness", 0.08f);
                EditorUtility.SetDirty(material);
                renderizador.sharedMaterial = material;
            }
        }

        private static Light CrearLuz(string nombre, LightType tipo, Color color, float intensidad, Vector3 euler)
        {
            var existente = GameObject.Find(nombre);
            if (existente != null) Object.DestroyImmediate(existente);

            var objeto = new GameObject(nombre);
            var luz = objeto.AddComponent<Light>();
            luz.type = tipo;
            luz.color = color;
            luz.intensity = intensidad;
            luz.shadows = LightShadows.None;
            objeto.transform.rotation = Quaternion.Euler(euler);
            return luz;
        }

        private static void Luces()
        {
            // Luz principal calida, la unica que proyecta sombra.
            var antigua = GameObject.Find("LuzPrincipal");
            if (antigua != null) Object.DestroyImmediate(antigua);

            var clave = CrearLuz("LuzClave", LightType.Directional, new Color(1f, 0.94f, 0.82f), 0.95f, new Vector3(48, -38, 0));
            clave.shadows = LightShadows.Soft;
            clave.shadowStrength = 0.75f;
            RenderSettings.sun = clave;

            // Relleno frio desde el lado opuesto, sin sombra, para que la silueta no se cierre en negro.
            CrearLuz("LuzRelleno", LightType.Directional, new Color(0.62f, 0.75f, 1f), 0.20f, new Vector3(22, 140, 0));

            // Contraluz desde atras: despega a los luchadores del fondo.
            CrearLuz("LuzContra", LightType.Directional, new Color(1f, 0.86f, 0.72f), 0.40f, new Vector3(-12, 8, 0));

            // Acentos de color por jugador, para leer quien es quien de un vistazo.
            AcentoDeJugador("AcentoP1", new Vector3(-2.4f, 1.6f, -1.6f), new Color(0.42f, 0.62f, 1f));
            AcentoDeJugador("AcentoP2", new Vector3(2.4f, 1.6f, -1.6f), new Color(1f, 0.42f, 0.38f));
        }

        private static void AcentoDeJugador(string nombre, Vector3 posicion, Color color)
        {
            var existente = GameObject.Find(nombre);
            if (existente != null) Object.DestroyImmediate(existente);

            var objeto = new GameObject(nombre);
            objeto.transform.position = posicion;
            var luz = objeto.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.color = color;
            luz.intensity = 1.1f;
            luz.range = 4.5f;
            luz.shadows = LightShadows.None;
        }

        private static void Ambiente()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.33f, 0.42f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.21f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.15f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.42f, 0.5f, 0.58f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 70f;
        }

        private static void VolumenGlobal()
        {
            var perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(RutaPerfil);
            if (perfil == null)
            {
                if (!Directory.Exists("Assets/Settings")) Directory.CreateDirectory("Assets/Settings");
                perfil = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(perfil, RutaPerfil);
            }

            var tono = Obtener<Tonemapping>(perfil);
            tono.mode.overrideState = true;
            tono.mode.value = TonemappingMode.ACES;

            var color = Obtener<ColorAdjustments>(perfil);
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.10f;
            color.contrast.overrideState = true;
            color.contrast.value = 22f;
            color.saturation.overrideState = true;
            color.saturation.value = 28f;

            var bloom = Obtener<Bloom>(perfil);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.05f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.55f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.62f;

            var vineta = Obtener<Vignette>(perfil);
            vineta.intensity.overrideState = true;
            vineta.intensity.value = 0.38f;
            vineta.smoothness.overrideState = true;
            vineta.smoothness.value = 0.45f;

            var sombras = Obtener<ShadowsMidtonesHighlights>(perfil);
            sombras.shadows.overrideState = true;
            sombras.shadows.value = new Vector4(0.92f, 0.96f, 1.08f, 0f);
            sombras.highlights.overrideState = true;
            sombras.highlights.value = new Vector4(1.05f, 1.0f, 0.94f, 0f);

            EditorUtility.SetDirty(perfil);

            var existente = GameObject.Find("PostProceso");
            if (existente != null) Object.DestroyImmediate(existente);
            var objeto = new GameObject("PostProceso");
            var volumen = objeto.AddComponent<Volume>();
            volumen.isGlobal = true;
            volumen.priority = 0f;
            volumen.sharedProfile = perfil;
        }

        private static T Obtener<T>(VolumeProfile perfil) where T : VolumeComponent
        {
            return perfil.TryGet(out T componente) ? componente : perfil.Add<T>(true);
        }

        private static void Camara()
        {
            Camera camara = Camera.main;
            if (camara == null) return;

            camara.transform.position = new Vector3(0f, 2.24f, -9.2f);
            camara.transform.rotation = Quaternion.Euler(4f, 0f, 0f);
            camara.orthographic = true;
            camara.orthographicSize = 2.9f;
            camara.backgroundColor = new Color(0.16f, 0.2f, 0.27f);
            camara.allowHDR = true;

            var datos = camara.GetUniversalAdditionalCameraData();
            datos.renderPostProcessing = true;
            datos.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            datos.antialiasingQuality = AntialiasingQuality.High;
        }
    }
}
