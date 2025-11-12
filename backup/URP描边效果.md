##### 法线外扩/视口夹角

<details>
    <summary>Outline3DOutExternal</summary>

```hlsl
Shader "URP/Outline3DOutExternal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Color",Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width",Range(0,0.1)) = 0.1
        _OutlinePowerIn ("Outline PowerIn",Range(0.01,10)) = 1
    }
    SubShader
    {
  


        Pass
        {
            Name "Outline3D_Inside"
  
            Tags
            {
                "RenderType"="Opaque"
                "Queue"="Transparent"
                "RenderPipeline"="UniversalPipeline"
                "LightMode"="UniversalForward"
            }
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert;
            #pragma fragment frag;
  
            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_linear_clamp_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlinePowerIn;
            CBUFFER_END
  
            struct Varyings
            {
                half4 position : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 normal_world : TEXCOORD1;
                half3 position_world : TEXCOORD2;
            };


            Varyings vert(half2 uv : TEXCOORD0,half4 position : POSITION,half4 normal : NORMAL)
            {
                Varyings OUT;
                OUT.position_world = mul(GetObjectToWorldMatrix(),position);
                OUT.normal_world = TransformObjectToWorldNormal(normal);
                OUT.position = TransformObjectToHClip(position);
                OUT.uv = uv;
                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                half3 direction_view = normalize(GetCameraPositionWS() - (IN.position_world));
                half v = dot(direction_view,IN.normal_world);
                v = 1 - saturate(v);
                v = pow(v,_OutlinePowerIn);
  
                half4 col = SAMPLE_TEXTURE2D_X(_MainTex,sampler_linear_clamp_MainTex,IN.uv);
                return lerp(col,_OutlineColor,v);
            }
  
            ENDHLSL
        }
  

        Pass
        {
            Name "Outline3D_Swell"
  
            Tags
            {
                "RenderType"="Opaque"
                "Queue"="Transparent"
                "RenderPipeline"="UniversalPipeline"
                "LightMode"="SRPDefaultUnlit"
            }
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front

            HLSLPROGRAM
  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert;
            #pragma fragment frag;


            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_linear_clamp_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END
  
            struct Varyings
            {
                half4 position : SV_POSITION;
                half2 uv : TEXCOORD0;
            };


            Varyings vert(half2 uv : TEXCOORD0,half4 position : POSITION,half4 normal : NORMAL)
            {
                Varyings OUT;
                position.xyz += normal * _OutlineWidth;
                OUT.position = TransformObjectToHClip(position);
                OUT.uv = uv;
                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
  
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
```

</details>

##### UI描边(配合C#)

<details>
	<summary>OutlineUI</summary>

```hlsl
Shader "URP/OutlineUI"
{
    Properties
    {
        [PerRendererData]_MainTex ("Main Texture", 2D) = "white" { }
        [HideInInspector]_Color ("Tint", Color) = (1, 1, 1, 1)

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector]_ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _AlphaClipThreshold ("Alpha Clip Threshold",Range(0,1)) = 0.001
        // 注意：Softness属性这里定义只是为了Material Inspector显示，
        // 实际值将由RectMask2D组件通过MaterialPropertyBlock动态覆盖
        _Softness("Softness", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }
  

        Pass
        {
            Name "UIOutlineEx"
  
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
            ZTest [unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _ClipRect;


            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _MainTex_TexelSize;
            half4 _Softness;
            half _AlphaClipThreshold;
            CBUFFER_END

            half4 _TextureSampleAdd;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                half4 color : COLOR;
            };


            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 texcoord : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                half4 color : COLOR;

                half4 objPosition : TEXCOORD3;
            };

            v2f vert(appdata IN)
            {
                v2f o;
  
                o.vertex = TransformObjectToHClip(IN.vertex.xyz);
                o.texcoord = IN.texcoord;
                o.color = IN.color * _Color;
                o.uv1 = IN.uv1;
                o.uv2 = IN.uv2;

                o.objPosition = IN.vertex;
                return o;
            }

            half IsInRect(float2 pPos, float2 pClipRectMin, float2 pClipRectMax)
            {
                pPos = step(pClipRectMin, pPos) * step(pPos, pClipRectMax);
                return pPos.x * pPos.y;
            }

            half SampleAlpha(int pIndex, v2f IN)
            {
                const half sinArray[12] =
                {
                    0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5
                };
                const half cosArray[12] =
                {
                    1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866
                };
                float2 pos = IN.texcoord.xy + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * IN.texcoord.z;
                half pos_uv_col_a = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pos) + _TextureSampleAdd).w;
                return IsInRect(pos, IN.uv1.xy, IN.uv1.zw) * pos_uv_col_a * IN.uv2.w;
            }
  
            half4 frag(v2f IN) : SV_Target
            {
                half4 color = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord.xy) + _TextureSampleAdd) * IN.color;

  
                if (all(IN.texcoord.z))
                {
                    half sign_a = IsInRect(IN.texcoord.xy, IN.uv1.xy, IN.uv1.zw);
                    color.a *= sign_a;
                    half4 val = half4(IN.uv2.rgb, 0);

                    val.w += SampleAlpha(0, IN);
                    val.w += SampleAlpha(1, IN);
                    val.w += SampleAlpha(2, IN);
                    val.w += SampleAlpha(3, IN);
                    val.w += SampleAlpha(4, IN);
                    val.w += SampleAlpha(5, IN);
                    val.w += SampleAlpha(6, IN);
                    val.w += SampleAlpha(7, IN);
                    val.w += SampleAlpha(8, IN);
                    val.w += SampleAlpha(9, IN);
                    val.w += SampleAlpha(10, IN);
                    val.w += SampleAlpha(11, IN);
                    val.w = saturate(val.w);

                    val.w *= IN.texcoord.w;
                    color = lerp(val, color, color.a) * val.w + color * (1 - val.w);
                }


               // 2. --- 核心：应用自定义的RectMask2D裁剪（带软边缘）---
                #if UNITY_UI_CLIP_RECT
                // 2.1 计算片元到裁剪矩形四条边的距离
                half2 minDist = (IN.objPosition.xy - _ClipRect.xy);
                half2 maxDist = (_ClipRect.zw - IN.objPosition.xy);
                half2 edgeDistance = min(minDist, maxDist); // 对于矩形内的点，这个值是正的

                // 2.2 计算软边缘因子
                // saturate(x / softness) ：
                // - 当 x > softness：结果 >= 1，完全保留
                // - 当 0 < x < softness：结果在 0 到 1 之间，平滑过渡
                // - 当 x < 0：结果为 0，完全裁剪
                // 使用max(softness, 0.001)避免除以零
                half2 softnessFactor = saturate(edgeDistance / max(_Softness.xy, 0.001));

                // 2.3 综合两个方向的因子（取最小值，这样任何一个方向出界都会导致裁剪）
                half finalAlphaFactor = softnessFactor.x * softnessFactor.y;

                // 2.4 将裁剪因子应用到最终颜色的Alpha通道
                color.a *= finalAlphaFactor;
                #endif

  
                #if UNITY_UI_ALPHACLIP
                    clip(color.a - _AlphaClipThreshold);
                #endif


                return color;
            }

  
            ENDHLSL
        }
    }
}
```

</details>

<details>
	<summary>UIOutlineEx</summary>

```cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UGUI描边
/// </summary>
public class UIOutlineEx : BaseMeshEffect
{
    private static List<UIVertex> m_VetexList = new List<UIVertex>();

    // 公开变量
    public Color OutlineColor = Color.black;
    [Range(0.01f, 6)] public float OutlineWidth = 1;
    [SerializeField] private Material m_outlineMaterial;
    [SerializeField] private bool m_DrawOutline;

    // 私有变量
    private float m_uivertex_alpha_max = 0;
    private CanvasGroup[] m_canvasGroups;
    private float m_canvasGroups_alphamul = 1;


    public void SetDrawOutline(bool draw)
    {
        m_DrawOutline = draw;
    }
  
  
  
    protected override void Awake()
    {
        base.Awake();

        if (base.graphic)
        {
            if (base.graphic.canvas)
            {
                var v1 = base.graphic.canvas.additionalShaderChannels;
                var v2 = AdditionalCanvasShaderChannels.TexCoord1;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }
                v2 = AdditionalCanvasShaderChannels.TexCoord2;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }
            }
            this._Refresh();
        }
    }

    protected override void OnDestroy()
    {
        base.graphic.material = null;
    }


    protected override void OnEnable()
    {
        base.graphic.material = m_outlineMaterial;
        this._Refresh();
    }

    protected override void OnDisable()
    {
        base.graphic.material = null;
        this._Refresh();
    }

    protected override void Start()
    {
        base.graphic.material = m_outlineMaterial;
    }


    protected override void OnCanvasGroupChanged()
    {
        base.OnCanvasGroupChanged();
        if (m_canvasGroups == null) m_canvasGroups = GetComponentsInParent<CanvasGroup>();

        m_canvasGroups_alphamul = 1;
        for (int i = 0; i < m_canvasGroups.Length; i++)
        {
            m_canvasGroups_alphamul *= m_canvasGroups[i].alpha;
        }
        this._Refresh();
    }
  

    private void _Refresh()
    {
        base.graphic.SetVerticesDirty();
    }


    public override void ModifyMesh(VertexHelper vh)
    {
        if(!m_DrawOutline) return;
  
        vh.GetUIVertexStream(m_VetexList);

        m_uivertex_alpha_max = 0;
        for (int i = 0; i < m_VetexList.Count; i++)
        {
            var v = m_VetexList[i];
            if (v.color.a / 255f > m_uivertex_alpha_max) m_uivertex_alpha_max = v.color.a / 255f;
        }
        m_uivertex_alpha_max *= m_canvasGroups_alphamul;

        this._ProcessVertices();

        vh.Clear();
        vh.AddUIVertexTriangleStream(m_VetexList);
    }


    private void _ProcessVertices()
    {
        for (int i = 0, count = m_VetexList.Count - 3; i <= count; i += 3)
        {
            var v1 = m_VetexList[i];
            var v2 = m_VetexList[i + 1];
            var v3 = m_VetexList[i + 2];
  
  
  
            // 计算原顶点坐标中心点
            //
            var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
  
  
  
            // 计算原始顶点坐标和UV的方向
            //
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
            {
                triX = pos2 - pos1;
                triY = pos3 - pos2;
                uvX = v2.uv0 - v1.uv0;
                uvY = v3.uv0 - v2.uv0;
            }
            else
            {
                triX = pos3 - pos2;
                triY = pos2 - pos1;
                uvX = v3.uv0 - v2.uv0;
                uvY = v2.uv0 - v1.uv0;
            }
  
  
  
            // 计算原始UV框
            var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);

            // 为每个顶点设置新的Position和UV，并传入原始UV框
            v1 = _SetNewPosAndUV(v1, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);
            v2 = _SetNewPosAndUV(v2, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);
            v3 = _SetNewPosAndUV(v3, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);


            // 应用设置后的UIVertex
            //
            m_VetexList[i] = v1;
            m_VetexList[i + 1] = v2;
            m_VetexList[i + 2] = v3;
        }
    }


    private UIVertex _SetNewPosAndUV(
        UIVertex pVertex, float pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVX, Vector2 pUVY,
        Vector2 pUVOriginMin, Vector2 pUVOriginMax)
    {
        // Position
        var pos = pVertex.position;
        var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
        var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
        pos.x += posXOffset;
        pos.y += posYOffset;
        pVertex.position = pos;
  
        // UV
        var uv = (Vector2)pVertex.uv0;
        var uv_additional_x = pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
        var uv_additional_y = pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
        uv += uv_additional_x;
        uv += uv_additional_y;
        pVertex.uv0 = uv;
  
  
        pVertex.uv0.z = pOutLineWidth; // z：定位为宽度
        pVertex.uv0.w = m_uivertex_alpha_max; // w：所有顶点中最大的透明度值

  
        // uv1 uv2 可用
        pVertex.uv1 = pUVOriginMin;     // uv1：定位为最小最大值数据
        pVertex.uv1.z = pUVOriginMax.x;
        pVertex.uv1.w = pUVOriginMax.y;

        pVertex.uv2 = this.OutlineColor; // uv2：定位为描边颜色

        return pVertex;
    }


    private static float _Min(float pA, float pB, float pC)
    {
        return Mathf.Min(Mathf.Min(pA, pB), pC);
    }


    private static float _Max(float pA, float pB, float pC)
    {
        return Mathf.Max(Mathf.Max(pA, pB), pC);
    }


    private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
    }


    private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
    }
}
```

</details>

##### 后处理Feature

<details>
	<summary>URP_Feature_Outline3D</summary>

```cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URP_Feature_Outline3D : ScriptableRendererFeature
{
    class Outline3DRenderPass : ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> s_shaderTagIds = new List<ShaderTagId>()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit")
        };
        private static readonly int s_shaderProperty_outline3d = Shader.PropertyToID("_Outline3D");



        private readonly Material m_outline_material;
        private readonly FilteringSettings m_filteringSettings;
        private readonly MaterialPropertyBlock m_materialPropertyBlock;
        private RTHandle m_outline_rtHandle;


        public Outline3DRenderPass(Material material)
        {
            // Configures where the render pass should be injected.
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        
            m_outline_material = material;
            m_filteringSettings = new FilteringSettings(RenderQueueRange.all, renderingLayerMask: 0b10);
            m_materialPropertyBlock = new MaterialPropertyBlock();
        }


        public void Dispose()
        {
            m_outline_rtHandle?.Release();
            m_outline_rtHandle = null;
        
        }





        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

            var desc_camera = renderingData.cameraData.cameraTargetDescriptor;
            desc_camera.msaaSamples = 1;
            desc_camera.depthBufferBits = 0;
            desc_camera.colorFormat = RenderTextureFormat.ARGB32;
            RenderingUtils.ReAllocateIfNeeded(ref m_outline_rtHandle, desc_camera, name: "_Outline3D");
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("Outline3D CMD");

            // CMD
            cmd.SetRenderTarget(m_outline_rtHandle);
            cmd.ClearRenderTarget(true, true, Color.clear);


            var draw_settings = CreateDrawingSettings(s_shaderTagIds, ref renderingData, SortingCriteria.None);
            RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, draw_settings, m_filteringSettings);
            var list_draw = context.CreateRendererList(ref rendererListParams);
            cmd.DrawRendererList(list_draw);
        
        
            // cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            // m_materialPropertyBlock.SetTexture(s_shaderProperty_outline3d, m_outline_rtHandle);
            // cmd.DrawProcedural(Matrix4x4.identity, m_outline_material, 0, MeshTopology.Triangles, 3, 1, m_materialPropertyBlock);
            m_outline_material.SetTexture(s_shaderProperty_outline3d, m_outline_rtHandle);
            Blitter.BlitCameraTexture(cmd, m_outline_rtHandle,renderingData.cameraData.renderer.cameraColorTargetHandle,m_outline_material,0);



            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        //public override void OnCameraCleanup(CommandBuffer cmd)
        //{
        //}
    }






    [SerializeField] private Material m_OutlineMaterial;
    private Outline3DRenderPass m_Outline3DScriptablePass;

    private bool IsMaterialValid => m_OutlineMaterial && m_OutlineMaterial.shader && m_OutlineMaterial.shader.isSupported;


    /// <inheritdoc/>
    public override void Create()
    {
        if (!IsMaterialValid)
        {
            return;
        }

        m_Outline3DScriptablePass = new Outline3DRenderPass(m_OutlineMaterial);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Outline3DScriptablePass == null)
        {
            return;
        }


        renderer.EnqueuePass(m_Outline3DScriptablePass);
    }


    protected override void Dispose(bool disposing)
    {
        m_Outline3DScriptablePass?.Dispose();
    }

}
```

</details>

<details>
	<summary>Outline3D</summary>

```hlsl
Shader "URP/Outline3D"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.01)) = 0.001
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100
        
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


            struct Attributes
            {
                uint vertex_id : SV_VERTEXID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half2 uv : TEXCOORD;
                half2 offsets[8] : TEXCOORD1;
            };


            TEXTURE2D_X(_Outline3D);
            SAMPLER(sampler_linear_clamp_Outline3D);

            CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor;
            float _OutlineWidth;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings Output;
                Output.positionCS = GetFullScreenTriangleVertexPosition(IN.vertex_id);
                Output.uv = GetFullScreenTriangleTexCoord(IN.vertex_id);

                const half ratio_w_h = _ScreenParams.x / _ScreenParams.y;
                const float multipty_num_2 = 0.707; // sprt(2) / 2

                Output.offsets[0] = half2(-1, ratio_w_h) * _OutlineWidth * multipty_num_2;
                Output.offsets[1] = half2(0, ratio_w_h) * _OutlineWidth;
                Output.offsets[2] = half2(1, ratio_w_h) * _OutlineWidth * multipty_num_2;
                Output.offsets[3] = half2(-1, 0) * _OutlineWidth;
                
                Output.offsets[4] = half2(1, 0) * _OutlineWidth;
                Output.offsets[5] = half2(-1, -ratio_w_h) * _OutlineWidth * multipty_num_2;
                Output.offsets[6] = half2(0, -ratio_w_h) * _OutlineWidth;
                Output.offsets[7] = half2(1, -ratio_w_h) * _OutlineWidth * multipty_num_2;

                return Output;
            }



            half4 frag(Varyings IN) : SV_Target
            {
                const half kernelX[8] =
                {
                    - 1, 0, 1,
                    - 2, 2,
                    - 1, 0, 1
                };

                const half kernelY[8] =
                {
                    - 1, -2, -1,
                    0, 0,
                    1, 2, 1
                };


                half gx = 0;
                half gy = 0;
                half multiply_num = 0;

                for (int i = 0; i < 8; i++)
                {
                    multiply_num = SAMPLE_TEXTURE2D_X(_Outline3D, sampler_linear_clamp_Outline3D, IN.uv + IN.offsets[i]).a;
                    gx += multiply_num * kernelX[i];
                    gy += multiply_num * kernelY[i];
                }

                const half _a = SAMPLE_TEXTURE2D_X(_Outline3D, sampler_linear_clamp_Outline3D, IN.uv).a;
                half4 col = _OutlineColor;
                col.a = saturate(abs(gx) + abs(gy)) * (1 - _a);

                
                return col;
            }

            ENDHLSL
        }
    }
}
```

</details>

##### 后处理Feature2

<details>
	<summary>URP_Feature_Outline3D_2</summary>

```cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class URP_Feature_Outline3D_2 : ScriptableRendererFeature
{
    private class Outline3DRenderPass : ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> s_shaderTagIds = new List<ShaderTagId>()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit")
        };
        
        
        private readonly Material m_outline_material;
        private readonly Color m_outlineColor = Color.yellow;
        private readonly float m_downSampleScale = 0.5f; 
        private readonly int m_blurIterations = 1; 
        private readonly float m_blurWidth = 1.0f;
        
        
        
        private readonly FilteringSettings m_filteringSettings;
        private readonly MaterialPropertyBlock m_materialPropertyBlock;
        
        private RTHandle m_outline_rtHandle;
        private RTHandle m_temp_downsample_rtHandle;
        private RTHandle m_temp_blurRt;

        public Outline3DRenderPass(Material material,Color outlineColor,float downSampleScale,int blurIterations,float blurWidth)
        {
            // Configures where the render pass should be injected.
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            m_outline_material = material;
            m_outlineColor = outlineColor;
            m_downSampleScale = downSampleScale;
            m_blurIterations = blurIterations;
            m_blurWidth = blurWidth;
            
            m_filteringSettings = new FilteringSettings(RenderQueueRange.all, renderingLayerMask: 0b10);
            m_materialPropertyBlock = new MaterialPropertyBlock();
        }


        public void Dispose()
        {
            m_outline_rtHandle?.Release();
            m_outline_rtHandle = null;
            m_temp_blurRt?.Release();
            m_temp_blurRt = null;
            m_temp_downsample_rtHandle?.Release();
            m_temp_downsample_rtHandle = null;
        }




        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

            var desc_camera = renderingData.cameraData.cameraTargetDescriptor;
            desc_camera.msaaSamples = 1;
            desc_camera.depthBufferBits = 0;
            desc_camera.colorFormat = RenderTextureFormat.ARGB32;
            RenderingUtils.ReAllocateIfNeeded(ref m_outline_rtHandle, desc_camera, name: "_Outline3D");
            
            Vector2 scaleFactor = new Vector2(m_downSampleScale, m_downSampleScale);
            RenderingUtils.ReAllocateIfNeeded(ref m_temp_downsample_rtHandle,scaleFactor,desc_camera ,name: "_TempDownsample",filterMode:FilterMode.Bilinear);
            RenderingUtils.ReAllocateIfNeeded(ref m_temp_blurRt,scaleFactor,desc_camera ,name: "_TempBlur",filterMode:FilterMode.Bilinear);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.camera.cameraType != CameraType.Game)
                return;

            if (m_outline_material == null)
                return;
            
            
            
            
            var cmd = CommandBufferPool.Get("Outline3D CMD");

            // CMD
            cmd.SetRenderTarget(m_temp_downsample_rtHandle);
            cmd.ClearRenderTarget(true, true, Color.clear);
            
            cmd.SetRenderTarget(m_temp_blurRt);
            cmd.ClearRenderTarget(true, true, Color.clear);
            
            cmd.SetRenderTarget(m_outline_rtHandle);
            cmd.ClearRenderTarget(true, true, Color.clear);


            var draw_settings = CreateDrawingSettings(s_shaderTagIds, ref renderingData, SortingCriteria.None);
            RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, draw_settings, m_filteringSettings);
            var list_draw = context.CreateRendererList(ref rendererListParams);
            cmd.DrawRendererList(list_draw);

            
            
            
            // Pass0
            m_outline_material.SetColor("_OutlineColor", m_outlineColor);
            Blitter.BlitCameraTexture(cmd, m_outline_rtHandle,m_outline_rtHandle,m_outline_material,0);
            m_outline_material.SetTexture("_OutlineColorTex", m_outline_rtHandle);

            
            
            // Pass1
            m_outline_material.SetFloat("_BlurWidth", m_blurWidth);
            for (int i = 0; i < m_blurIterations; ++i)
            {
                if (i == 0)
                {
                    Blitter.BlitCameraTexture(cmd, m_outline_rtHandle,m_temp_downsample_rtHandle,m_outline_material,1);
                    Blitter.BlitCameraTexture(cmd, m_temp_downsample_rtHandle,m_temp_blurRt,m_outline_material,1);
                }
                else
                {
                    Blitter.BlitCameraTexture(cmd, m_temp_blurRt,m_temp_downsample_rtHandle,m_outline_material,1);
                    Blitter.BlitCameraTexture(cmd, m_temp_downsample_rtHandle,m_temp_blurRt,m_outline_material,1);
                }
            }
            
            

            // Pass2
            m_outline_material.SetTexture("_BlurTex", m_temp_blurRt);
            
            
            RTHandle targetRT = cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, targetRT,targetRT,m_outline_material,2);
            
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            
        }
    }






    [SerializeField] private Material m_OutlineMaterial;
    public Color m_outlineColor = Color.yellow;

    [Range(0.1f, 1)]
    public float m_downSampleScale = 0.5f;                 // 降采样比例
    [Range(0, 4)]
    public int m_blurIterations = 1;                  // 均值糊迭代次数
    [FormerlySerializedAs("m_blurRadius")] [Range (0.2f, 10.0f)]
    public float m_blurWidth = 1.0f;
    
    private bool IsMaterialValid => m_OutlineMaterial && m_OutlineMaterial.shader && m_OutlineMaterial.shader.isSupported;

    
    
    
    private Outline3DRenderPass m_Outline3DScriptablePass;
    

    /// <inheritdoc/>
    public override void Create()
    {
        if (!IsMaterialValid)
        {
            return;
        }

        m_Outline3DScriptablePass = new Outline3DRenderPass(m_OutlineMaterial,m_outlineColor,m_downSampleScale,m_blurIterations,m_blurWidth);
    }
    
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Outline3DScriptablePass == null)
        {
            return;
        }

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(m_Outline3DScriptablePass);
        }
    }


    protected override void Dispose(bool disposing)
    {
        m_Outline3DScriptablePass?.Dispose();
    }

}
```

</details>

<details>
	<summary>PostEffectOutline3D</summary>

```hlsl
Shader "URP/PostEffectOutline3D"
{
    Properties
    {
    	[PerRendererData]_OutlineColor ("Outline Color",Color) = (1,1,1,1)
    	[PerRendererData]_BlurRadius("Blur Width",Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="true"
        	"Queue" = "Geometry"
        }
        LOD 100
		Cull Off 
		ZWrite Off
	    Blend SrcAlpha OneMinusSrcAlpha
        
        HLSLINCLUDE
        
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
        TEXTURE2D_X(_CameraOpaqueTexture);
        TEXTURE2D_X(_OutlineColorTex);
        TEXTURE2D_X(_BlurTex);
        SAMPLER(sampler_linear_clamp_CameraOpaqueTexture);

        half4 _BlitTexture_TexelSize;
        CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor;
		    float _BlurWidth;
        CBUFFER_END


        struct appdata_img
        {
	        half4 vertex : POSITION;
        	half2 texcoord : TEXCOORD0;
        };

        
        struct v2f
		{
			float4 pos : SV_POSITION;
			half2 uv_ : TEXCOORD0;
			half4 uv1 : TEXCOORD3;
			half4 uv2 : TEXCOORD4;
			half4 uv3 : TEXCOORD5;
			half4 uv4 : TEXCOORD6;
		};
        
        

        // 均值模糊
        // ---------------------------【顶点着色器】---------------------------
        v2f vert_average(appdata_img v,uint vid : SV_VertexID)  
        {  
            v2f o;  
            o.pos = GetFullScreenTriangleVertexPosition(vid); //TransformObjectToHClip(v.vertex);  
            //uv坐标  
            o.uv_ = GetFullScreenTriangleTexCoord(vid); //v.texcoord.xy;  
            //计算周围的8个uv坐标
            o.uv1.xy = o.uv_ + _BlitTexture_TexelSize.xy * float2(1, 0) * _BlurWidth;  
            o.uv1.zw = o.uv_ + _BlitTexture_TexelSize.xy * float2(-1, 0) * _BlurWidth;
            
            o.uv2.xy = o.uv_ + _BlitTexture_TexelSize.xy * float2(0, 1) * _BlurWidth;
            o.uv2.zw = o.uv_ + _BlitTexture_TexelSize.xy * float2(0, -1) * _BlurWidth;
            
            o.uv3.xy = o.uv_ + _BlitTexture_TexelSize.xy * float2(1, 1) * _BlurWidth;
            o.uv3.zw = o.uv_ + _BlitTexture_TexelSize.xy * float2(-1, 1) * _BlurWidth;
            
            o.uv4.xy = o.uv_ + _BlitTexture_TexelSize.xy * float2(1, -1) * _BlurWidth;
            o.uv4.zw = o.uv_ + _BlitTexture_TexelSize.xy * float2(-1, -1) * _BlurWidth;
            return o;  
        }  
        
        // ---------------------------【片元着色器】---------------------------
        half4 frag_average(v2f i) : SV_Target  
        {  
            half4 color = half4(0,0,0,0);  
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv_.xy);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv1.xy);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv1.zw);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv2.xy);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv2.zw);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv3.xy);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv3.zw);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv4.xy);
            color += SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv4.zw);
            // 取平均值
            return color / 9;
        }
        
        
        ENDHLSL
        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
			#pragma fragment frag
            
            v2f vert (appdata_img v,uint vid : SV_VertexID)
            {
                v2f o;
				o.uv_ = GetFullScreenTriangleTexCoord(vid); //v.texcoord;
        		o.pos = GetFullScreenTriangleVertexPosition(vid); //TransformObjectToHClip(v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
            	half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv_);
            	clip(col.r - 0.1f);
            	return _OutlineColor;
            }
            
           ENDHLSL
        }


        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert_average
			#pragma fragment frag_average
            
            ENDHLSL
        }




        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert;
            #pragma fragment frag;

            v2f vert(appdata_img v,uint vid : SV_VertexID)
            {
                v2f o;
                o.pos = GetFullScreenTriangleVertexPosition(vid); //TransformObjectToHClip(v.vertex);
                o.uv_ = GetFullScreenTriangleTexCoord(vid); //v.texcoord;
                return o;
            }
            

			half4 frag(v2f i) : SV_Target
			{
				half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_linear_clamp_CameraOpaqueTexture, i.uv_);
				half4 blur = SAMPLE_TEXTURE2D_X(_BlurTex, sampler_linear_clamp_CameraOpaqueTexture,i.uv_);
				half4 outlieColor = SAMPLE_TEXTURE2D_X(_OutlineColorTex,sampler_linear_clamp_CameraOpaqueTexture, i.uv_);
				half4 o = outlieColor - blur;

				half sign = step(0.01h,o.a);
				half4 colMix = lerp(scene,_OutlineColor,o.a);
				return scene * (1-sign) + sign * colMix;
			}
            
            ENDHLSL
        }
        

    }
}

```



</details>
