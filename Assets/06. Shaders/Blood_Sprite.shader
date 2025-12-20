Shader "BloodSystem/Blood_Sprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BloodMask ("Blood Mask", 2D) = "black" {}
        _BloodColor ("Blood Color", Color) = (0.4, 0.05, 0.05, 1)
        _BloodSpecularColor ("Blood Specular Color", Color) = (0.8, 0.1, 0.1, 1)
        _BloodGlossiness ("Blood Glossiness", Range(0, 1)) = 0.6
        _BloodSpecularIntensity ("Blood Specular Intensity", Range(0, 5)) = 1.5
        _SpriteUVMin ("Sprite UV Min", Vector) = (0, 0, 0, 0)
        _SpriteUVMax ("Sprite UV Max", Vector) = (1, 1, 0, 0)

        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Sprite Lit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
            #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
            #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
            #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BloodMask);
            SAMPLER(sampler_BloodMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BloodColor;
                float4 _BloodSpecularColor;
                float _BloodGlossiness;
                float _BloodSpecularIntensity;
                float4 _SpriteUVMin;
                float4 _SpriteUVMax;
                half4 _RendererColor;
            CBUFFER_END

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif
            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lightingUV : TEXCOORD1;
                float4 color : COLOR;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.positionWS = TransformObjectToWorld(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.lightingUV = input.uv;
                output.color = input.color * _RendererColor;

                return output;
            }

            float FresnelEffect(float3 normal, float3 viewDir, float power)
            {
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                return pow(fresnel, power);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Atlas UV 정규화
                float2 normalizedUV = (input.uv - _SpriteUVMin.xy) / (_SpriteUVMax.xy - _SpriteUVMin.xy);

                // 원본 스프라이트 색상
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // 피 마스크 샘플링 (정규화된 UV 사용)
                float bloodMask = SAMPLE_TEXTURE2D(_BloodMask, sampler_BloodMask, normalizedUV).r;

                // 피 색상 블렌딩
                half4 finalColor = lerp(mainTex, _BloodColor, bloodMask);
                finalColor *= input.color;

                // 2D 라이팅 적용
                half2 lightingColor = half2(1, 1);

                #if defined(USE_SHAPE_LIGHT_TYPE_0) || defined(USE_SHAPE_LIGHT_TYPE_1) || defined(USE_SHAPE_LIGHT_TYPE_2) || defined(USE_SHAPE_LIGHT_TYPE_3)
                    float2 positionWS = input.positionWS.xy;

                    #if USE_SHAPE_LIGHT_TYPE_0
                        lightingColor *= ComputeShapeLightColor(positionWS, 0, input.lightingUV);
                    #endif
                    #if USE_SHAPE_LIGHT_TYPE_1
                        lightingColor *= ComputeShapeLightColor(positionWS, 1, input.lightingUV);
                    #endif
                    #if USE_SHAPE_LIGHT_TYPE_2
                        lightingColor *= ComputeShapeLightColor(positionWS, 2, input.lightingUV);
                    #endif
                    #if USE_SHAPE_LIGHT_TYPE_3
                        lightingColor *= ComputeShapeLightColor(positionWS, 3, input.lightingUV);
                    #endif
                #endif

                finalColor.rgb *= lightingColor.x;

                // 피 영역에 Specular 효과 추가 (젖은 느낌)
                if (bloodMask > 0.01)
                {
                    // 간단한 뷰 방향 (2D이므로 카메라는 항상 정면)
                    float3 viewDir = float3(0, 0, 1);
                    float3 normal = float3(0, 0, 1);

                    // Fresnel 효과로 젖은 느낌 표현
                    float fresnel = FresnelEffect(normal, viewDir, 5.0 - _BloodGlossiness * 4.0);
                    float3 specular = _BloodSpecularColor.rgb * fresnel * _BloodSpecularIntensity * bloodMask;

                    finalColor.rgb += specular * lightingColor.x;
                }

                // 프리멀티플라이 알파
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
