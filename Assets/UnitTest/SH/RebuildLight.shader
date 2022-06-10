Shader "MJ/RebuildLight"
{
    Properties
    {
    }
    
    SubShader
    {
        Tags{"LightMode" = "UniversalForward"}

        Pass
        {
            ZTest  Always
            ZWrite Off
            // 一定要 Cull Off //
            Cull Off

            HLSLPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma target 3.0
                        
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SHCoefficient.hlsl"

            float4 _SHCoefficients[];
            int _SHDegree;

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
                half3 rebuiltLight : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv.xy * 2.0 - 1.0, 0.01, 1);
                #if UNITY_UV_STARTS_AT_TOP
                    o.vertex.y *= -1;
                #endif
                o.uv = v.uv;

                half3 rebuiltLight = 0;
                for(int i = 0; i < _SHDegree * _SHDegree; i++)
                {
                    rebuiltLight += SHBasis(i, v.normal) * _SHCoefficients[i];
                }
                o.rebuiltLight = rebuiltLight;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return half4(i.rebuiltLight, 1);
            }
            ENDHLSL
        }
    }
}
