using UnityEngine;

namespace SmashThePast.Stages
{
    /// <summary>
    /// Da a cada bloque del escenario un tamano de textura constante en metros.
    ///
    /// El escenario son cubos primitivos escalados sin proporcion (una grada puede
    /// medir 20 x 0,3 x 1). Los UV de un cubo son fijos, asi que la textura se estira
    /// tanto como el cubo: en las franjas largas se veian brochazos en vez de hierba.
    ///
    /// La correccion va por objeto, con un MaterialPropertyBlock, asi que no duplica
    /// materiales. Se ajusta la cara que se ve: en un juego de perfil casi siempre es
    /// la de delante, o sea X por Y.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EscalaDeTextura : MonoBehaviour
    {
        [SerializeField, Tooltip("Repeticiones de la textura por metro")]
        private float repeticionesPorMetro = 0.4f;

        private void Awake() => Aplicar();

        public void Aplicar()
        {
            var bloque = new MaterialPropertyBlock();
            int tocados = 0;
            foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
            {
                var escala = r.transform.lossyScale;
                var st = new Vector4(
                    Mathf.Max(0.01f, Mathf.Abs(escala.x) * repeticionesPorMetro),
                    Mathf.Max(0.01f, Mathf.Abs(escala.y) * repeticionesPorMetro),
                    0f, 0f);
                r.GetPropertyBlock(bloque);
                bloque.SetVector("_BaseMap_ST", st);
                bloque.SetVector("_MainTex_ST", st);
                r.SetPropertyBlock(bloque);
                tocados++;
            }
            if (tocados > 0) Debug.Log($"[textura] escala por objeto aplicada a {tocados} bloques");
        }
    }
}
