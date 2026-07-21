using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

public class ModelProcessCustom : AssetPostprocessor
{
    private const string modelPath = "Assets/Models";
    private static bool isCustomImport = false;
    //模型导入之前调用
    public void OnPreprocessModel()
    {
        var model = this.assetImporter as ModelImporter;

        #region 设置模型  import Settings
        // model
        model.importBlendShapes = false; // 导入混合形状
        model.importCameras = false; // 导入摄像机
        model.importLights = false; // 导入灯光
        model.meshCompression = ModelImporterMeshCompression.Medium; // Mesh 压缩程度
        model.importNormals = ModelImporterNormals.Import; // 定义计算法线方式

        // Rig
        model.animationType = ModelImporterAnimationType.None; // 定义导入模型动画
        model.optimizeBones = false;

        // Animation
        model.importAnimation = false; // 禁用导入模型动画[其实本身也没有]

        // Materials
        model.materialImportMode = ModelImporterMaterialImportMode.None;
        #endregion

        #region test
        /*
        model.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        model.materialLocation = ModelImporterMaterialLocation.InPrefab;
        model.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
        model.materialSearch = ModelImporterMaterialSearch.Local;

        var sourceMaterials = typeof(ModelImporter)
                .GetProperty("sourceMaterials", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(model) as AssetImporter.SourceAssetIdentifier[];

        foreach (var identifier in sourceMaterials ?? Enumerable.Empty<AssetImporter.SourceAssetIdentifier>())
        {
            model.AddRemap(identifier, defaultMaterial);
        }*/

        #endregion
        Debug.Log($"OnPreprocessModel={this.assetPath}");
    }

    //将此函数添加到一个子类中，以在材质从 Model Importer 导入时接收通知。
    public void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] materialAnimation)
    {
        Debug.Log("OnPreprocessMaterialDescription=" + description.materialName);
    }

    // 提供源材质 > 向 MeshRenderer 分配材质之前，系统会调用 OnAssignMaterialModel 函数
    public Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
        var defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Model/Gray.mat");
        Debug.Log("OnAssignMaterialModel=" + material.name);
        return defaultMaterial;
    }

    // 当变换层级视图已完成导入时调用此函数
    public void OnPostprocessMeshHierarchy(GameObject go)
    {
        Debug.Log("OnPostprocessMeshHierarchy=" + go.name);
    }

    //模型导入完成调用
    public void OnPostprocessModel(GameObject go)
    {
        Debug.Log("OnPostprocessModel=" + go.name);
    }

}
