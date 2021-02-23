/*
 * BundleTable 和 DepBundleTable

 *
 */

using System;
using UnityEngine;
using System.Collections.Generic;

using System.Text;
using System.IO;
using GStore;
using UnityEditor;

public class BundleTableCreateMgr
{
    #region =======内部接口======
    /// <summary>
    ///存在assetTable里面的ab
    /// </summary>
    public Dictionary<string, AssetBundleTable.BundleTableInfo> m_BundleDict = new Dictionary<string, AssetBundleTable.BundleTableInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 不在assetTable表的ab
    /// </summary>
    public Dictionary<string, AssetBundleTable.BundleTableInfo> m_DepbundleDict = new Dictionary<string, AssetBundleTable.BundleTableInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// bundleTable Scriptable路径
    /// </summary>
    public string BundleTableScriptablePath
    {
        get { return string.Format("Assets/{0}", AssetPathDefine.bundleTableFileName.Replace(".json", ".asset")); }
    }

    #endregion



    #region =========对外接口=======

    /// <summary>
    ///初始化
    /// </summary>
    /// <returns><c>true</c>, if bundle data was inited, <c>false</c> otherwise.</returns>
    public bool Clear()
    {
        m_BundleDict.Clear();
        m_DepbundleDict.Clear();
        return true;
    }

    /// <summary>
    /// 复制bundleTable.json和depBundleTable.json到ab的目录
    /// </summary>
    /// <param name="abFilePath">Ab file path.</param>
    public void CopyBundleTableToABPath(string abFilePath)
    {
        string src = ProjectBuilderSettings.Instance.bundleTableFile.Replace("\\", "/");
        string des = string.Format("{0}{1}", abFilePath, AssetPathDefine.bundleTableFileName).Replace("\\", "/");
        UnityEditor.FileUtil.ReplaceFile(src, des);
    }

    /// <summary>
    /// 记录abName
    /// </summary>
    /// <param name="assetKey"></param>
    /// <param name="assetPath"></param>
    /// <param name="bundleName"></param>
    public void RecordBundleName(string assetKey, string assetPath, string bundleName, bool isDep = false)
    {
        //依赖bundle没有索引
        if (string.IsNullOrEmpty(assetKey) != isDep)
        {
            return;
        }
        if (isDep)
        {
            assetKey = assetPath;
        }

        assetPath = isDep ? assetPath : Path.GetFileName(assetPath);
        AssetBundleTable.BundleTableInfo info = new AssetBundleTable.BundleTableInfo(assetKey, bundleName, assetPath);

        var targetDict = isDep ? m_DepbundleDict : m_BundleDict;

        AssetBundleTable.BundleTableInfo existInfo = null;
        if (targetDict.TryGetValue(assetKey, out existInfo))
        {
            if (existInfo.abn != info.abn || string.Equals(existInfo.path, info.path, StringComparison.OrdinalIgnoreCase) == false)
            {
                Debug.LogErrorFormat("existInfo={0}, nowInfo={1}", existInfo, info);
            }
        }
        else
        {
            targetDict[assetKey] = info;
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public bool SaveFile()
    {
        if (m_BundleDict.Count > 0)
        {
            SaveFile(m_BundleDict, ProjectBuilderSettings.Instance.bundleTableFile);
            //保存ScriptableObject
            SaveScriptableObject(m_BundleDict, BundleTableScriptablePath);
        }

        if (m_DepbundleDict.Count > 0)
        {
            SaveFile(m_DepbundleDict, ProjectBuilderSettings.Instance.depBundleTableFile);
        }

        return true;
    }

    private bool SaveFile(Dictionary<string, AssetBundleTable.BundleTableInfo> dict, string filepath)
    {
        try
        {
            List<AssetBundleTable.BundleTableInfo> list = new List<AssetBundleTable.BundleTableInfo>();
            foreach (var pair in dict)
            {
                list.Add(pair.Value);
            }

            string json = JsonUtil.ToPrettyJson(list, NewLine.Unix);
            File.WriteAllText(filepath, json, new UTF8Encoding(false));

        }
        catch (System.Exception ex)
        {
            Debug.LogError("====SaveFile error :" + ex.ToString());
        }

        return true;
    }

    /// <summary>
    /// 保存到Scriptable Object
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="filePath"></param>
    private void SaveScriptableObject(Dictionary<string, AssetBundleTable.BundleTableInfo> dict, string filePath)
    {
        try
        {
            BundleTableAsset bundleTableAsset = AssetDatabase.LoadAssetAtPath<BundleTableAsset>(filePath);
            if (bundleTableAsset == null)
            {
                bundleTableAsset = ScriptableObject.CreateInstance<BundleTableAsset>();
                AssetDatabase.CreateAsset(bundleTableAsset, filePath);
            }

            bundleTableAsset.listBundleTable = new List<AssetBundleTable.BundleTableInfo>(dict.Values);

            //设置assetBundleName，以便把BundleTable也打成assetBundle
            AssetImporter assetImporter = AssetImporter.GetAtPath(filePath);
            assetImporter.assetBundleName = Path.GetFileNameWithoutExtension(filePath).ToLower();

            EditorUtility.SetDirty(bundleTableAsset);
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogError("====SaveFile error :" + e.ToString());
        }
    }

    #endregion
}

