using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Saca la trayectoria de la mano fotograma a fotograma. Un golpe que se lee tiene
    /// un salto grande entre dos fotogramas seguidos (la salida) y despues casi ninguno
    /// (la retencion). Si todos los saltos son parecidos, el brazo se desliza.
    /// </summary>
    public static class MedirGolpe
    {
        public static void Ver()
        {
            var fuente = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Prefabs/TupacAmaruII_P1.prefab");
            var copia = (GameObject)Object.Instantiate(fuente);
            var anim = copia.GetComponentInChildren<Animator>();
            var piel = copia.GetComponentInChildren<SkinnedMeshRenderer>();

            foreach (var nombre in new[] { "golpe_ligero", "golpe_fuerte" })
            {
                AnimationClip clip = null;
                foreach (var g in AssetDatabase.FindAssets("t:AnimationClip", new[]{"Assets/Characters/Modelos"}))
                {
                    var r = AssetDatabase.GUIDToAssetPath(g);
                    if (r.Contains("Externos") || r.Contains("Vestuario")) continue;
                    foreach (var c in AssetDatabase.LoadAllAssetsAtPath(r).OfType<AnimationClip>())
                        if (c.name == nombre) clip = c;
                }
                if (clip == null) continue;

                // Por la correspondencia humanoide, no por nombre de hueso: los
                // modelos de Quaternius no usan los nombres mixamorig.
                var mano = anim.GetBoneTransform(nombre == "golpe_ligero"
                    ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
                if (mano == null) { Debug.LogError($"[golpe] {nombre}: no hay mano en el avatar"); continue; }
                int n = Mathf.RoundToInt(clip.length * clip.frameRate);
                Vector3 previa = Vector3.zero;
                float mayor = 0f; int dondeMayor = 0;
                var saltos = new System.Collections.Generic.List<float>();
                for (int f = 0; f <= n; f++)
                {
                    clip.SampleAnimation(anim.gameObject, f / clip.frameRate);
                    var p = mano.position;
                    if (f > 0) { float d = (p - previa).magnitude; saltos.Add(d); if (d > mayor) { mayor = d; dondeMayor = f; } }
                    previa = p;
                }
                float media = saltos.Count > 0 ? saltos.Average() : 0f;
                Debug.Log($"[golpe] {nombre}: {n} fotogramas, salto mayor {mayor:F3} m en el {dondeMayor}, " +
                          $"medio {media:F3} m, relacion pico/medio {(media > 0 ? mayor / media : 0):F1}");
            }
            Object.DestroyImmediate(copia);
            EditorApplication.Exit(0);
        }
    }
}
