<details>
<summary>UIScan</summary>

```hlsl
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

```hlsl
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


<details>
<summary>简单截屏</summary>

```cs
        // 世界坐标转RectTransform本地坐标 
        // RenderMode = Screen Space - Camera
        public static Vector2 WorldPosToRtLocalPos(Vector3 worldpos3, RectTransform rectTransform)
        {
            Camera camera = Camera.main;
            Vector2 screenpos2 = RectTransformUtility.WorldToScreenPoint(camera, worldpos3);
            Vector2 newvec2;
            bool isCan = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenpos2, camera, out newvec2);
            if (isCan) return newvec2;
            else
            {
                MyLog.Info("[WorldPosToRtLocalPos]定位失败");
                return Vector2.zero;
            }
        }

        // RenderMode = Screen Space - Camera
        public static string Screenshot(Camera camera, RectTransform rectTransform)
        {
            if (camera == null || rectTransform == null)
            {
                MyLog.Error("Camera or rectTransform is null");
                return null;
            }

            var path = Application.persistentDataPath + "/Scrshot";

            if (!MyPlatformUtils.ExistsFileOrDir(path)) MyPlatformUtils.CreateDirectory(path);
            Rect rt = rectTransform.rect;

            RenderTexture rtrender = new RenderTexture(Screen.width, Screen.height, 0);
            Texture2D screenShot = new Texture2D((int)rt.width, (int)rt.height, TextureFormat.RGB24, false);

            camera.targetTexture = rtrender;
            camera.Render();
            RenderTexture.active = rtrender;

            Vector2 scrpos2 = RectTransformUtility.WorldToScreenPoint(camera, rectTransform.position);
            scrpos2.x += rectTransform.pivot.x * rt.width * -1;
            scrpos2.y += rectTransform.pivot.y * rt.height * -1;

            float y = Screen.height - scrpos2.y - rt.height;
            MyLog.Info($"[Screenshot] Pos x:{scrpos2.x},y:{y}"); // 左上角原点

            screenShot.ReadPixels(new Rect(scrpos2.x, y, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            camera.targetTexture = null;
            RenderTexture.active = null;
            GameObject.DestroyImmediate(rtrender);

            byte[] bytes = screenShot.EncodeToJPG();

            String newjpg = path + "/scrshot_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "_temp.jpg";
            MyPlatformUtils.WriteFile(newjpg, bytes);
            MyLog.Info($"[Screenshot] path:{newjpg}");
            return newjpg;
        }
    }
```

</details>


<details>
<summary>贝塞尔曲线</summary>

```cs
        // 贝塞尔曲线2阶  3个定位点
        public static Vector3[] GetBezierCurve2(Vector3 start, Vector3 center, Vector3 end, int count = 21)
        {
            Vector3[] newpos = new Vector3[count];
            var maxidx = count - 1;
            for (int i = 0; i <= maxidx; i++)
            {
                var t = i * 1.0f / maxidx;
                Vector3 aa = start + (center - start) * t;
                Vector3 bb = center + (end - center) * t;
                newpos[i] = aa + (bb - aa) * t;
            }

            return newpos;
        }

        // 贝塞尔曲线3阶 4个定位点
        public static Vector3[] GetBezierCurve3(Vector3 start, Vector3 center, Vector3 center2, Vector3 end, int count = 21)
        {
            Vector3[] newpos = new Vector3[count];
            var maxidx = count - 1;
            for (int i = 0; i <= maxidx; i++)
            {
                var t = i * 1.0f / maxidx;
                Vector3 aa = start + (center - start) * t;
                Vector3 bb = center + (center2 - center) * t;
                Vector3 cc = center2 + (end - center2) * t;

                Vector3 aaa = aa + (bb - aa) * t;
                Vector3 bbb = bb + (cc - bb) * t;
                newpos[i] = aaa + (bbb - aaa) * t;
            }

            return newpos;
        }
```


</details>

<details>
<summary>适应字符串</summary>

```cs
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System;

public class BMTextFit : MonoBehaviour
{
    public enum TextCompProp
    {
        TextLegacy,
        TMP_UI,
        TMP_3D
    }


    [SerializeField] private TextCompProp m_textCompProp;


    
    // 字符换字节宽度限制
    private void FitText1()
    {
        var TComp = GetComponent<Text>();
        var origStr = TComp.text;
        var endIndex = 20;
        
        if (String.IsNullOrEmpty(origStr)) return;
        
        int bytesCount = Encoding.UTF8.GetByteCount(origStr);
        if (bytesCount > endIndex)
        {
            int readyLength = 0;
            float readyWidth = 0;
            for (int i = 0; i < origStr.Length; i++)
            {
                // 中日韩 0x4E00-0x9FFF 字 3字节
                // 标点符号如下范围
                // 0x3000-0x303F 0xFF00-0xFFEF 0xFE10-0xFE1F 0xFE30-0xFE4F 3字节
                // 0x00B7, 0x2014, 0x2026, 0x2018, 0x2019, 0x201C, 0x201D 1字节
                readyLength += Encoding.UTF8.GetByteCount(new [] { origStr[i] });
                if (readyLength == endIndex)
                {
                    origStr = origStr.Substring(0, i + 1) + "...";
                    break;
                }
                else if (readyLength > endIndex)
                {
                    origStr = origStr.Substring(0, i) + "...";
                    break;
                }
            }
        }

        TComp.text = origStr;
    }


    
    // 字符串字节渲染宽度限制
    private void FitText2()
    {
        var TComp = GetComponent<Text>();
        TextGenerator textGenerator = TComp.cachedTextGenerator;
        List<UICharInfo> list_ui_char = new List<UICharInfo>();
        textGenerator.GetCharacters(list_ui_char);
        
        var origStr = TComp.text;
        var maxWidth = 300;
        
        if (String.IsNullOrEmpty(origStr)) return;
        
        float readyWidth = 0;
        for (int i = 0; i < origStr.Length; i++)
        {
            readyWidth += list_ui_char[i].charWidth;
            if (maxWidth == readyWidth)
            {
                origStr = origStr.Substring(0, i + 1) + "...";
                break;
            }
            else if(readyWidth > maxWidth)
            {
                origStr = origStr.Substring(0, i) + "...";
                break;
            }
        }
        
        TComp.text = origStr;
    }
    
    
}




```


</details>

<details>
<summary>屏幕场景偏移</summary>

```cs
    // 3D场景中 单指滑动屏幕屏幕映射到世界平面的位移增量（世界）
    // 屏幕上的两个点 转换摄像机 平面（点，法线）
    private static Plane s_plane;
    private static Vector3 s_plane_delta;

    public static Vector3 GetDeltaForPlane(Vector2 screen_start,Vector2 screen_end,Vector3 pos_world,Vector3 normal,Camera camera = null)
    {
        if(!camera) camera = Camera.main;

        Ray ray_a = camera.ScreenPointToRay(screen_start);
        Ray ray_b = camera.ScreenPointToRay(screen_end);
        s_plane.SetNormalAndPosition(normal, pos_world);

        float a_distance;
        float b_distance;
        bool a_is = s_plane.Raycast(ray_a, out a_distance);
        bool b_is = s_plane.Raycast(ray_b, out b_distance);

        s_plane_delta.Set(0,0,0);
        if (a_is && b_is)
        {
            Vector3 pos_a = ray_a.GetPoint(a_distance);
            Vector3 pos_b = ray_b.GetPoint(b_distance);
            s_plane_delta.x = pos_b.x - pos_a.x;
            s_plane_delta.y = pos_b.y - pos_a.y;
            s_plane_delta.z = pos_b.z - pos_a.z;
        }

        return s_plane_delta;
    }
```


</details>