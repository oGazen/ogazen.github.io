<details>
<summary>UIScan</summary>

```
Shader "URP/UIScan"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _Color ("Color",Color) = (1,1,1,1)
        [Toggle] _Reverse("Reverse",Integer) = 0
        
        
        [Header(SCAN)]
        _ScanIntensity("Scan Intensity",Range(0,2)) = 1
        _ScanWidth("Scan Width",Range(0,0.5)) = 0.1
        _ScanAngle("Scan Angle",Range(-89,89)) = 0 // 水平为0
        _Feathering("Feathering",Range(0,1)) = 0
        
        [Header(TIME)]
        _TimeDuration("Time Duration",Range(1,5)) = 1
        _TimeInterval("Time Interval",Range(0,5)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="true"
            "Queue"="Transparent" // Transparent AlphaTest Geometry Background
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag



            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;


            half4 _Color;
            
            half _ScanIntensity;
            half _ScanWidth;
            half _ScanAngle;
            half _Feathering;

            half _TimeDuration;
            half _TimeInterval;
            int _Reverse;
            CBUFFER_END

            
            struct Attributes
            {
                half4 position : POSITION;
                half2 uv : TEXCOORD0;
                half4 color : COLOR;
            };


            struct Varyings
            {
                half4 position_sv : SV_POSITION;
                half2 uv : TEXCOORD0;
                half4 color : COLOR;
            };


            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.position_sv = TransformObjectToHClip(IN.position);
                OUT.uv = TRANSFORM_TEX(IN.uv,_MainTex);
                OUT.color = IN.color;
                return OUT;
            }



            half4 Frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D_X(_MainTex,sampler_MainTex,IN.uv) * IN.color;

                // 时间的变化周期，时间间隔
                half timey = _Time.y;
                half timey_normal = timey % (_TimeDuration + _TimeInterval) / _TimeDuration;
                timey_normal = saturate(timey_normal);


                // 扫描效果
                _ScanAngle = _Reverse==0 ? _ScanAngle : _ScanAngle * -1;
                half k = tan(radians(_ScanAngle));
                half cos_v = cos(radians(_ScanAngle));
                half halfWidth = _ScanWidth / 2 / cos_v;

                half b,y;
                if(k > 0)
                {
                    b = lerp(-halfWidth-k,1+halfWidth,timey_normal);
                    y = k*IN.uv.x + b;
                }
                else
                {
                    b = lerp(-halfWidth,1+halfWidth+abs(k),timey_normal);
                    y = k*IN.uv.x + b;
                }

                
                half y_min = y - halfWidth;
                half y_max = y + halfWidth;
                half uv_y = _Reverse==0 ? IN.uv.y : 1-IN.uv.y;
                if (uv_y >= y_min && uv_y <= y_max)
                {
                    // 羽化效果
                    half4 colAdd = _Color * _ScanIntensity;
                    half feather = smoothstep(halfWidth,0,abs(uv_y - y));
                    colAdd *= feather;
                    col += colAdd; 
                }
                

                
                return col;
            }
            

            
            ENDHLSL
        }
    }
}
```

</details>

<details>
<summary>UIGray</summary>

```
Shader "URP/UIGray"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="true"
            "Queue"="Transparent" // Transparent AlphaTest Geometry Background
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag



            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            CBUFFER_END

            
            struct Attributes
            {
                half4 position : POSITION;
                half2 uv : TEXCOORD0;
            };


            struct Varyings
            {
                half4 position_sv : SV_POSITION;
                half2 uv : TEXCOORD0;
            };


            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.position_sv = TransformObjectToHClip(IN.position);
                OUT.uv = TRANSFORM_TEX(IN.uv,_MainTex);
                return OUT;
            }



            half4 Frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D_X(_MainTex,sampler_MainTex,IN.uv);
                const half3 gray_multiply = half3(0.222h, 0.707h, 0.071h);
                col.rgb = dot(col.rgb,gray_multiply);
                return col;
            }
            

            
            ENDHLSL
        }
    }
}

```

</details>
