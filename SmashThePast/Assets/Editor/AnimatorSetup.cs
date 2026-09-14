using System.Linq;
using SmashThePast.Characters;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Monta el Animator del luchador y cambia el modelo de cubos de Tupac por el
    /// modelo de Blender. Todo por script, para que se pueda repetir con los otros ocho.
    /// </summary>
    public static class AnimatorSetup
    {
        private const string Carpeta = "Assets/Characters/Modelos";
        private const string RutaControlador = Carpeta + "/Luchador.controller";
        private const string Modelo = Carpeta + "/TupacAmaruII.fbx";

        public static void Configurar()
        {
            var controlador = ConstruirControlador();
            foreach (int jugador in new[] { 1, 2 })
                CambiarPrefab($"Assets/Characters/Prefabs/TupacAmaruII_P{jugador}.prefab", controlador);

            AssetDatabase.SaveAssets();
            Debug.Log("[animator] ANIMATOR_LISTO");
            EditorApplication.Exit(0);
        }

        private static AnimationClip Clip(string nombre)
        {
            string ruta = $"{Carpeta}/TupacAmaruII@{nombre}.fbx";
            return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview"));
        }

        private static AnimatorController ConstruirControlador()
        {
            var controlador = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador)
                ?? AnimatorController.CreateAnimatorControllerAtPath(RutaControlador);

            var maquina = controlador.layers[0].stateMachine;
            foreach (var estado in maquina.states.ToArray()) maquina.RemoveState(estado.state);
            foreach (var parametro in controlador.parameters.ToArray()) controlador.RemoveParameter(parametro);

            controlador.AddParameter("velocidad", AnimatorControllerParameterType.Float);
            controlador.AddParameter("guardia", AnimatorControllerParameterType.Bool);
            foreach (var disparador in new[] { "ligero", "fuerte", "encajar", "ko" })
                controlador.AddParameter(disparador, AnimatorControllerParameterType.Trigger);

            var idle = maquina.AddState("idle");
            idle.motion = Clip("idle");
            maquina.defaultState = idle;

            var andar = maquina.AddState("andar");
            andar.motion = Clip("andar");

            var guardia = maquina.AddState("guardia");
            guardia.motion = Clip("guardia");

            // Umbral bajo: el personaje se mueve despacio y con 0.5 nunca entraria.
            var aAndar = idle.AddTransition(andar);
            aAndar.AddCondition(AnimatorConditionMode.Greater, 0.15f, "velocidad");
            aAndar.hasExitTime = false;
            aAndar.duration = 0.08f;

            var aQuieto = andar.AddTransition(idle);
            aQuieto.AddCondition(AnimatorConditionMode.Less, 0.15f, "velocidad");
            aQuieto.hasExitTime = false;
            aQuieto.duration = 0.08f;

            foreach (var origen in new[] { idle, andar })
            {
                var aGuardia = origen.AddTransition(guardia);
                aGuardia.AddCondition(AnimatorConditionMode.If, 0, "guardia");
                aGuardia.hasExitTime = false;
                aGuardia.duration = 0.05f;
            }
            var saleGuardia = guardia.AddTransition(idle);
            saleGuardia.AddCondition(AnimatorConditionMode.IfNot, 0, "guardia");
            saleGuardia.hasExitTime = false;
            saleGuardia.duration = 0.08f;

            // Golpes, encajar y KO entran desde cualquier sitio: un golpe no puede
            // esperar a que termine lo anterior o se pierde el disparo.
            foreach (var (nombre, disparador) in new[]
                     { ("golpe_ligero", "ligero"), ("golpe_fuerte", "fuerte"), ("encajar", "encajar"), ("ko", "ko") })
            {
                var estado = maquina.AddState(nombre);
                estado.motion = Clip(nombre);

                var entrada = maquina.AddAnyStateTransition(estado);
                entrada.AddCondition(AnimatorConditionMode.If, 0, disparador);
                entrada.hasExitTime = false;
                entrada.duration = 0.04f;
                entrada.canTransitionToSelf = false;

                if (nombre == "ko") continue;   // el KO se queda en el suelo
                var salida = estado.AddTransition(idle);
                salida.hasExitTime = true;
                salida.exitTime = 0.9f;
                salida.duration = 0.08f;
            }

            EditorUtility.SetDirty(controlador);
            return controlador;
        }

        private static void CambiarPrefab(string ruta, AnimatorController controlador)
        {
            var contenido = PrefabUtility.LoadPrefabContents(ruta);
            try
            {
                var viejo = contenido.transform.Find("Modelo");
                if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

                var fuente = AssetDatabase.LoadAssetAtPath<GameObject>(Modelo);
                var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fuente, contenido.transform);
                modelo.name = "Modelo";
                modelo.transform.localPosition = Vector3.zero;
                modelo.transform.localRotation = Quaternion.identity;

                // Unity culea la malla con los limites de la pose de union. Al animar el
                // esqueleto esos limites se quedan viejos y el personaje desaparece aunque
                // este delante de la camara.
                foreach (var piel in modelo.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    piel.updateWhenOffscreen = true;

                var animador = modelo.GetComponent<Animator>() ?? modelo.AddComponent<Animator>();
                animador.runtimeAnimatorController = controlador;
                animador.avatar = AssetDatabase.LoadAssetAtPath<Avatar>(Modelo);
                animador.applyRootMotion = false;

                // El movimiento a mano y el Animator no pueden convivir: empujarian el
                // mismo transform en direcciones distintas.
                var aMano = contenido.GetComponent<FighterAnimation>();
                if (aMano != null) Object.DestroyImmediate(aMano);

                var puente = contenido.GetComponent<AnimadorDeLuchador>() ?? contenido.AddComponent<AnimadorDeLuchador>();
                var datos = new SerializedObject(puente);
                datos.FindProperty("animador").objectReferenceValue = animador;
                datos.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(contenido, ruta);
                Debug.Log($"[animator] {System.IO.Path.GetFileName(ruta)} cambiado");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contenido);
            }
        }
    }
}
