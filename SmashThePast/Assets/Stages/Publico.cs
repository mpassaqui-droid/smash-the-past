using UnityEngine;

namespace SmashThePast.Stages
{
    /// <summary>
    /// Llena las gradas. Eran bloques vacios, que en un juego de lucha se lee como
    /// escenario sin terminar.
    ///
    /// Se intento con `Graphics.RenderMeshInstanced` y en el editor todo parecia bien
    /// (instancing soportado, shader URP/Lit, 120 matrices), pero en la BUILD no se
    /// dibujaba nada: la variante instanciada del shader se queda fuera al compilar.
    /// Con objetos de verdad funciona, y 240 mallas en una escena que ya tiene dos mil
    /// no se notan. El balanceo lo hace un solo script moviendo transforms.
    ///
    /// Las medidas salen del constructor del escenario: tres gradas en z 4,4 / 6,3 /
    /// 8,2, con la superficie en y 0,55 / 1,25 / 1,95, y 27 de ancho.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Publico : MonoBehaviour
    {
        [SerializeField] private int porGrada = 24;
        [SerializeField] private float ancho = 24f;
        [SerializeField] private Material plantilla;

        static readonly float[] AlturaDeGrada = { 0.55f, 1.25f, 1.95f };
        static readonly float[] ProfundidadDeGrada = { 4.4f, 6.3f, 8.2f };

        static readonly Color[] Ropas =
        {
            new Color(0.62f, 0.24f, 0.22f), new Color(0.24f, 0.32f, 0.46f),
            new Color(0.70f, 0.62f, 0.42f), new Color(0.34f, 0.42f, 0.30f),
            new Color(0.52f, 0.46f, 0.52f), new Color(0.72f, 0.54f, 0.30f),
        };
        static readonly Color Piel = new Color(0.66f, 0.48f, 0.34f);

        struct Espectador { public Transform cuerpo, cabeza; public float fase, ritmo, base_; }

        Espectador[] gente;

        private void Awake()
        {
            var plantillaMat = plantilla != null ? plantilla : BuscarPlantilla();
            if (plantillaMat == null) { Debug.LogWarning("[publico] sin material de referencia"); enabled = false; return; }

            var ropas = new Material[Ropas.Length];
            for (int c = 0; c < Ropas.Length; c++) ropas[c] = Vestir(plantillaMat, Ropas[c]);
            var piel = Vestir(plantillaMat, Piel);

            var padre = new GameObject("Publico").transform;
            padre.SetParent(transform, false);

            int total = porGrada * AlturaDeGrada.Length;
            gente = new Espectador[total];
            var azar = new System.Random(20260915);

            int i = 0;
            for (int g = 0; g < AlturaDeGrada.Length; g++)
                for (int n = 0; n < porGrada; n++, i++)
                {
                    float t = porGrada == 1 ? 0.5f : n / (float)(porGrada - 1);
                    // Un poco de desorden: una fila perfecta se lee como una valla.
                    float x = (t - 0.5f) * ancho + (float)(azar.NextDouble() - 0.5) * 0.35f;
                    // DETRAS del frente de la grada, no encima: puestos en la cara de
                    // delante formaban un muro de gente que tapaba el combate.
                    float z = ProfundidadDeGrada[g] + 0.75f + (float)(azar.NextDouble() - 0.5) * 0.4f;
                    float escala = 0.58f + (float)azar.NextDouble() * 0.16f;
                    float alto = 0.62f * escala;
                    var color = ropas[azar.Next(ropas.Length)];

                    var cuerpo = Caja(padre, "Cuerpo", new Vector3(0.34f, alto, 0.26f) * escala, color);
                    var cabeza = Caja(padre, "Cabeza", Vector3.one * 0.22f * escala, piel);
                    float baseY = AlturaDeGrada[g] + alto * 0.5f;
                    cuerpo.position = new Vector3(x, baseY, z);
                    cabeza.position = new Vector3(x, AlturaDeGrada[g] + alto + 0.11f * escala, z);

                    gente[i] = new Espectador
                    {
                        cuerpo = cuerpo, cabeza = cabeza,
                        fase = (float)azar.NextDouble() * 6.283f,
                        ritmo = 1.4f + (float)azar.NextDouble() * 1.3f,
                        base_ = baseY,
                    };
                }

            var uno = gente[0].cuerpo.GetComponent<MeshRenderer>();
            Debug.Log($"[publico] {total} espectadores, {total * 2} mallas. " +
                      $"escala del padre {transform.lossyScale}, " +
                      $"un espectador mide {uno.bounds.size.y:F2} m de cuerpo y esta en y {uno.bounds.center.y:F2}");
        }

        private static Transform Caja(Transform padre, string nombre, Vector3 escala, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nombre;
            DestroyImmediate(go.GetComponent<Collider>());   // nadie choca con el publico
            go.transform.SetParent(padre, false);
            go.transform.localScale = escala;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.transform;
        }

        private Material Vestir(Material plantillaMat, Color color)
        {
            var m = new Material(plantillaMat);
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", null);
            return m;
        }

        private Material BuscarPlantilla()
        {
            // Se copia el shader de algo que ya se dibuja en esta escena, en vez de
            // buscarlo por nombre: Shader.Find puede no encontrarlo en una build.
            foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
                if (r.sharedMaterial != null) return r.sharedMaterial;
            return null;
        }

        private void Update()
        {
            if (gente == null) return;
            float t = Time.time;
            foreach (var e in gente)
            {
                // El publico respira: sin esto las gradas se leen como una fila de cajas.
                float sube = Mathf.Sin(t * e.ritmo + e.fase) * 0.045f;
                var p = e.cuerpo.position; p.y = e.base_ + sube; e.cuerpo.position = p;
                var q = e.cabeza.position; q.y = e.base_ + (e.cuerpo.localScale.y * 0.5f) + 0.11f + sube; e.cabeza.position = q;
            }
        }

    }
}
