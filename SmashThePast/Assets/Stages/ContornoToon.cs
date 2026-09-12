using System.Collections.Generic;
using System.Linq;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Stages
{
    // Anade el contorno negro a los luchadores. El material del contorno se mete como
    // segundo material del renderer: Unity lo dibuja en una pasada aparte y el material
    // original no se toca, asi que los colores y el SRP Batcher siguen igual.
    [DefaultExecutionOrder(120)]
    public sealed class ContornoToon : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField, Range(0f, 0.06f)] private float grosor = 0.022f;
        [SerializeField] private Color color = new Color(0.05f, 0.05f, 0.07f);

        private Material material;
        private readonly HashSet<Renderer> tratados = new HashSet<Renderer>();

        private void Awake()
        {
            var shader = Resources.Load<Shader>("ContornoToon");
            if (shader == null)
            {
                Debug.LogError("[contorno] falta el shader ContornoToon en Resources", this);
                enabled = false;
                return;
            }

            material = new Material(shader) { name = "ContornoToon" };
            material.SetColor("_ColorContorno", color);
            material.SetFloat("_Grosor", grosor);
        }

        private void Update()
        {
            if (match == null || material == null) return;
            Aplicar(match.PlayerOne);
            Aplicar(match.PlayerTwo);
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }

        private void Aplicar(Fighter luchador)
        {
            if (luchador == null) return;
            foreach (var pieza in luchador.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (pieza == null || tratados.Contains(pieza)) continue;
                tratados.Add(pieza);

                var materiales = pieza.sharedMaterials;
                if (materiales.Any(m => m == material)) continue;

                var ampliado = new Material[materiales.Length + 1];
                materiales.CopyTo(ampliado, 0);
                ampliado[materiales.Length] = material;
                pieza.sharedMaterials = ampliado;
            }
        }
    }
}
