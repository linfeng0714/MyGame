using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ReferenceCache
{
    private static ReferenceCache s_Cache;

    /// <summary>
    /// 替代AssetDatabase接口，利用缓存提速
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public static string[] GetDependencies(string assetPath, bool recursive)
    {
        Debug.Assert(recursive == false);

        if (s_Cache == null)
        {
            s_Cache = new ReferenceCache();
            s_Cache.ReadFromCache(false);
        }

        s_Cache.ImportAsset(assetPath);
        var dependencies = s_Cache.assetDict[AssetDatabase.AssetPathToGUID(assetPath)].dependencies;
        string[] result = new string[dependencies.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = AssetDatabase.GUIDToAssetPath(dependencies[i]);
        }

        return result;
    }
    /// <summary>
    /// 保存到文件
    /// </summary>
    public static void Save()
    {
        s_Cache.WriteToCache();
    }

    //缓存路径
    private const string CACHE_PATH = "Library/ReferenceCache";
    //资源引用信息字典
    public Dictionary<string, AssetDescription> assetDict = new Dictionary<string, AssetDescription>();

    //收集资源引用信息并更新缓存
    public void CollectDependenciesInfo()
    {
        try
        {
            ReadFromCache(false);
            var allAssets = AssetDatabase.GetAllAssetPaths();
            int totalCount = allAssets.Length;
            for (int i = 0; i < allAssets.Length; i++)
            {
                //每遍历100个Asset，更新一下进度条，同时对进度条的取消操作进行处理
                if (EditorTools.DisplayCancelableProgressBar("Refresh", string.Format("Collecting {0} assets", i), (float)i / totalCount))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                string path = allAssets[i];
                if (path.StartsWith("Assets/") || path.StartsWith("Packages/"))
                {
                    ImportAsset(path);
                }

                if (i % 2000 == 0)
                    GC.Collect();
            }
            //将信息写入缓存
            EditorUtility.DisplayCancelableProgressBar("Refresh", "Write to cache", 1f);
            WriteToCache();
            //生成引用数据
            EditorUtility.DisplayCancelableProgressBar("Refresh", "Generating asset reference info", 1f);
            UpdateReferenceInfo();
            EditorUtility.ClearProgressBar();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            EditorUtility.ClearProgressBar();
        }
    }

    //通过依赖信息更新引用信息
    private void UpdateReferenceInfo()
    {
        foreach (var asset in assetDict)
        {
            foreach (var assetGuid in asset.Value.dependencies)
            {
                assetDict[assetGuid].references.Add(asset.Key);
            }
        }
    }

    //生成并加入引用信息
    private void ImportAsset(string path)
    {
        //通过path获取guid进行储存
        string guid = AssetDatabase.AssetPathToGUID(path);
        //获取该资源的最后修改时间，用于之后的修改判断
        Hash128 assetDependencyHash = AssetDatabase.GetAssetDependencyHash(path);
        //如果assetDict没包含该guid或包含了修改时间不一样则需要更新
        if (!assetDict.ContainsKey(guid) || assetDict[guid].assetDependencyHash != assetDependencyHash)
        {
            //将每个资源的直接依赖资源转化为guid进行储存
            var dependencies = AssetDatabase.GetDependencies(path, false);
            var guids = dependencies.Select(p => AssetDatabase.AssetPathToGUID(p)).ToList();

            //生成asset依赖信息，被引用需要在所有的asset依赖信息生成完后才能生成
            AssetDescription ad = new AssetDescription();
            ad.path = path;
            ad.assetDependencyHash = assetDependencyHash;
            ad.dependencies = guids;

            assetDict[guid] = ad;

            foreach (var depPath in dependencies)
            {
                ImportAsset(depPath);
            }
        }
    }

    //读取缓存信息
    public bool ReadFromCache(bool updateReferenceInfo = true)
    {
        assetDict.Clear();
        try
        {
            if (File.Exists(CACHE_PATH))
            {
                var serializedGuid = new List<string>();
                var serializedDependencyHash = new List<Hash128>();
                var serializedDenpendencies = new List<int>();
                var serializedDenpendenciesLengths = new List<int>();
                //反序列化数据
                using (FileStream fs = File.OpenRead(CACHE_PATH))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    serializedGuid = (List<string>)bf.Deserialize(fs);
                    serializedDependencyHash = ((List<string>)bf.Deserialize(fs)).ConvertAll((input) =>
                    {
                        return Hash128.Parse(input);
                    });
                    serializedDenpendencies = (List<int>)bf.Deserialize(fs);
                    serializedDenpendenciesLengths = (List<int>)bf.Deserialize(fs);
                }

                for (int i = 0; i < serializedGuid.Count; ++i)
                {
                    string path = AssetDatabase.GUIDToAssetPath(serializedGuid[i]);
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        var ad = new AssetDescription();
                        ad.path = path;
                        ad.assetDependencyHash = serializedDependencyHash[i];
                        assetDict.Add(serializedGuid[i], ad);
                    }
                }

                int lastIndex = 0;
                for (int i = 0; i < serializedGuid.Count; ++i)
                {
                    string guid = serializedGuid[i];
                    if (assetDict.ContainsKey(guid))
                    {
                        int length = serializedDenpendenciesLengths[i];
                        var indecies = serializedDenpendencies.GetRange(lastIndex, length);
                        lastIndex += length;

                        var guids = indecies.
                            Select(index => serializedGuid[index]).
                            Where(g => assetDict.ContainsKey(g)).
                            ToList();
                        assetDict[guid].dependencies = guids;
                    }
                }
                if (updateReferenceInfo)
                {
                    UpdateReferenceInfo();
                }
                return true;
            }
        }
        catch (Exception e)
        {
            assetDict.Clear();
            Debug.LogError("读取缓存失败！");
            Debug.LogException(e);
            return false;
        }

        return false;
    }

    //写入缓存
    public void WriteToCache()
    {
        if (File.Exists(CACHE_PATH))
            File.Delete(CACHE_PATH);

        var serializedGuid = new List<string>();
        var serializedDependencyHash = new List<string>();
        var serializedDenpendencies = new List<int>();
        var serializedDenpendenciesLengths = new List<int>();
        //辅助映射字典
        var guidIndex = new Dictionary<string, int>();
        //序列化
        using (FileStream fs = File.OpenWrite(CACHE_PATH))
        {
            foreach (var pair in assetDict)
            {
                guidIndex.Add(pair.Key, guidIndex.Count);
                serializedGuid.Add(pair.Key);
                serializedDependencyHash.Add(pair.Value.assetDependencyHash.ToString());
            }

            foreach (var guid in serializedGuid)
            {
                int[] indexes = assetDict[guid].dependencies.Select(s => guidIndex[s]).ToArray();
                serializedDenpendenciesLengths.Add(indexes.Length);
                serializedDenpendencies.AddRange(indexes);
            }

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, serializedGuid);
            bf.Serialize(fs, serializedDependencyHash);
            bf.Serialize(fs, serializedDenpendencies);
            bf.Serialize(fs, serializedDenpendenciesLengths);
        }
    }

    //更新引用信息状态
    public void UpdateAssetState(string guid)
    {
        AssetDescription ad;
        if (assetDict.TryGetValue(guid, out ad) && ad.state != AssetState.NODATA)
        {
            if (File.Exists(ad.path))
            {
                //修改时间与记录的不同为修改过的资源
                if (ad.assetDependencyHash != AssetDatabase.GetAssetDependencyHash(ad.path))
                {
                    ad.state = AssetState.CHANGED;
                }
                else
                {
                    //默认为普通资源
                    ad.state = AssetState.NORMAL;
                }
            }
            //不存在为丢失
            else
            {
                ad.state = AssetState.MISSING;
            }
        }

        //字典中没有该数据
        else if (!assetDict.TryGetValue(guid, out ad))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ad = new AssetDescription();
            ad.path = path;
            ad.state = AssetState.NODATA;
            assetDict.Add(guid, ad);
        }
    }

    //根据引用信息状态获取状态描述
    public static string GetInfoByState(AssetState state)
    {
        switch (state)
        {
            case AssetState.CHANGED:
                return "<color=#F0672AFF>Changed</color>";
            case AssetState.MISSING:
                return "<color=#FF0000FF>Missing</color>";
            case AssetState.NODATA:
                return "<color=#FFE300FF>No Data</color>";
            case AssetState.RECURSIVE:
                return "<color=#FFE300FF>Recursive</color>";
            case AssetState.BUILTIN:
                return "<color=#FFE300FF>BuiltIn</color>";
            default:
                return "Normal";
        }
    }

    public class AssetDescription
    {
        public string path = "";
        public Hash128 assetDependencyHash;
        public List<string> dependencies = new List<string>();
        public List<string> references = new List<string>();
        public AssetState state = AssetState.NORMAL;
    }

    public enum AssetState
    {
        NORMAL,
        CHANGED,
        MISSING,
        NODATA,
        RECURSIVE,
        BUILTIN,
    }
}
