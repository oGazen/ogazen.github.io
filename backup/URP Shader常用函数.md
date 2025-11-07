[URP 文档](https://docs.unity.cn/cn/Packages-cn/com.unity.render-pipelines.universal@14.1/manual/)

### 在自定义 URP 着色器中转换坐标

> 1.在着色器文件的 `HLSLPROGRAM` 内部添加 `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"`。`Core.hlsl` 文件会自动导入 `ShaderVariablesFunction.hlsl` 文件。
> 2.使用 `ShaderVariablesFunction.hlsl` 文件中的以下方法之一。

| **方法**                       | **语法**                                                                   | **描述**
| :------------------------ | :------------------------------------------------------ | :--------------------------------- |
| `GetNormalizedScreenSpaceUV`     | `float2 GetNormalizedScreenSpaceUV(float2 positionInClipSpace)`              | 将裁剪空间中的位置转换为屏幕空间。                                                                       |
| `GetObjectSpaceNormalizeViewDir` | `half3 GetObjectSpaceNormalizeViewDir(float3 positionInObjectSpace)` | 将物体空间中的位置转换为朝向观察者的标准化方向。 |
| `GetVertexNormalInputs` | `VertexNormalInputs GetVertexNormalInputs(float3 normalInObjectSpace)` | 将物体空间中的法线转换为世界空间中的切线、双切线和法线。你还可以同时输入法线和 `float4` 形式的切线 |
| `GetVertexPositionInputs`       | `VertexPositionInputs GetVertexPositionInputs(float3 positionInObjectSpace)` | 将物体空间中的顶点位置转换为世界空间、视图空间、裁剪空间和标准化设备坐标（NDC）。 |
| `GetWorldSpaceNormalizeViewDir` | `half3 GetWorldSpaceNormalizeViewDir(float3 positionInWorldSpace)`           | 计算世界空间中的位置到观察者的方向，并进行标准化。                                |
| `GetWorldSpaceViewDir`          | `float3 GetWorldSpaceViewDir(float3 positionInWorldSpace)`                   | 计算世界空间中的位置到观察者的方向。                                              |


#### VertexPositionInputs

使用 `GetVertexPositionInputs` 方法可以获取此结构体。

| **字段**           | **描述**                  |
| -------------------------- | --------------------------------- |
| `float3 positionWS`  | 世界空间中的位置。              |
| `float3 positionVS`  | 视图空间中的位置。              |
| `float4 positionCS`  | 裁剪空间中的位置。              |
| `float4 positionNDC` | 归一化设备坐标（NDC）中的位置。 |

 


#### VertexNormalInputs

使用 `GetVertexNormalInputs` 方法可以获取此结构体。

| **字段**          | **描述**       |
| ------------------------- | ---------------------- |
| `real3 tangentWS`   | 世界空间中的切线。   |
| `real3 bitangentWS` | 世界空间中的双切线。 |
| `float3 normalWS`   | 世界空间中的法线。   |


### 在自定义 URP 着色器中使用相机
> 1. 在着色器文件中的 `HLSLPROGRAM` 内部添加 `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"`。`Core.hlsl` 文件会导入 `ShaderVariablesFunction.hlsl` 文件。
> 2. 使用以下方法之一，它们都在 `ShaderVariablesFunction.hlsl` 文件中定义。

| **方法**                | **语法**                                           | **描述**                                                                                                                                               |
| ------------------------------- | ---------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetCameraPositionWS`     | `float3 GetCameraPositionWS()`                       | 返回相机的世界空间位置。                                                                                                                                     |
| `GetScaledScreenParams`   | `float4 GetScaledScreenParams()`                     | 返回屏幕的宽度和高度（以像素为单位）。                                                                                                                       |
| `GetViewForwardDir`       | `float3 GetViewForwardDir()`                         | 返回视图的前向方向（世界空间）。                                                                                                                             |
| `IsPerspectiveProjection` | `bool IsPerspectiveProjection()`                     | 如果相机的投影设置为透视投影，则返回 `true`。                                                                                                            |
| `LinearDepthToEyeDepth`   | `half LinearDepthToEyeDepth(half linearDepth)`       | 将线性深度缓冲区值转换为视图深度。 |
| `TransformScreenUV`       | `void TransformScreenUV(inout float2 screenSpaceUV)` | 如果 Unity 使用的是颠倒的坐标空间，则翻转屏幕空间位置的 y 坐标。您还可以输入 `uv` 和屏幕高度（作为 `float`），方法将输出缩放到屏幕大小的像素位置。   |

### 在自定义 URP 着色器中使用光照

> 1. 在着色器文件中的 `HLSLPROGRAM` 内部添加 `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"`。
> 2. 使用以下部分中的任何方法。

#### 获取光照数据

`Lighting.hlsl` 文件导入了 `RealtimeLights.hlsl` 文件，该文件包含以下方法。

| **方法**                 | **语法**                                                               | **描述**                                                                                                             |
| -------------------------------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| `GetMainLight`             | `Light GetMainLight()`                                                   | 返回场景中的主光源。                                                                                                       |
| `GetAdditionalLight`       | `Light GetAdditionalLight(uint lightIndex, float3 positionInWorldSpace)` | 返回影响 `positionWS` 的 `lightIndex` 额外光源。例如，如果 `lightIndex` 为 `0`，该方法返回第一个额外光源。 |
| `GetAdditionalLightsCount` | `int GetAdditionalLightsCount()`                                         | 返回额外光源的数量。                                                                                                       |


#### 为表面法线计算光照

| **方法**         | **语法**                                                                                                                                              | **描述**                                                                                                                                                           |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `LightingLambert`  | `half3 LightingLambert(half3 lightColor, half3 lightDirection, half3 surfaceNormal)`                                                                    | 使用 Lambert 模型计算并返回表面法线的漫反射光照。                                                                                                                        |
| `LightingSpecular` | `half3 LightingSpecular(half3 lightColor, half3 lightDirection, half3 surfaceNormal, half3 viewDirection, half4 specularAmount, half smoothnessAmount)` | 使用 简单阴影 计算并返回表面法线的高光反射光照。 |

#### 计算环境光遮蔽

`Lighting.hlsl` 文件导入了 `AmbientOcclusion.hlsl` 文件，该文件包含以下方法。

| **方法**                       | **语法**                                                                              | **描述**                                                                  |
| -------------------------------------- | --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `SampleAmbientOcclusion`         | `half SampleAmbientOcclusion(float2 normalizedScreenSpaceUV)`                           | 返回屏幕空间中指定位置的环境光遮蔽值，其中 0 表示遮蔽，1 表示未遮蔽。           |
| `GetScreenSpaceAmbientOcclusion` | `AmbientOcclusionFactor GetScreenSpaceAmbientOcclusion(float2 normalizedScreenSpaceUV)` | 返回屏幕空间中指定位置的间接和直接环境光遮蔽值，其中 0 表示遮蔽，1 表示未遮蔽。 |


#### AmbientOcclusionFactor

使用 `GetScreenSpaceAmbientOcclusion` 方法返回此结构体。

| **字段**                      | **描述**                                               |
| ------------------------------------- | -------------------------------------------------------------- |
| `half indirectAmbientOcclusion` | 由物体遮挡间接光导致的阴影量，表示物体受到环境光遮蔽的程度。 |
| `half directAmbientOcclusion`   | 由物体遮挡直接光导致的阴影量，表示物体受到直接光遮蔽的程度。 |

#### Light

使用 `GetMainLight` 和 `GetAdditionalLight` 方法返回此结构体。

| **字段**                  | **描述**                         |
| --------------------------------- | ---------------------------------------- |
| `half3 direction`           | 光源的方向。                           |
| `half3 color`               | 光源的颜色。                           |
| `float distanceAttenuation` | 光源的强度，基于光源与物体之间的距离。 |
| `half shadowAttenuation`    | 光源的强度，基于物体是否在阴影中。     |
| `uint layerMask`            | 光源的层掩码。                         |


### 在自定义 URP 着色器中使用阴影

要在自定义 Universal Render Pipeline (URP) 着色器中使用阴影，请按照以下步骤操作：

1. 在着色器文件的 `HLSLPROGRAM` 内部添加 `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"`。`Core.hlsl` 文件会自动导入 `Shadows.hlsl` 和 `RealtimeLights.hlsl` 文件。
2. 使用以下部分的方法。

#### 获取阴影空间中的位置

使用这些方法将位置转换为阴影贴图坐标。

| **方法**                    | **语法**                                                        | **描述**                                                                                                                                                                                                                                        |
| ----------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetShadowCoord`              | `float4 GetShadowCoord(VertexPositionInputs vertexInputs)`        | 将顶点位置转换为阴影空间。有关 `VertexPositionInputs` 结构体的信息 |
| `TransformWorldToShadowCoord` | `float4 TransformWorldToShadowCoord(float3 positionInWorldSpace)` | 将世界空间中的位置转换为阴影空间。                                                                                                                                                                                                                    |

#### 计算阴影

以下方法使用阴影贴图计算阴影。在使用这些方法之前，请先完成以下步骤：

1. 确保场景中存在包含 `ShadowCaster` 着色器 Pass 的物体，例如使用 `Universal Render Pipeline/Lit` 着色器的物体。
2. 在着色器中添加 `#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN`，以便访问主光源的阴影贴图。
3. 在着色器中添加 `#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS`，以便访问额外光源的阴影贴图。

| **方法**                      | **语法**                                                                                    | **描述**                                                                                                                                        |
| ------------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetMainLight`                  | `Light GetMainLight(float4 shadowCoordinates)`                                                | 返回场景中的主光源，并根据阴影坐标位置计算 `shadowAttenuation` 值。                                                                               |
| `ComputeCascadeIndex`           | `half ComputeCascadeIndex(float3 positionInWorldSpace)`                                       | 返回世界空间中位置的阴影级联索引。 | `half MainLightRealtimeShadow(float4 shadowCoordinates)`                                      | 在指定的阴影坐标位置从主光源的阴影贴图中获取阴影值。有关更多信息，请参阅 [阴影映射](https://docs.unity.cn/cn/tuanjiemanual/Manual/shadow-mapping.html)。 |
| `AdditionalLightRealtimeShadow` | `half AdditionalLightRealtimeShadow(int lightIndex, float3 positionInWorldSpace)`             | 在世界空间中的指定位置，从额外光源的阴影贴图中获取阴影值。                                                                                            |
| `GetMainLightShadowFade`        | `half GetMainLightShadowFade(float3 positionInWorldSpace)`                                    | 基于世界空间位置与相机的距离，返回主光源阴影的渐变程度。                                                                                              |
| `GetAdditionalLightShadowFade`  | `half GetAdditionalLightShadowFade(float3 positionInWorldSpace)`                              | 基于世界空间位置与相机的距离，返回额外光源阴影的渐变程度。                                                                                            |
| `ApplyShadowBias`               | `float3 ApplyShadowBias(float3 positionInWorldSpace, float3 normalWS, float3 lightDirection)` | 在世界空间的位置上添加阴影偏移。 |
