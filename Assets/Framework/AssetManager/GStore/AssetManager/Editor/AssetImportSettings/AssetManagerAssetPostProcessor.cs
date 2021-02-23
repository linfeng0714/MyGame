using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace GStore.Editor
{
    /// <summary>
    /// 资源导入处理器
    /// </summary>
    public class AssetManagerAssetPostProcessor : AssetPostprocessor
    {
        /// <summary>
        /// Unity提供的资源导入回调接口
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AssetInfo info;
            foreach (var assetPath in importedAssets)
            {
                if (AssetTable.AssetPathMap.TryGetValue(assetPath, out info))
                {
                    SetupAddress(info.assetPath, info.id);
                }
            }

            foreach (var assetPath in movedAssets)
            {
                if (AssetTable.AssetPathMap.TryGetValue(assetPath, out info))
                {
                    SetupAddress(info.assetPath, info.id);
                }
            }
        }

        /// <summary>
        /// 配置地址，asset表中的资源自动设置Address
        /// </summary>
        private static void SetupAddress(string assetPath, int id)
        {
            //跳过文件夹
            if (AssetTable.FolderMap.ContainsKey(assetPath))
            {
                return;
            }
            //跳过shader
            if (assetPath.EndsWith(".shader"))
            {
                return;
            }
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return;
            }

            if (assetPath != importer.assetPath)
            {
                Debug.LogWarningFormat("资源表命名大小写不一致！表格={0}, 实际={1}", assetPath, importer.assetPath);
            }

            //图集
            if (IsSpriteAtlas(importer))
            {
                return;
            }

            string abName = string.Format("{0}{1}", id, AssetPathDefine.assetBundleExtension);
            if (importer.assetBundleName != abName)
            {
                importer.assetBundleName = abName;
            }
        }

        private static bool IsSpriteAtlas(AssetImporter importer)
        {
            TextureImporter textureImporter = importer as TextureImporter;
            return textureImporter != null && string.IsNullOrEmpty(textureImporter.spritePackingTag) == false;
        }

        /// <summary>
        /// 配置整张表
        /// </summary>
        [MenuItem("GStore/Asset/导入Asset表资源")]
        public static void SetupAssetTable()
        {
            int i = 0;
            int count = AssetTable.AssetPathMap.Count;
            foreach (var kvp in AssetTable.AssetPathMap)
            {
                i++;

                var assetPath = kvp.Value.assetPath;
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    EditorTools.DisplayProgressBar("导入asset表资源", importer.assetPath, (float)i / count);
                    SetupAddress(assetPath, kvp.Value.id);
                }
            }
            EditorTools.ClearProgressBar();
        }
    }
}

