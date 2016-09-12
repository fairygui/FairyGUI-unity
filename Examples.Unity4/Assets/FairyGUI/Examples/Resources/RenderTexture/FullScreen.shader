Shader "Game/FullScreen" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "black" {}
		_Tex2 ("Base2 (RGB) Trans (A)", 2D) = "black" {}
	}
	SubShader {
		Pass {
			Tags {"Queue"="Background" "IgnoreProjector"="True" "RenderType"="Background"}
			ZWrite Off
			Cull Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _Tex2;
			
			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			struct v2f {
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			float4 _MainTex_ST;
			float4 _Tex2_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.pos = v.vertex;
				#if UNITY_UV_STARTS_AT_TOP
					o.pos.y = -o.pos.y;
				#endif
				o.pos.z = 1;
				o.pos.w = 1;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 col1 = tex2D(_MainTex, i.texcoord);
				half4 col2 = tex2D(_Tex2, i.texcoord1);
				col1.rgb *= col1.a;
				col1.rgb *= (1-col2.a);
				col2.rgb *= col2.a;
				col1.a = 1;
				col2.a = 1;
				return col1 + col2;
			}
			ENDCG
		}
	}
} 