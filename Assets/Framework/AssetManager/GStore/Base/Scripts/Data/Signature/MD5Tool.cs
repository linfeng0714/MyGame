﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.IO;

public static class MD5Tool
{
    public static string GetMD5(byte[] bytes)
    {
        try
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytes);
            return BytesToHexString(retVal);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("GetMD5() fail, error:" + ex.Message);
            return null;
        }
    }

    public static string GetMD5(Stream stream)
    {
        try
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(stream);
            return BytesToHexString(retVal);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("GetMD5() fail, error:" + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 二进制hash转string
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static string BytesToHexString(byte[] hash)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static byte[] GetMD5Bytes(byte[] bytes)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        return md5.ComputeHash(bytes);
    }

    public static byte[] GetMD5Bytes(Stream stream)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        return md5.ComputeHash(stream);
    }

    /// <summary>
    /// 检验md5
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool ValidateMD5(Stream stream, byte[] val, out byte[] md5)
    {
        md5 = GetMD5Bytes(stream);
        return ValidateMD5(val, md5);
    }

    public static bool ValidateMD5(byte[] val, byte[] md5)
    {
        if (md5.Length != val.Length)
        {
            return false;
        }
        for (int i = 0; i < md5.Length; i++)
        {
            if (md5[i] != val[i])
            {
                return false;
            }
        }
        return true;
    }
}
