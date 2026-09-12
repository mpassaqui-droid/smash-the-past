// Contorno por casco invertido: dibuja las caras traseras de la misma malla,
// desplazadas hacia fuera por la normal, en negro. Se anade como segundo
// material del renderer, asi que el material original no se toca.
// El desplazamiento es en espacio de mundo, no de objeto: los personajes estan
// hechos de primitivas con escalas no uniformes y en espacio de objeto el
// contorno saldria mas gordo en unos trozos que en otros. Con camara ortografica
// un grosor en metros equivale a un grosor constante en pantalla.
Shader "SmashThePast/ContornoToon"
{
    Properties
    {
        _ColorContorno ("Color del contorno", Color) = (0.04, 0.04, 0.06, 1)
        _Grosor ("Grosor en metros", Range(0, 0.08)) = 0.018
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "Contorno"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorContorno;
                float _Grosor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz) + normalWS * _Grosor;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(_ColorContorno.rgb, 1);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
