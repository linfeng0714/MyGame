using UnityEditor;
using UnityEngine;

/// <summary>
/// 模型设置 - 自动删除fbx文件自带的材质
/// </summary>
public class FBXSetting : AssetPostprocessor
{
    /// <summary>
    /// 导入模型前自动执行的接口
    /// </summary>
    void OnPreprocessModel()
    {
        ModelImporter _modelImporter = (ModelImporter)assetImporter;
        CommonMeshSetting(_modelImporter);
    }

    /// <summary>
    /// 自动删除导入的材质
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var assetPath in importedAssets)
        {
            if (assetPath.ToLower().EndsWith(".fbx") == false)
            {
                continue;
            }
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                continue;
            }

            string matDir = System.IO.Path.GetDirectoryName(assetPath) + "/Materials";

            if (System.IO.Directory.Exists(matDir))
            {
                System.IO.Directory.Delete(matDir, true);
            }
            string matDirMeta = matDir + ".meta";
            if (System.IO.File.Exists(matDirMeta))
            {
                System.IO.File.Delete(matDirMeta);
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 通用配置，锁定不可修改
    /// </summary>
    void CommonMeshSetting(ModelImporter _modelImporter)
    {
        _modelImporter.importMaterials = true;
        _modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
        _modelImporter.materialLocation = ModelImporterMaterialLocation.External;
        _modelImporter.importVisibility = false;
        _modelImporter.importLights = false;
        _modelImporter.importCameras = false;
    }
}
