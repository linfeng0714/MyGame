using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Framework.AssetManager;

/// <summary>
/// 安装资源管理器
/// </summary>
public static class AssetManagerSetup
{
    /// <summary>
    /// 安装各种方法
    /// </summary>
    static public void Setup()
    {
        //安装表格数据读取方法
        //CSVHelper.loadBytes = (path) =>
        //{
        //    return AssetManager.Instance.LoadBytes(path);
        //};

        ////设置资源表的适配器
        //AssetInfo.getAssetTable = () =>
        //{
        //    List<AssetInfo> assetInfoList = null;

        //    CSVAssets.Load();

        //    assetInfoList = new List<AssetInfo>(CSVAssets.GetAllDic(true).Count);
        //    foreach (var kvp in CSVAssets.GetAllDic(true))
        //    {
        //        CSVAssets csv = kvp.Value;
        //        AssetInfo assetInfo = new AssetInfo(csv.id, csv.dir, csv.name, csv.suffix);
        //        assetInfoList.Add(assetInfo);
        //    }

        //    CSVAssets.UnLoad();

        //    return assetInfoList;
        //};
    }
}

