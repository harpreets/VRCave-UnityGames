Shader "Infinite Runner/Diffuse Curve" {
	Properties {
		_MainTex ("Main Texture", 2D) = "black" {}
		_NearCurve ("Near Curve", Float) = 0.0
		_FarCurve ("Far Curve", Float) = 0.0
		_Dist ("Distance Mod", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
        Pass { // pass 0

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma multi_compile_fwdbase LIGHTMAP_OFF LIGHTMAP_ON
			#include "UnityCG.cginc"
			#ifndef SHADOWS_OFF		
			#include "AutoLight.cginc"	
			#endif
						
			uniform sampler2D _MainTex;
			uniform sampler2D _MainTex_ST;
			uniform sampler2D unity_Lightmap;
			uniform float4 	_MainTex_TexelSize;
			uniform half4 unity_LightmapST;
			uniform float _NearCurve;
			uniform float _FarCurve;
			uniform float _Dist;

			uniform float4 _LightColor0;
			
			struct fragmentInput
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uvLM : TEXCOORD1;
				#ifndef SHADOWS_OFF
		        LIGHTING_COORDS(2,3)
				#endif
			};
						
			fragmentInput vert(appdata_full v)
			{
				fragmentInput o;

				// Apply the curve
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
                float distanceSquared = pos.z * pos.z * _Dist;
                pos.x += (_FarCurve - max(1.0 - distanceSquared / _NearCurve, 0.0) * _FarCurve);
                o.pos = mul(UNITY_MATRIX_P, pos); 
				o.uv = v.texcoord;
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1.0-o.uv.y;
				#endif
				
				#ifndef SHADOWS_OFF			  	
      			TRANSFER_VERTEX_TO_FRAGMENT(o);
				#endif

				return o;
			}
			
			fixed4 frag(fragmentInput i) : COLOR
			{
				fixed4 color = tex2D(_MainTex, i.uv);

				#ifdef LIGHTMAP_ON
				fixed3 lm = DecodeLightmap (tex2D (unity_Lightmap, i.uvLM));
				color.rgb *= lm;
				#endif
				
				#ifndef SHADOWS_OFF			  	
				fixed atten = LIGHT_ATTENUATION(i);
				color.rgb *= atten;
				#endif

				return color;
			}
			
			ENDCG
        } // end pass
	} 
	FallBack "Diffuse"
}
