using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SmashThePast.EditorTools
{
    /// <summary>
    /// Cuelga las piezas de vestuario del hueso que les toca. Cada pieza es una malla
    /// rigida, sin pesos: sigue a su hueso y ya esta, que para un poncho sobre un torso
    /// de pocos poligonos se ve igual de bien y no obliga a pintar pesos.
    ///
    /// La malla trae sus coordenadas del mundo horneadas (se modelo sobre el esqueleto
    /// en reposo), asi que despues de colgarla del hueso hay que ponerle la posicion del
    /// MUNDO a cero. Si se pone la local a cero, la pieza se va al doble de altura.
    /// </summary>
    public static class VestuarioSetup
    {
        // personaje -> (pieza, hueso, colores por submalla)
        static readonly (string personaje, string pieza, string hueso)[] PIEZAS =
        {
            ("TupacAmaruII", "TupacAmaruII_poncho", "Chest"),
        };

        public static void Poner()
        {
            int puestas = 0;
            foreach (var (personaje, pieza, hueso) in PIEZAS)
            {
                string rutaFbx = $"Assets/Characters/Modelos/Vestuario/{pieza}.fbx";
                var fuente = AssetDatabase.LoadAssetAtPath<GameObject>(rutaFbx);
                if (fuente == null) { Debug.LogError($"[vestuario] falta {rutaFbx}"); EditorApplication.Exit(1); return; }

                foreach (string lado in new[] { "P1", "P2" })
                {
                    string ruta = $"Assets/Characters/Prefabs/{personaje}_{lado}.prefab";
                    var raiz = PrefabUtility.LoadPrefabContents(ruta);

                    // Quitar una version anterior de la pieza antes de poner la nueva.
                    foreach (var viejo in raiz.GetComponentsInChildren<Transform>(true)
                                              .Where(t => t.name.StartsWith(pieza)).ToArray())
                        Object.DestroyImmediate(viejo.gameObject);

                    var destino = raiz.GetComponentsInChildren<Transform>(true)
                                      .FirstOrDefault(t => t.name == hueso);
                    if (destino == null)
                    {
                        Debug.LogError($"[vestuario] {ruta} no tiene hueso '{hueso}'");
                        PrefabUtility.UnloadPrefabContents(raiz);
                        EditorApplication.Exit(1); return;
                    }

                    var nueva = (GameObject)PrefabUtility.InstantiatePrefab(fuente, destino);
                    PrefabUtility.UnpackPrefabInstance(nueva, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    nueva.name = pieza;
                    // Hay que anular las TRES cosas que hereda del hueso. Poner solo
                    // la posicion y la rotacion deja la escala del padre: los FBX de
                    // Quaternius llevan un factor 100 en la raiz, y la pieza se iba a
                    // y = -5,50 en vez de quedarse a la altura del pecho.
                    nueva.transform.position = Vector3.zero;
                    // NO identidad: el FBX de Blender llega con su rotacion de
                    // conversion de ejes (Z arriba -> Y arriba) en la raiz. Ponerla a
                    // identidad la anula y la pieza queda tumbada, con el centro en z
                    // en vez de en y. Se copia la del modelo tal como Unity lo importo.
                    nueva.transform.rotation = fuente.transform.rotation;
                    var escalaPadre = destino.lossyScale;
                    nueva.transform.localScale = new Vector3(
                        escalaPadre.x != 0f ? 1f / escalaPadre.x : 1f,
                        escalaPadre.y != 0f ? 1f / escalaPadre.y : 1f,
                        escalaPadre.z != 0f ? 1f / escalaPadre.z : 1f);

                    // Color por nombre de submalla, no por indice: si manana cambia el
                    // orden en el script de Blender, esto sigue poniendo el rojo en la
                    // tela y el oro en la franja.
                    var rend = nueva.GetComponentInChildren<MeshRenderer>();
                    if (rend != null)
                    {
                        var mats = rend.sharedMaterials;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            if (mats[i] == null) continue;
                            string sub = mats[i].name;
                            Color color = sub.Contains("franja") ? new Color(0.88f, 0.62f, 0.14f)
                                        : sub.Contains("cuello") ? new Color(0.10f, 0.09f, 0.08f)
                                        : new Color(0.62f, 0.09f, 0.10f);
                            string destinoMat = $"Assets/Characters/Materiales/{personaje}/{sub}.mat";
                            var mat = AssetDatabase.LoadAssetAtPath<Material>(destinoMat);
                            if (mat == null)
                            {
                                mat = new Material(mats[i]);
                                AssetDatabase.CreateAsset(mat, destinoMat);
                            }
                            mat.color = color;
                            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                            EditorUtility.SetDirty(mat);
                            mats[i] = mat;
                        }
                        rend.sharedMaterials = mats;
                    }

                    var r = rend;
                    Debug.Log($"[vestuario] {personaje}_{lado} <- {pieza} en '{hueso}' " +
                              $"submallas={(r != null ? r.sharedMaterials.Length : 0)} " +
                              $"centro={(r != null ? r.bounds.center.ToString("F2") : "?")} tam={(r != null ? r.bounds.size.ToString("F2") : "?")}");

                    PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
                    PrefabUtility.UnloadPrefabContents(raiz);
                    puestas++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[vestuario] PIEZAS_PUESTAS {puestas}");
            EditorApplication.Exit(0);
        }
    }
}
