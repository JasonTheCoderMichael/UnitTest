Shader "MJ/RebuildLight"
{
    Properties
    {
        _Skybox("Skybox", Cube) = "white"
    }
    
    SubShader
    {
        Tags{"LightMode" = "UniversalForward"}

        Pass
        {
            // ZTest  Always
            // ZWrite Off
            // 一定要 Cull Off //
            // Cull Off

            HLSLPROGRAM
            
            #pragma target 4.0
                        
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SHCoefficient.hlsl"

            samplerCUBE _Skybox;
            uniform float4 _SHCoefficients[9];
            uniform int _SHOrder;
            uniform float _LerpValue;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // half3 rebuiltLight : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv.xy * 2.0 - 1.0, 0.01, 1);
                #if UNITY_UV_STARTS_AT_TOP
                    o.vertex.y *= -1;
                #endif
                o.uv = v.uv;

                // o.vertex = TransformObjectToHClip(v.vertex);
                // half3 rebuiltLight = 0;
                // UNITY_LOOP
                // for(int i = 0; i < _SHOrder * _SHOrder; i++)
                // {
                //     rebuiltLight += SHBasis(i, v.normal) * _SHCoefficients[i];
                // }
                // o.rebuiltLight = rebuiltLight;

                o.normal = v.normal;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                half3 originSkyColor = texCUBE(_Skybox, normal).rgb;
                
                half3 rebuiltSkyColor = 0;
                UNITY_LOOP
                for(int i = 0; i < _SHOrder * _SHOrder; i++)
                {
                    rebuiltSkyColor += SHBasis(i, normal) * _SHCoefficients[i];
                }
                half3 finalColor = lerp(rebuiltSkyColor, originSkyColor, _LerpValue);
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}
