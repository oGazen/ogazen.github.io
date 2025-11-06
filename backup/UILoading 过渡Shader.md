<details>
	<summary>URP/Loading_Fixed</summary>

```hlsl
Shader "URP/Loading_Fixed"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [Toggle] _Invert("Invert", Float) = 0
        _Radius("Radius", Range(-0.5,1.5)) = 0.1
        _Smooth("Smooth", Range(0,10)) = 0.2
        _OffsetX("Offset Center U",Range(0,1)) = 0.5
        _OffsetY("Offset Center V",Range(0,1)) = 0.5
        _StampTex("Stamp", 2D) = "black" {}
        _StampColor("StampColor", Color) = (1,1,1,1)

        _GradientColorTop("Gradient Color Top", Color) = (1,1,1,1)
        _GradientColorDown("Gradient Color Down", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_StampTex);
            SAMPLER(sampler_StampTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _MainTex_ST;
                half4 _MainTex_TexelSize;

                half4 _StampTex_ST;
                half4 _StampColor;

                half4 _GradientColorTop;
                half4 _GradientColorDown;

                half _OffsetX;
                half _OffsetY;
                half _Radius;
                half _Smooth;
                half _Invert;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;   // 改为 float
                half4  color      : COLOR;
                half2  uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 改为 float
                half4  color      : COLOR;
                half2  uv         : TEXCOORD0;
                half2  stampuv    : TEXCOORD1;
            };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;

                // 等价于 TRANSFORM_TEX(IN.uv, _StampTex)
                OUT.stampuv = IN.uv * _StampTex_ST.xy + _StampTex_ST.zw;

                OUT.color = IN.color;
                return OUT;
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;

                // 动态屏幕分辨率
                half2 screen = GetScaledScreenParams();
                half aspectRatio = screen.y / screen.x;

                
                // 你的原始 alpha/圆环/反转等逻辑（按需打开）
                half aspect = max(aspectRatio, 1e-4);
                uv.y *= aspect; // 以水平宽度为基准 对应屏蔽 比例 X:（0，1） Y:（0，1*aspect）

                half smoothV = _Smooth * 0.2h; // 0.2h 调节变化率
                half alpha = distance(uv, half2(_OffsetX, _OffsetY * aspect)) - _Radius;
                alpha = smoothstep(0.0h, smoothV, alpha);

                half invOn = step(_Invert, 0.1h); // 反转
                alpha = invOn * (1.0h - alpha) + (1.0h - invOn) * alpha;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color * _Color;
                col.a *= saturate(1.0h - alpha);

                // 渐变与 Stamp（按需）
                half3 grad = lerp(_GradientColorTop.rgb, _GradientColorDown.rgb, 1.0h - IN.uv.y);
                col.rgb *= grad;

                half stampA = SAMPLE_TEXTURE2D(_StampTex, sampler_StampTex, IN.stampuv).a;
                col.rgb = lerp(col.rgb, _StampColor.rgb * col.rgb, stampA);

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001h);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
    
}
```

</details>

###### 简介

> 中心圆形过渡动画shader


