Shader "Hidden/BloodSystem/SplatBlit"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _SplatTex ("Splatter Texture", 2D) = "white" {}
        _SplatRect ("Splat Rect (Center XY, Size ZW)", Vector) = (0.5, 0.5, 0.2, 0.2)
        _SplatRotation ("Splat Rotation (Radians)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "SplatBlit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SplatTex);
            SAMPLER(sampler_SplatTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SplatRect; // xy = center, zw = size
                float _SplatRotation;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            float2 RotateUV(float2 uv, float rotation, float2 center)
            {
                float cosRot = cos(rotation);
                float sinRot = sin(rotation);
                float2 offset = uv - center;
                float2 rotated;
                rotated.x = offset.x * cosRot - offset.y * sinRot;
                rotated.y = offset.x * sinRot + offset.y * cosRot;
                return rotated + center;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 기존 마스크 값
                half4 existing = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // 스플래터 UV 계산
                float2 splatCenter = _SplatRect.xy;
                float2 splatSize = _SplatRect.zw;

                // 회전 적용
                float2 rotatedUV = RotateUV(input.uv, _SplatRotation, splatCenter);

                // 스플래터 영역 내인지 확인
                float2 splatUV = (rotatedUV - splatCenter) / splatSize + 0.5;

                // 범위 체크
                half splatValue = 0;
                if (splatUV.x >= 0 && splatUV.x <= 1 && splatUV.y >= 0 && splatUV.y <= 1)
                {
                    splatValue = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, splatUV).r;
                }

                // Max 블렌딩 (누적)
                half finalValue = max(existing.r, splatValue);

                return half4(finalValue, finalValue, finalValue, 1);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
