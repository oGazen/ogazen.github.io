### 平面水shader

<details open>
    <summary>WaterPanel</summary>

```hlsl
Shader "URP/WaterPanel"
{
    Properties
    {
        [MainTexture]_MainTex ("Texture", 2D) = "white" {}
        [MainColor]_Color ("Color",Color) = (1,1,1,1)

        // 第一步
        [Header(Depth And Color)]
        [Space()]
        _DepthIntensity ("Depth Intensity",Range(0,1)) = 0.05
        _EdgeColor ("Edge Color",Color) = (1,1,1,1)
        _WaterColor ("Water Color",Color) = (1,1,1,1)

        // 第二步 法线贴图
        [Header(Normal Texture)]
        [Space()]
        _NormalTex("Normal Texture",2D) = "white" {}
        _FlowSpeed("Flow Speed",Float) = 1
        _NormalMixture("Normal Mixture",Float) = 1


        // 第三步 高光
        [Header(Specular Color)]
        [Space()]
        _SpecularColor("Specular Color",Color)=(1,1,1,1)
        _SpecularIntensity("Specular Intensity",float)=1
        _Shininess("Shininess",float)=1


        // 第四步 反射 CubeMap
        [Header(Reflect Texture)]
        [Space()]
        _ReflectTex2("ReflectTex2",2D) = "white" {}
        _ReflectMixture("Reflect Mixture",float)=1
        _FresnelIntensity("Fresnel Intensity",float)=1



        // 第五步 水下折射扭曲
        [Header(Distort Texture)]
        [Space()]
        _DistortTex("DistortTex",2D) = "white"{}
        _Refract_X("Refract_X",float)=1
        _Refract_Y("Refract_Y",float)=1
        _DistortIntensity("Distort Intensity",float)=0 //折射纹理的扭曲强度


        // 第六步 水下焦散
        [Header(Caustic Texture)]
        [Space()]
        _CausticTex("CausticTex",2D)="white"{}
        _OffsetY("Caustic OffsetY",float)=1
        _CausticIntensity("Caustic Intensity",float)=1


        // 第七步 水面泡沫
        [Header(Foam Texture)]
        [Space()]
        _FoamTex("FoamTex",2D)="white"{}
        _FoamColor("Foam Color",Color)=(1,1,1,1)
        _FoamNoise("FoamNoise",float)=1
        _FoamMultiplier("Foam Multiplier",Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalRenderPipeline"
            "IgnoreProjector"="true"
            "Queue"="Transparent" // Transparent AlphaTest Geometry Background
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag



            TEXTURE2D_X(_MainTex);
            TEXTURE2D_X(_NormalTex);
            TEXTURE2D_X(_ReflectTex2);
            TEXTURE2D_X(_DistortTex);
            TEXTURE2D_X(_CausticTex);
            TEXTURE2D_X(_FoamTex);

            TEXTURE2D_X(_CameraDepthTexture);
            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _MainTex_ST;

            // 第一步
            half _DepthIntensity;
            half4 _EdgeColor;
            half4 _WaterColor;

            // 第二步
            half4 _NormalTex_ST;
            half _FlowSpeed;
            half _NormalMixture;

            // 第三步
            half4 _SpecularColor;
            half _SpecularIntensity;
            half _Shininess;

            // 第四步
            half _ReflectMixture;
            half _FresnelIntensity;
            half4 _ReflectTex2_ST;


            // 第五步
            half4 _DistortTex_ST;
            half _Refract_X;
            half _Refract_Y;
            half _DistortIntensity;


            // 第六步
            half4 _CausticTex_ST;
            half _OffsetY;
            half _CausticIntensity;


            // 第七步
            half4 _FoamTex_ST;
            half4 _FoamColor;
            half _FoamNoise;
            half _FoamMultiplier;
            CBUFFER_END


            struct Attributes
            {
                half4 position : POSITION;
                half2 uv : TEXCOORD0;

                // 第二步
                half4 normalUV : TEXCOORD1;
                half3 normal : NORMAL;
            };


            struct Varyings
            {
                half4 positionHCS : SV_POSITION;
                half2 uv : TEXCOORD0;

                // 第一步
                half3 positionView : TEXCOORD1;
                half3 positionWorld : TEXCOORD2;


                // 第二步
                half4 normalUV : TEXCOORD3;
                half3 normalWorld : TEXCOORD4;

                // 补丁
                half4 screenPos : TEXCOORD5;
            };


            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                // 第一步
                OUT.positionHCS = TransformObjectToHClip(IN.position);
                OUT.positionWorld = TransformObjectToWorld(IN.position);
                OUT.positionView = TransformWorldToView(OUT.positionWorld);

                // 第二步
                OUT.normalWorld = TransformObjectToWorldNormal(IN.normal);
                const half2 _mul1 = half2(1,1);
                const half2 _mul2 = half2(-1.15,0.8);
                OUT.normalUV.xy = TRANSFORM_TEX(IN.normalUV,_NormalTex) + _Time.y * _FlowSpeed * _mul1;
                OUT.normalUV.zw = TRANSFORM_TEX(IN.normalUV,_NormalTex) + _Time.y * _FlowSpeed * _mul2;
                OUT.uv = TRANSFORM_TEX(IN.uv,_FoamTex) + _Time.y * _FlowSpeed;

                // 补丁
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);

                return OUT;
            }



            half4 Frag(Varyings IN) : SV_Target
            {
                /* 第一步 */
                //根据深度差设置水面颜色
                half2 ScreenUV = IN.positionHCS.xy / _ScreenParams.xy; //屏幕UV
                half4 depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_MainTex,ScreenUV); //采样深度图
                half depth_eye = LinearEyeDepth(depthTex,_ZBufferParams); //将深度纹理的深度值转换到视图空间
                //（此时水体面片不渲染到深度图中）用深度图中的深度值与水体面片的在视图空间的深度值做差值，两者交接处颜色为0
                half depth_eye_viewZ = depth_eye + IN.positionView.z; //i.positionVS.z为负值，做差值时需两者相加
                //_depthIntensity用于调节深度差变化
                half depthDifference = saturate( depth_eye_viewZ * _DepthIntensity ); 
                //水面颜色
                half4 col = lerp(_EdgeColor,_WaterColor,depthDifference);
                // return col;



                /* 第二步 */
                //水面法线
                //利用两个流动不同方向的normalUV来采样法线纹理，最后将两者混合，做出水面流动的效果
                half4 NormalTex01 = SAMPLE_TEXTURE2D(_NormalTex,sampler_MainTex,IN.normalUV.xy);
                half4 NormalTex02 = SAMPLE_TEXTURE2D(_NormalTex,sampler_MainTex,IN.normalUV.zw);
                half4 normalCalc = NormalTex01 * NormalTex02;
                // return col + normalCalc;


                /* 第三步 */
                //水面高光
                //利用_NormalMixture来混合模型本身法线和法线纹理
                half3 N = normalize(lerp(IN.normalWorld,normalCalc,_NormalMixture));
                Light light = GetMainLight();
                half3 L = light.direction; // 看向灯光方向
                half3 V = normalize(_WorldSpaceCameraPos - IN.positionWorld); // 看向摄像机
                half3 H = normalize(L+V); // L 和 V 的中间向量
                half4 Specular = _SpecularColor * _SpecularIntensity * pow(max(0,dot(N,H)),_Shininess);
                // return Specular + col;



                /* 第四步 */
                //水面反射以及菲尼尔
                //利用_ReflectMixture来控制法线取值，与上一步制作高光的法线值区分
                half3 N1 = normalize( lerp(IN.normalWorld,normalCalc,_ReflectMixture) );
                // 通过Schlick近似菲涅尔函数权重取折射和反射采样 空气折射率1 水折射率1.333
                half Fresnel2 = pow(1 - saturate(dot(V, N1)), _FresnelIntensity);
                // return ReflectTex2;



                /* 第五步 */
                //水下的折射扭曲
                //利用水体面片的世界空间下坐标采样用于扭曲水下的纹理
                half3 DistortTex = UnpackNormal(SAMPLE_TEXTURE2D(_DistortTex,sampler_MainTex,IN.positionWorld.xz * half2(_Refract_X,_Refract_Y)+_Time.y));
                //偏移值 _DistortIntensity为扭曲强度值
                half2 offset = DistortTex.xy * _DistortIntensity;
                //扭曲UV值
                half2 DistortUV = ScreenUV + offset; 
                //利用扭曲后的屏幕UV采样深度图
                half DistortDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_MainTex,DistortUV);
                //将深度图中的深度信息转换到视图空间
                DistortDepth = LinearEyeDepth(DistortDepth,_ZBufferParams);
                //计算深度图和水体面片在视图空间中的深度差（i.positionVS.z为负值，所以两者要相加）
                half DistDepthDifferent = saturate(DistortDepth + IN.positionView.z);
                //消除水面以上物体的扭曲，因为水面以上物体的深度值小于i.positionVS.z,所以DistDepthDifferent值为负时，使水面以上物体采样屏幕纹理的UV为屏幕UV(不进行扭曲)
                if(!all(DistDepthDifferent)) DistortUV = ScreenUV;
                //最后利用上面计算得到的UV来采样屏幕纹理
                half4 RefractTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_MainTex,DistortUV);
                half4 ReflectTex2 = SAMPLE_TEXTURE2D(_ReflectTex2,sampler_MainTex,DistortUV);
                half4 refract_reflect = ReflectTex2 * Fresnel2 + RefractTex * (1-Fresnel2);






                /* 第六步 */
                //水下的焦散
                //因为焦散纹理需要处于水底，而我们只能使用水体面片来采样焦散纹理。所以可以求出深度图中的像素位置在世界空间下的值，用此值作为UV来采样焦散纹理
                half4 depthVS = 1;
                half depthTex_01 = normalize(depthTex);
                depthVS.z = depthTex_01;
                //求深度图中对应点的xy方向上的值
                depthVS.xy = IN.positionView.xy * depthTex_01 / -IN.positionView.z;
                //将求得的视图空间下深度图中的位置信息转换到世界空间下
                half3 depthWS = mul(unity_CameraToWorld,depthVS).xyz;
                //用深度图中像素的在世界空间下位置作为UV来采样焦散纹理
                //加depthWS.y的作用是使从上往下看同一个位置上的不同高度的点，在加上不同的高度值，从而使XZ方向的UV值错开从而采样不同的纹理
                half2 uv1 = (depthWS.xz * _CausticTex_ST.xy + depthWS.y * _OffsetY) + _Time.y * _FlowSpeed;
                const half2 _mul3 = half2(-1.15,0.9);
                half2 uv2 = (depthWS.xz * _CausticTex_ST.xy + depthWS.y * _OffsetY) + _Time.y * _FlowSpeed * _mul3;
                half4 CausticTex01 = SAMPLE_TEXTURE2D(_CausticTex,sampler_MainTex,uv1);
                half4 CausticTex02 = SAMPLE_TEXTURE2D(_CausticTex,sampler_MainTex,uv2);
                half4 CausticTex = min(CausticTex01,CausticTex02) * _CausticIntensity; //_CausticIntensity为焦散的强度值
                // return (CausticTex + refract_reflect + Specular) * col;




                /* 第七步 */
                //水面的泡沫
                //采样泡沫纹理
                half4 FoamTex = SAMPLE_TEXTURE2D(_FoamTex,sampler_MainTex,IN.uv);
                //_FoamNoise的值越大，FoamTex的黑色区域越多，后面求得的白色泡沫越少
                FoamTex = pow(abs(FoamTex),_FoamNoise);
                //此时靠近交接处的泡沫纹理的白色区域会形成泡沫遮罩
                half4 Foam = max(FoamTex * (1 - depth_eye_viewZ * _FoamMultiplier),0) * _FoamColor;
                // return Foam;





                /* 最终颜色合成 */
                half4 col_new = (CausticTex + refract_reflect + Specular) * col + Foam;
                return col_new;
            }



            ENDHLSL
        }
    }
}
```

</details>

### 摄像机平面反射

<details open>
    <summary>BMReflectPlane</summary>

```cs
using UnityEngine;
using UnityEngine.UI;

using System;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Camera = UnityEngine.Camera;


[ExecuteInEditMode]
public class BMReflectPlane : MonoBehaviour
{
        [Serializable]
        public enum ResolutionMulltiplier { Full, Half, Third, Quarter }

        [Serializable]
        public class PlanarReflectionSettings {
            public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Half;
            public float m_ClipPlaneOffset = 0.07f;
            public LayerMask m_ReflectLayers = -1;
            public bool m_Shadows;
        }

        [SerializeField]
        public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();
        public GameObject targetPlane;
        public float m_planeOffset;

        private static Camera _reflectionCamera;
        private RenderTexture _reflectionTexture;
        private readonly int _planarReflectionTextureId = Shader.PropertyToID("_ReflectTex2");
        private Material _waterPlaneMaterial;


        private void Start()
        {
            _waterPlaneMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        }


        // public static event Action<ScriptableRenderContext, Camera> BeginPlanarReflections;
        private void OnEnable() {

            RenderPipelineManager.beginCameraRendering += RunPlannarReflection;  // 订阅 beginCameraRendering 事件，加入平面反射函数
        }

        private void OnDisable() {
            Cleanup();
        }

        private void OnDestroy() {
            Cleanup();
        }

        private void Cleanup() {
            RenderPipelineManager.beginCameraRendering -= RunPlannarReflection;
            if(_reflectionCamera) {  // 释放相机
                _reflectionCamera.targetTexture = null;
                SafeDestroy(_reflectionCamera.gameObject);
            }
            if (_reflectionTexture) {  // 释放纹理
                RenderTexture.ReleaseTemporary(_reflectionTexture);
            }
        }
        private static void SafeDestroy(UnityEngine.Object obj) {
            if (Application.isEditor) {
                DestroyImmediate(obj);  //TODO
            }
            else {
                Destroy(obj);   //TODO
            }
        }
        private void RunPlannarReflection(ScriptableRenderContext context, Camera camera) {
            // we dont want to render planar reflections in reflections or previews
            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.SceneView)
                return;

            if (targetPlane == null) {
                targetPlane = gameObject;
            }
            if (_reflectionCamera == null) {
                _reflectionCamera = CreateReflectCamera();
            }

            var data = new PlanarReflectionSettingData(); // save quality settings and lower them for the planar reflections
            data.Set(); // set quality settings

            UpdateReflectionCamera(camera);  // 设置相机位置和方向等参数
            CreatePlanarReflectionTexture(camera);  // create and assign RenderTexture

            // BeginPlanarReflections?.Invoke(context, _reflectionCamera); // callback Action for PlanarReflection "?."确保event被订阅
            UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera); // render planar reflections  开始渲染函数
            // RenderPipeline.SubmitRenderRequest(_reflectionCamera,context);

            data.Restore(); // restore the quality settings
            // Shader.SetGlobalTexture(_planarReflectionTextureId, _reflectionTexture); // Assign texture to water shader
            _waterPlaneMaterial.SetTexture(_planarReflectionTextureId, _reflectionTexture);
        }

        private Vector2Int ReflectionResolution(Camera cam, float scale) {
            var x = (int)(cam.pixelWidth * scale * GetScaleValue());
            var y = (int)(cam.pixelHeight * scale * GetScaleValue());
            return new Vector2Int(x, y);
        }

        private float GetScaleValue() {
            switch(m_settings.m_ResolutionMultiplier) {
                case ResolutionMulltiplier.Full:
                    return 1f;
                case ResolutionMulltiplier.Half:
                    return 0.5f;
                case ResolutionMulltiplier.Third:
                    return 0.33f;
                case ResolutionMulltiplier.Quarter:
                    return 0.25f;
                default:
                    return 0.5f; // default to half res
            }
        }

        private void CreatePlanarReflectionTexture(Camera cam) {
            if (_reflectionTexture == null) {
                var res = ReflectionResolution(cam, UniversalRenderPipeline.asset.renderScale);  // 获取 RT 的大小
                // const bool useHdr10 = true;
                // const RenderTextureFormat hdrFormat = useHdr10 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
                // GraphicsFormatUtility.GetGraphicsFormat(hdrFormat, true);
                _reflectionTexture = RenderTexture.GetTemporary(res.x, res.y, 16, RenderTextureFormat.RGB111110Float);
            }

            _reflectionCamera.targetTexture =  _reflectionTexture; // 将 RT 赋予相机
        }
        private void UpdateCamera(Camera src, Camera dest) {
            if (dest == null) return;

            // dest.CopyFrom(src);
            dest.aspect = src.aspect;
            dest.cameraType = src.cameraType;   // 这个参数不同步就错
            dest.clearFlags = src.clearFlags;
            dest.fieldOfView = src.fieldOfView;
            dest.depth = src.depth;
            dest.farClipPlane = src.farClipPlane;
            dest.focalLength = src.focalLength;
            dest.useOcclusionCulling = false;
            if (dest.gameObject.TryGetComponent(out UniversalAdditionalCameraData camData)) {  // TODO
                camData.renderShadows = m_settings.m_Shadows; // turn off shadows for the reflection camera
            }
        }

        // Calculates reflection matrix around the given plane
        private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 reflectionMat = Matrix4x4.identity;
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;

            return reflectionMat;
        }
        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
            var offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
            var m = cam.worldToCameraMatrix;
            var cameraPosition = m.MultiplyPoint(offsetPos);
            var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
        }

        private void UpdateReflectionCamera(Camera curCamera) {
            if (targetPlane == null) {
                Debug.LogError("target plane is null!");
            }

            Vector3 planeNormal = targetPlane.transform.up;
            Vector3 planePos = targetPlane.transform.position + planeNormal * m_planeOffset;

            UpdateCamera(curCamera, _reflectionCamera);  // 同步当前相机数据

            // 获取视空间平面，使用反射矩阵，将图像根据平面对称上下颠倒
            var planVS = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, -Vector3.Dot(planeNormal, planePos));
            Matrix4x4 reflectionMat = CalculateReflectionMatrix(planVS);
            _reflectionCamera.worldToCameraMatrix = curCamera.worldToCameraMatrix * reflectionMat;
            // 斜截视锥体
            var clipPlane = CameraSpacePlane(_reflectionCamera, planePos, planeNormal, 1.0f);
            var newProjectionMat = CalculateObliqueMatrix(curCamera, clipPlane);
            _reflectionCamera.projectionMatrix = newProjectionMat;
            _reflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer

        }


        private Matrix4x4 CalculateObliqueMatrix(Camera cam, Vector4 plane) {
                Vector4 Q_clip = new Vector4(Mathf.Sign(plane.x), Mathf.Sign(plane.y), 1f, 1f);
                Vector4 Q_view = cam.projectionMatrix.inverse.MultiplyPoint(Q_clip);

                Vector4 scaled_plane = plane * 2.0f / Vector4.Dot(plane, Q_view);
                Vector4 M3 = scaled_plane - cam.projectionMatrix.GetRow(3);

                Matrix4x4 new_M = cam.projectionMatrix;
                new_M.SetRow(2, M3);

                // 使用 unity API
                // var new_M = cam.CalculateObliqueMatrix(plane);
                return new_M;
        }

        private Camera CreateReflectCamera() {
            var go = new GameObject("Planar Reflection Camera (Auto)",typeof(Camera));
            go.transform.SetParent(transform);
            var cameraData = go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;

            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraData.requiresDepthOption = CameraOverrideOption.Off;
            cameraData.renderShadows = false;
            cameraData.SetRenderer(0);  // 根据 render list 的索引选择 render TODO

            var t = transform;
            var reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.transform.SetPositionAndRotation(transform.position, t.rotation);  // 相机初始位置设为当前 gameobject 位置
            reflectionCamera.depth = -10;  // 渲染优先级 [-100, 100]
            reflectionCamera.enabled = false;
            reflectionCamera.orthographic = true;
            // go.hideFlags = HideFlags.HideAndDontSave;

            return reflectionCamera;
        }

        class PlanarReflectionSettingData {
            private readonly bool _fog;
            private readonly int _maxLod;
            private readonly float _lodBias;
            private bool _invertCulling;

            public PlanarReflectionSettingData() {
                _fog = RenderSettings.fog;
                _maxLod = QualitySettings.maximumLODLevel;
                _lodBias = QualitySettings.lodBias;
            }

            public void Set() {
                _invertCulling = GL.invertCulling;
                GL.invertCulling = !_invertCulling;  // 因为镜像后绕序会反，将剔除反向
                RenderSettings.fog = false; // disable fog for now as it's incorrect with projection
                QualitySettings.maximumLODLevel = 1;
                QualitySettings.lodBias = _lodBias * 0.5f;
            }

            public void Restore() {
                GL.invertCulling = _invertCulling;
                RenderSettings.fog = _fog;
                QualitySettings.maximumLODLevel = _maxLod;
                QualitySettings.lodBias = _lodBias;
            }
        }


}
```

</details>

### 注意事项

1. 开启`Opaque Texture`和`Depth Texture`

2. 对反射平面添加`BMReflectPlane`脚本组件

### 参考链接

1. [Unity中URP实现水体（水下的扭曲）](https://jishuzhan.net/article/1762484976412004354)
2. [Unity水面渲染研究（CubeMap，反射探针，平面反射）](https://www.cnblogs.com/strawberryPudding/p/17192083.html)
3. [URP - 水效果Shader_unity 水shader](https://blog.csdn.net/2401_83878847/article/details/148369745)
4. [Unity 实现平面反射（基于 URP）](https://zhuanlan.zhihu.com/p/493766119)
5. [Unity Shader 水多种元素的实现（反射、折射、菲涅尔、深浅、浪花/泡沫、水波、可交互）](https://blog.csdn.net/m0_37686228/article/details/120426167)


