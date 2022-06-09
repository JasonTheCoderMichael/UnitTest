Shader "MJ/UnfoldUV"
{
    Properties
    {
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZTest  Always
            ZWrite Off
            // 一定要 Cull Off //
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv.xy * 2.0 - 1.0, 0.01, 1);
                #if UNITY_UV_STARTS_AT_TOP
                    o.vertex.y *= -1;
                #endif
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv.xy;
                return half4(uv, 0, 1);
            }
            ENDCG
        }
    }
}
