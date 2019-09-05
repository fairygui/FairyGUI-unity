Shader "FairyGUI/GlowFilter" {  

    Properties{  
        _MainTex("Base (RGB)", 2D) = "white" {}  
        _BlurTex("Blur", 2D) = "white"{}  
        _OutlineColor("OutLineColor", Color) = (1,1,1,1) 
    }  

    CGINCLUDE  
    #include "UnityCG.cginc"  

    //用于剔除中心留下轮廓  
    struct v2f_cull  
    {  
        float4 pos : SV_POSITION;  
        float2 uv : TEXCOORD0;  
    };  

    //用于模糊  
    struct v2f_blur  
    {  
        float4 pos : SV_POSITION;  
        float2 uv  : TEXCOORD0;  
        float4 uv01 : TEXCOORD1;  
        float4 uv23 : TEXCOORD2;  
        float4 uv45 : TEXCOORD3;  
    };  

    //用于最后叠加  
    struct v2f_add  
    {  
        float4 pos : SV_POSITION;  
        float2 uv  : TEXCOORD0;  
        float2 uv1 : TEXCOORD1;  
    };  

    sampler2D _MainTex;  
    float4 _MainTex_TexelSize;  
    sampler2D _BlurTex;  
    float4 _BlurTex_TexelSize;  
    float4 _offsets;  
    float _OutlineStrength;  
    fixed4 _OutlineColor;

    //获得轮廓 pass 1
    v2f_cull vert_cull(appdata_img v)  
    {  
        v2f_cull o;  
        o.pos = UnityObjectToClipPos(v.vertex);  
        o.uv = v.texcoord.xy;  
        //dx中纹理从左上角为初始坐标，需要反向  
#if UNITY_UV_STARTS_AT_TOP  
        if (_MainTex_TexelSize.y < 0)  
            o.uv.y = 1 - o.uv.y;  
#endif    
        return o;  
    }  

    fixed4 frag_cull(v2f_cull i) : SV_Target  
    {  
        fixed4 colorMain = tex2D(_MainTex, i.uv);  
        fixed4 colorBlur = tex2D(_BlurTex, i.uv);  
        clip(colorMain.a - 0.001);
        return colorBlur;  
    }  

    //高斯模糊 pass 0
    v2f_blur vert_blur(appdata_img v)  
    {  
        v2f_blur o;  
        _offsets *= _MainTex_TexelSize.xyxy;  
        o.pos = UnityObjectToClipPos(v.vertex);  
        o.uv = v.texcoord.xy;  

        o.uv01 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1);  
        o.uv23 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 2.0;  
        o.uv45 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 3.0;  

        return o;  
    }  
    fixed4 frag_blur(v2f_blur i) : SV_Target  
    {  
        fixed4 color = fixed4(0,0,0,0);  
        color += 0.40 * tex2D(_MainTex, i.uv);  
        color += 0.15 * tex2D(_MainTex, i.uv01.xy);  
        color += 0.15 * tex2D(_MainTex, i.uv01.zw);  
        color += 0.10 * tex2D(_MainTex, i.uv23.xy);  
        color += 0.10 * tex2D(_MainTex, i.uv23.zw);  
        color += 0.05 * tex2D(_MainTex, i.uv45.xy);  
        color += 0.05 * tex2D(_MainTex, i.uv45.zw);  
        return color;  
    }  



    //最终叠加 pass 2
    v2f_add vert_final(appdata_img v)  
    {  
        v2f_add o;  
        o.pos = UnityObjectToClipPos(v.vertex);  
        o.uv.xy = v.texcoord.xy;  
        o.uv1.xy = o.uv.xy;  
#if UNITY_UV_STARTS_AT_TOP  
        if (_MainTex_TexelSize.y < 0)  
            o.uv.y = 1 - o.uv.y;  
#endif    
        return o;  
    }  

    fixed4 frag_final(v2f_add i) : SV_Target  
    {  
        fixed4 ori = tex2D(_MainTex, i.uv1);  
        fixed4 blur = tex2D(_BlurTex, i.uv);  
        blur.rgb = _OutlineColor;
        blur.a = blur.a * _OutlineStrength;
        fixed4 final = ori * ori.a + blur * (1- ori.a);  
        return final;  
    }  
    
    
    
    
    v2f_cull vert_pure (appdata_img v)
    {
        v2f_cull o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv.xy = v.texcoord.xy;  
        return o;
    }
    fixed4 frag_pure (v2f_cull i) : SV_Target
    {
        half4 color = tex2D(_MainTex, i.uv);
        return fixed4(1,1,1,color.a);
        
    }
    
    
    ENDCG  

    SubShader  
    {  
        //pass 0: 高斯模糊  
        Pass  
        {  
            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  

            CGPROGRAM  
            #pragma vertex vert_blur  
            #pragma fragment frag_blur  
            ENDCG  
        }  

        //pass 1: 剔除中心部分   
        Pass  
        {  
            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  

            CGPROGRAM  
            #pragma vertex vert_cull
            #pragma fragment frag_cull
            ENDCG  
        }  


        //pass 2: 最终叠加  
        Pass  
        {  

            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  

            CGPROGRAM  
            #pragma vertex vert_final
            #pragma fragment frag_final  
            ENDCG  
        }  
        
        
        Pass {
             ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  

            CGPROGRAM  
            #pragma vertex vert_pure
            #pragma fragment frag_pure  
            ENDCG  
        }

    }  
}