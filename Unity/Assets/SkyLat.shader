// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SkyLat"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			inline float2 RadialCoords(float3 a_coords)
			{
				float3 a_coords_n = normalize(a_coords);
				float lon = atan2(a_coords_n.z, a_coords_n.x);
				float lat = acos(a_coords_n.y);
				float2 sphereCoords = float2(lon, lat) / 3.14159265358979f;
				return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y) ;
			}

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 worldRefl : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				float3 V = normalize(_WorldSpaceCameraPos.xyz - worldPos);

				o.worldRefl = -normalize(reflect(V, normalize(v.normal)));

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return float4(i.worldRefl, 1);
				float2 uv0 = RadialCoords(normalize(i.worldRefl));

				float2 uv1 = uv0;
				if (uv1.x < 0.25) {
					uv1.x += 1.0f;
				}


				float2 d0 = length(ddx(uv0) + ddy(uv0));
				float2 d1 = length(ddx(uv1) + ddy(uv1));
				float2 uv = lerp(uv0, uv1, d0 > d1);
				//float2 uv = uv0;

				// sample the texture
				fixed4 col = tex2D(_MainTex, uv);
				return col;
			}
			ENDCG
		}
	}
}
