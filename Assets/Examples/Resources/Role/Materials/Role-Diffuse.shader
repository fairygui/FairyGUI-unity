// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Game/Role/Role-Diffuse" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        _LightDir_0 ("Light0 direction", Vector) = (0.6, -0.8, 0.2, 1.0)
        _LightColor_0 ("Light0 color", Color) = (0.6196,0.5255,0.46275,1)
        _LightIntensity_0 ("Light0 intensity", Range(0,8)) = 0.8
        
        _LightDir_1 ("Light1 direction", Vector) = (-0.9, 0.5, 0.1, 1.0)
        _LightColor_1 ("Light1 color", Color) = (0.2196,0.498,0.61176,1)
        _LightIntensity_1 ("Light1 intensity", Range(0,8)) = 0.6
        
        _RimColor ("Rim color", Color) = (0.4, 0.4, 0.4, 1)
        _RimWidth ("Rim width", Range(0,1)) = 0.7
        
        _Direction ("Direction", Range(-1,1)) = 1
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader {
        Pass {
            Tags {"Queue"="AlphaTest" "IgnoreProjector"="True"}
            AlphaTest Greater [_Cutoff]
            Lighting Off
            Fog {Mode Off}
            Offset 0,-1
            Cull Back
            ZWrite On
            LOD 200

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }
            
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma exclude_renderers flash xbox360 ps3
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _MainTex;
            float4 _LightDir_0;
            float4 _LightColor_0;
            float _LightIntensity_0;
            float4 _LightDir_1;
            float4 _LightColor_1;
            float _LightIntensity_1;
            float4 _RimColor;
            float _RimWidth;
            float _Direction;
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 pos : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            
            float4 _MainTex_ST;
            
            v2f vert(appdata v) {
                v2f o;
                float4x4 modelMatrix = unity_ObjectToWorld;
                o.posWorld = mul(modelMatrix, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalDir = normalize(mul(modelMatrix, float4(v.normal, 0.0)).xyz);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = 1 - dot(v.normal, viewDir);
                
                return o;
            }
            
            half4 frag(v2f i) :COLOR {
                float3 normalDirection = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
                float3 cDir = float3(_Direction,1,1);
                
                float3 lightDirection0 = normalize(-_LightDir_0 * cDir);
                float3 NDotL0 = max(0.0, dot(normalDirection, lightDirection0));
                float4 diffuseReflection0 = float4(_LightIntensity_0 * _LightColor_0.rgb * NDotL0, 1.0);
                
                float3 lightDirection1 = normalize(-_LightDir_1 * cDir);
                float3 NDotL1 = max(0.0, dot(normalDirection, lightDirection1));
                float4 diffuseReflection1 = float4(_LightIntensity_1 * _LightColor_1.rgb * NDotL1, 1.0);
                
                float dotProduct = 1 - dot(normalDirection, viewDirection);
                float4 rim = smoothstep(1 - _RimWidth, 1.0, dotProduct) * _RimColor;
                
                half4 tex = tex2D(_MainTex, i.texcoord);
                half4 c = ((UNITY_LIGHTMODEL_AMBIENT + diffuseReflection0 + diffuseReflection1) * 2+ rim) * tex ;
                c.a = tex.a;
                return c;
            }
            ENDCG
        }
    } 
    FallBack "Diffuse"
}