using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 自动重导shader
/// </summary>
public class ShaderImportSettings : AssetPostprocessor
{
    /// <summary>
    /// 自动导入shader
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        List<string> cgincList = null;
        foreach (var assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".cginc") == false)
            {
                continue;
            }

            if (cgincList == null)
            {
                cgincList = new List<string>();
            }

            cgincList.Add(assetPath);
        }
        if (cgincList == null || cgincList.Count == 0)
        {
            return;
        }
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        //加上依赖的文件
        cgincList = FindCgincReferences(cgincList);

        HashSet<string> regexList = new HashSet<string>();
        foreach (string assetPath in cgincList)
        {
            regexList.Add(GetRegexPattern(assetPath));
        }

        //获取所有打AB的shader
        List<string> shaderList = GetAllShadersFromAssetBundle();
        if (shaderList == null || shaderList.Count == 0)
        {
            return;
        }

        HashSet<string> matchShaderSet = new HashSet<string>();
        foreach (string assetPath in shaderList)
        {
            string text = File.ReadAllText(assetPath);
            //匹配cginc文件
            foreach (string regex in regexList)
            {
                if (Regex.IsMatch(text, regex))
                {
                    matchShaderSet.Add(assetPath);
                    break;
                }
            }
        }
        if (matchShaderSet.Count == 0)
        {
            return;
        }

        foreach (string assetPath in matchShaderSet)
        {
            Debug.LogFormat("Reimport:{0}", assetPath);
            AssetDatabase.ImportAsset(assetPath);
        }

        //删除AB以触发重打
        AssetBundleBuildTools.ClearShaderAB();

        sw.Stop();
        Debug.LogFormat("检测到cginc文件修改，自动更新{0}个关联shader, 耗时：{1}", matchShaderSet.Count, sw.Elapsed.ToString());
    }

    /// <summary>
    /// 获取所有设置了ABName的shader
    /// </summary>
    /// <returns></returns>
    private static List<string> GetAllShadersFromAssetBundle()
    {
        List<string> list = new List<string>();
        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(ProjectBuilderSettings.Instance.shaderAssetBundleName);
        foreach (string assetPath in assetPaths)
        {
            if (assetPath.EndsWith(".shader") == false)
            {
                continue;
            }
            list.Add(assetPath);
        }
        return list;
    }

    /// <summary>
    /// 获取所有cginc文件
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, string> GetAllCgincTexts()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in assetPaths)
        {
            if (assetPath.EndsWith(".cginc") == false)
            {
                continue;
            }
            dict.Add(assetPath, File.ReadAllText(assetPath));
        }
        return dict;
    }

    /// <summary>
    /// 获取依赖cginc
    /// </summary>
    /// <param name="sourceList"></param>
    /// <returns></returns>
    private static List<string> FindCgincReferences(List<string> sourceList)
    {
        List<string> resultSet = new List<string>();
        FindCgincReferences(sourceList, resultSet, GetAllCgincTexts());
        return resultSet;
    }

    /// <summary>
    /// 获取依赖cginc
    /// </summary>
    /// <param name="sourceList"></param>
    /// <param name="allCginc"></param>
    /// <returns></returns>
    private static void FindCgincReferences(List<string> sourceList, List<string> resultList, Dictionary<string, string> allCginc)
    {
        foreach (var source in sourceList)
        {
            resultList.Add(source);
            allCginc.Remove(source);
        }

        List<string> depList = new List<string>();
        foreach (var kvp in allCginc)
        {
            foreach (var source in sourceList)
            {
                if (Regex.IsMatch(kvp.Value, GetRegexPattern(source)))
                {
                    depList.Add(kvp.Key);
                    break;
                }
            }
        }
        if (depList.Count > 0)
        {
            //搜索下一级引用
            FindCgincReferences(depList, resultList, allCginc);
        }
    }

    /// <summary>
    /// 获取正则表达式
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static string GetRegexPattern(string assetPath)
    {
        return Path.GetFileName(assetPath);
    }
}