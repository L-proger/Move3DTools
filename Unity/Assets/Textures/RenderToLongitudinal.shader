Shader "Unlit/RenderToLongitudinal"
{
	Properties
	{
		_Cube("Texture", Cube) = "white" {}
		_MipOffset("_MipOffset", Float) = 1.0
		_VerticalUvScale("_VerticalUvScale", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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

			samplerCUBE _Cube;
			float _MipOffset;
			float _VerticalUvScale;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			

			float3 SphericalToCartesian(float r, float theta, float phi) {
				return float3(
					r * sin(phi) * cos(theta),
					r * cos(phi),
					r * sin(phi) * sin(theta)
					);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float pi = 3.14159265358979f;

				if (_VerticalUvScale > 0.5f) {
					i.uv.y = 1.0f - i.uv.y;
				}

				float3 uv3 = SphericalToCartesian(1.0f, i.uv.x * (2.0f * pi), i.uv.y * pi);

				float4 c = texCUBElod(_Cube, float4(uv3, _MipOffset));
				return c;
				return float4(uv3, 1);
			}
			ENDCG
		}
	}
}
