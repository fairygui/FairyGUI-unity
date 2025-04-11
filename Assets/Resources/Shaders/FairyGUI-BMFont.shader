// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FairyGUI/BMFont"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        _BlendSrcFactor ("Blend SrcFactor", Float) = 5
        _BlendDstFactor ("Blend DstFactor", Float) = 10
    }

    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend [_BlendSrcFactor] [_BlendDstFactor]
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
                #pragma multi_compile _ GRAYED
                #pragma multi_compile _ CLIPPED SOFT_CLIPPED
                #pragma vertex vert
                #pragma fragment frag
                #pragma exclude_renderers d3d9 opengl flash

                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float4 texcoord : TEXCOORD0;

                    #ifdef CLIPPED
                    float2 clipPos : TEXCOORD1;
                    #endif

                    #ifdef SOFT_CLIPPED
                    float2 clipPos : TEXCOORD1;
                    #endif
                };

                sampler2D _MainTex;

                CBUFFER_START(UnityPerMaterial)
                #ifdef CLIPPED
                float4 _ClipBox = float4(-2, -2, 0, 0);
                #endif

                #ifdef SOFT_CLIPPED
                float4 _ClipBox = float4(-2, -2, 0, 0);
                float4 _ClipSoftness = float4(0, 0, 0, 0);
                #endif
                CBUFFER_END

                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    #if !defined(UNITY_COLORSPACE_GAMMA) && (UNITY_VERSION >= 550)
                    o.color.rgb = GammaToLinearSpace(v.color.rgb);
                    o.color.a = v.color.a;
                    #else
                    o.color = v.color;
                    #endif

                    #ifdef CLIPPED
                    o.clipPos = mul(unity_ObjectToWorld, v.vertex).xy * _ClipBox.zw + _ClipBox.xy;
                    #endif

                    #ifdef SOFT_CLIPPED
                    o.clipPos = mul(unity_ObjectToWorld, v.vertex).xy * _ClipBox.zw + _ClipBox.xy;
                    #endif

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = i.color;
                    fixed4 tcol = tex2D(_MainTex, i.texcoord);
                    col.a *= tcol[i.texcoord.z];//z stores channel

                    #ifdef GRAYED
                    fixed grey = dot(col.rgb, fixed3(0.299, 0.587, 0.114));
                    col.rgb = fixed3(grey, grey, grey);
                    #endif

                    #ifdef SOFT_CLIPPED
		            float2 factor;
		            float2 condition = step(i.clipPos.xy, 0);
		            float4 clip_softness = _ClipSoftness * float4(condition, 1 - condition);
		            factor.xy = (1.0 - abs(i.clipPos.xy)) * (clip_softness.xw + clip_softness.zy);
                    col.a *= clamp(min(factor.x, factor.y), 0.0, 1.0);
                    #endif
                    #ifdef CLIPPED
                    float2 factor = abs(i.clipPos);
                    col.a *= step(max(factor.x, factor.y), 1);
                    #endif

                    return col;
            }
            ENDCG
        }
    }

    //Fallback "FairyGUI/Text"
}
