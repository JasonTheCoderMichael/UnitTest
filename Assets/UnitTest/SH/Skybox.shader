Shader "MJ/Skybox"
{
	Properties
	{
		_SkyboxTex("Skybox Texture", Cube) = ""
	}

	SubShader
	{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off 
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			samplerCUBE _SkyboxTex;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.vertex.xyz;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 skyColor = texCUBE(_SkyboxTex, i.texcoord);
				return skyColor;
			}
			ENDCG
		}
	}
	
	Fallback Off
}
