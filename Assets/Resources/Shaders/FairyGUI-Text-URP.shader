// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FairyGUI/Text-URP"
{
    Properties
    {
        _MainTex ("Alpha (A)", 2D) = "white" {}

        _ColorMask ("Color Mask", Float) = 15

        _BlendSrcFactor ("Blend SrcFactor", Float) = 5
        _BlendDstFactor ("Blend DstFactor", Float) = 10
    }

    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent+3"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }


        Cull Off
        Lighting Off
        ZWrite Off
        Fog
        {
            Mode Off
        }
        Blend [_BlendSrcFactor] [_BlendDstFactor]
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile NOT_GRAYED GRAYED
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float4 texcoord : TEXCOORD0;
            };

            half3 GammaToLinearSpace(half3 sRGB)
            {
                return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
            }


            sampler2D _MainTex;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(TransformObjectToWorld(v.vertex.xyz));
                o.texcoord = v.texcoord;
                #if !defined(UNITY_COLORSPACE_GAMMA) && (UNITY_VERSION >= 550)
                    o.color.rgb = GammaToLinearSpace(v.color.rgb);
                    o.color.a = v.color.a;
                #else
                o.color = v.color;
                #endif

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = i.color;
                col.a *= half(tex2D(_MainTex, i.texcoord.xy).a);

                #ifdef GRAYED
                    col.rgb = dot(col.rgb, half3(0.299, 0.587, 0.114));  
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}