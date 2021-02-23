using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// CSV解析类
/// </summary>
public class CSVHelper
{
    public const string PATH_NAME = "Data/Table";

    public static string Combine(string csv_name)
    {
        return Path.Combine(PATH_NAME, csv_name);
    }

    /// <summary>
    /// 文本过滤器
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public delegate string TextFilter(string text, string tablename);
    public static TextFilter textFilter = null;

    /// <summary>
    /// 数据加载委托
    /// </summary>
    public delegate byte[] LoadBytesHandler(string path);
    public static LoadBytesHandler loadBytes { set; private get; }


    public static byte[] GetBytes(string table_path)
    {
        if (loadBytes == null)
        {
            Debug.LogError("没有实现加载委托！请先设置loadBytes！");
            return null;
        }

        return loadBytes(table_path);
  
    }

    #region 获取唯一键值
    public static ulong GetKey(int key1)
    {
        if (key1 < 0)
        {
            Debug.LogError($"请检查 读表 key 不能小于0  key1={key1}");
            return 0;
        }
        return Convert.ToUInt64(key1);
    }

    public static ulong GetKey(int key1, int key2)
    {
        if (key1 < 0 || key2 < 0)
        {
            Debug.LogError($"请检查 读表 key 不能小于0   key1={key1}, key2={key2}");
            return 0;
        }
        return (((ulong)key1 & 0xffffffff) | (((ulong)key2 & 0xffffffff) << 32));
    }

    public static ulong GetKey(int key1, int key2, int key3)
    {
        if (key1 < 0 || key2 < 0 || key3 < 0)
        {
            Debug.LogError($"请检查 读表 key 不能小于0  key1={key1}, key2={key2} ,key3={key3}");
            return 0;
        }
        short shortKey2 = Convert.ToInt16(key2);
        short shortKey3 = Convert.ToInt16(key3);
        return (((ulong)key1 & 0xffffffff) | (((ulong)shortKey2 & 0xffff) << 32) | (((ulong)shortKey3 & 0xffff) << 48));
    }

    public static ulong GetKey(int key1, int key2, int key3, int key4)
    {
        if (key1 < 0 || key2 < 0 || key3 < 0 || key4 < 0)
        {
            Debug.LogError($"请检查 读表 key 不能小于0  key1={key1}, key2={key2} ,key3={key3} ,key4={key4}");
            return 0;
        }
        short shortKey1 = Convert.ToInt16(key1);
        short shortKey2 = Convert.ToInt16(key2);
        short shortKey3 = Convert.ToInt16(key3);
        short shortKey4 = Convert.ToInt16(key4);
        return (((ulong)shortKey1 & 0xffff) | (((ulong)shortKey2 & 0xffff) << 16) | (((ulong)shortKey3 & 0xffff) << 32) | (((ulong)shortKey4 & 0xffff) << 48));
    }

    public static ulong GetKey(int key1, int key2, int key3, int key4, int key5)
    {
        Debug.LogWarning("不支持5个key");
        return 0;
    }
    #endregion

}
