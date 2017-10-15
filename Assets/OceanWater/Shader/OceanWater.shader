// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/OceanWater"
{
	Properties
	{
		[HideInInspector] _ReflectionTex ("Reflection Texture", 2D) = "" {}
		[NoScaleOffset] _BumpMap("Wave NormalMap", 2D) = ""{}
		[HideInInspector]_Timeelapsed("Time elapsed", Float) = 0.1
		_WaveHeight("Wave Height", Float) = 0.6
		_WaveLength("Wave Length", Float) = 0.1
		_WaveSpeed("Wave Speed", Float) = 0.1
		_WindDirection("Wind Direction", Vector) = (1,1,0,0)
		_RefrColor("Refraction Color", Color) = (0.4,0.4,0.8,1)

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
				float3 norm : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 refl_uv : TEXCOORD0;
				float3 norm : NORMAL;
				float2 normal_uv : TEXCOORD1;
				float2 normal_uv2 : TEXCOORD2;
				float3 view_dir : TEXCOORD3;

				UNITY_FOG_COORDS(1)
			};

			float _WaveHeight, _WaveLength, _WaveSpeed, _Timeelapsed;
			float4 _WindDirection;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.refl_uv = ComputeScreenPos(o.vertex);
				o.view_dir = WorldSpaceViewDir(v.vertex);
				o.norm = mul(unity_ObjectToWorld, v.norm);

				_WindDirection = normalize(_WindDirection);
				float3 perp_to_wind = cross(_WindDirection.xyz, float3(0,1,0));
				float2 temp = v.uv.xy * 2 - 1;

				float2 move_vector = float2(dot(temp, _WindDirection.xz), dot(temp, perp_to_wind.xz));
				float2 temp_vec = move_vector.yx;

				move_vector.x += _WaveSpeed * _Timeelapsed;
				o.normal_uv = (v.uv + move_vector) / _WaveLength;
				temp_vec.y += 0.25 * _Timeelapsed;
				o.normal_uv2 = (v.uv + temp_vec);// / _WaveLength;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D _ReflectionTex, _BumpMap;
			float4 _RefrColor;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				i.view_dir = normalize(i.view_dir);
				i.norm = normalize(i.norm);
				half fresnel_factor = dot(i.view_dir, i.norm);

				half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.normal_uv));
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.normal_uv2));
				half3 bumptexcol = (bump1 + bump2) * 0.5;
				//half fresnel_factor = dot(i.view_dir, normalize(bumptexcol));
				float4 perturbated_coords = i.refl_uv;
				perturbated_coords.xy += (_WaveHeight *  bumptexcol.xy);

				half4 refl_col =  tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(perturbated_coords) ) ;
				fixed4 col = lerp(refl_col, _RefrColor, fresnel_factor);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
