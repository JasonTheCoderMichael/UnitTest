Shader "Unlit/EulerAngleToForwardDirection"
{
    Properties
    {
        _VirtualLightRotation("Virtual Light Rotation", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag            

            #include "UnityCG.cginc"

            float3 _VirtualLightRotation;
            
            uniform float4 _CalculatedForward;
            uniform float4 _TransformForward;
            
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            float3x3 GetEulerAngleToRotationMatrix(float3 eulerAngle)
            {
                float3 sinTheta, cosTheta;
                sincos(eulerAngle.xyz / 180 * 3.1415926, sinTheta, cosTheta);

                float3x3 rotateAroundX = float3x3(1, 0, 0,
                                                  0, cosTheta.x, -sinTheta.x,
                                                  0, sinTheta.x, cosTheta.x);

                float3x3 rotateAroundY = float3x3(cosTheta.y, 0, sinTheta.y,
                                                  0, 1, 0,
                                                  -sinTheta.y, 0, cosTheta.y);
                
                float3x3 rotateAroundZ = float3x3(cosTheta.z, -sinTheta.z, 0,
                                                  sinTheta.z, cosTheta.z, 0,
                                                  0, 0, 1);
                                                  
                // 顺序: Z -> X -> Y //
                return mul(rotateAroundY, mul(rotateAroundX, rotateAroundZ));
            }
            
            half4 frag (v2f i) : SV_Target
            {
                return _CalculatedForward + _WorldSpaceLightPos0 + _TransformForward;
            
                float3x3 eulerToRotateMatrix = GetEulerAngleToRotationMatrix(_VirtualLightRotation.xyz);
                float3 rotatedForward = mul(eulerToRotateMatrix, float3(0, 0, 1));

                float threshold = 0.000001;
                float3 delta = abs(rotatedForward.xyz - _TransformForward.xyz);
                if(any(delta > threshold.xxx))
                    return half4(1, 0, 0, 1);

                return half4(0, 0, 1, 1);
            }
            ENDCG
        }
    }
}
