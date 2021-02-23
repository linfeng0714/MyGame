/*
 * 自动生成代码
 * 请勿编辑
 */
using System.Collections.Generic;
using System;
using UnityEngine;

public class CSVAssets : CSVData
{
    private static readonly CSVAssets ms_instance = new CSVAssets();

    public static CSVAssets Instance
    {
        get
        {
            return ms_instance;
        }
    }

    public Dictionary<byte[], CSVAssets> csvDataDic = new Dictionary<byte[], CSVAssets>(new ByteArrayComparer());

    //定义字段
    /// <summary>
    /// id
    /// </summary>
    public int id { get; private set; }
    /// <summary>
    /// 目录
    /// </summary>
    public string dir { get; private set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string name { get; private set; }
    /// <summary>
    /// 后缀
    /// </summary>
    public string suffix { get; private set; }


    static string tableName = "assets";

    //对应的csv文件
    protected override string Name()
    {
        return tableName;
    }

    public static void Load()
    {
        Instance.LoadCSVTable();
    }
    
    /// <summary>
    /// 获取对象(通过多个Key,最多5个key)
    /// </summary>
    public static CSVAssets GetAssets(int id)
    {
        byte[] key = CSVHelper.GetKey(id);
        CSVAssets data = Instance.Get(key);
        if(data == null)
        {
            Debug.LogErrorFormat("{0} 表 key {1} 出错了",tableName,id);  
        }
        return data;
    }
   

    public static Dictionary<byte[], CSVAssets> GetAllDic(bool isCache = false)
    {
        return Instance.GetAll(isCache);
    }

    public static void UnLoad(bool isRemove = true)
    {
        Instance.UnLoadData(isRemove);
    }
    
    /// <summary>
    /// 通过ID获取对象
    /// </summary>
    private CSVAssets Get(byte[] key, bool isCache = true)
    {
        LoadCSVTable();
        CSVAssets csvData;
        if (!csvDataDic.TryGetValue(key, out csvData))
        {
            CSVBytesData bytesData = GetCSVBytesData(key);
            if (bytesData == null)
            {
                return null;
            }
            csvData = GetCSVData(bytesData);
            if (isCache)
            {
                if (csvDataDic.ContainsKey(key))
                {
                    csvDataDic.Add(key, csvData);
                }
            }
        }
        return csvData;
    }

    protected CSVAssets GetCSVData(CSVBytesData bytesData)
    {
        CSVAssets csvData = null;
        try
        {
            csvData = new CSVAssets();
            csvData.bytesData = bytesData;
            bytesData.BeginLoad();
            //设置字段值
    	    csvData.id = bytesData.ReadIntValue();
            csvData.dir = bytesData.ReadString();
            csvData.name = bytesData.ReadString();
            csvData.suffix = bytesData.ReadString();

        }
        catch (Exception exception)
        {
            Debug.LogErrorFormat("{0}表 解析出错 {1}",tableName, exception.StackTrace);
            return null;
        }

        return csvData;
    }
    


    private Dictionary<byte[], CSVAssets> GetAll(bool isCache)
    {
        LoadCSVTable();
        Dictionary<byte[], CSVAssets> allDic = new Dictionary<byte[], CSVAssets> ();
        if (csvDataDic.Count == GetAllCSVBytesData().Count)
        {
            allDic = csvDataDic;
        }
        else
        {
            csvDataDic.Clear();
            var _itor = GetAllCSVBytesData().GetEnumerator();
            while (_itor.MoveNext())
            {
                CSVAssets csvData = Get(_itor.Current.Key);
                allDic.Add(_itor.Current.Key, csvData);
            }
            if (isCache)
            {
                csvDataDic = allDic;
            }
        }
        return allDic;
    }

    public override void UnLoadData(bool isRemove = true)
    {
        base.UnLoadData(isRemove);
        csvDataDic.Clear();
        if (isRemove)
        {
            CSVManager.Instance.RemoveCSVData(Name());
        }
    }
}
