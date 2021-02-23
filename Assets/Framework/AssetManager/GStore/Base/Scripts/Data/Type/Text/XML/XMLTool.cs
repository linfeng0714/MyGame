using System;
using System.IO;
//#if UNITY_EDITOR
using System.Xml;
//#endif
using UnityEngine;
using Mono.Xml;
using System.Security;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// XML工具类
/// </summary>
public static class XMLTool
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static string ToString(byte[] array)
    {
        if (array.Length > 3)
        {
            //去除bom
            if (array[0] == 0xef && array[1] == 0xbb && array[2] == 0xbf)
            {
                return System.Text.Encoding.UTF8.GetString(array, 3, array.Length - 3);
            }
        }        

        return System.Text.Encoding.UTF8.GetString(array, 0, array.Length);
    }

    public static SecurityElement SelectSingleNode(SecurityElement root, string v)
    {
        return root.SearchForChildByTag(v);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string Attribute(SecurityElement node, string name)
    {
        return node.Attribute(name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <param name="str_default"></param>
    /// <returns></returns>
    public static string Attribute(SecurityElement node, string name, string str_default)
    {
        string value = node.Attribute(name);
        if (value == null)
        {
            return str_default;
        }
        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool HasAttribute(SecurityElement node, string name)
    {
        return !(string.IsNullOrEmpty(node.Attribute(name)));
    }

    public static byte GetByteAttribute(SecurityElement node, string name, byte defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseByte(result, defaultValue);
    }
    public static short GetShortAttribute(SecurityElement node, string name, short defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseShort(result, defaultValue);
    }
    public static int GetIntAttribute(SecurityElement node, string name, int defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseInt(result, defaultValue);
    }
    public static long GetLongAttribute(SecurityElement node, string name, long defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseLong(result, defaultValue);
    }
    public static double GetDoubleAttribute(SecurityElement node, string name, double defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseDouble(result, defaultValue);
    }
    public static float GetFloatAttribute(SecurityElement node, string name, float defaultValue = 0)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseFloat(result, defaultValue);
    }
    public static bool GetBoolAttribute(SecurityElement node, string name, bool defaultValue = false)
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return ParseBool(result, defaultValue);
    }
    public static string GetStringAttribute(SecurityElement node, string name, string defaultValue = "")
    {
        if (node == null)
        {
            return defaultValue;
        }
        if (name == null)
        {
            return defaultValue;
        }

        string result = Attribute(node, name);
        if (string.IsNullOrEmpty(result))
        {
            return defaultValue;
        }
        return result;
    }
    public static Vector2 GetVector2Attribute(SecurityElement node, string name, Vector2 defaultValue)
    {
        Vector2 v2 = defaultValue;

        if (node == null)
        {
            return v2;
        }
        if (name == null)
        {
            return v2;
        }
        string _str = Attribute(node, name);
        return ParseVector2(_str, defaultValue);
    }

    public static Vector3 GetVector3Attribute(SecurityElement node, string name)
    {
        return GetVector3Attribute(node, name, Vector3.zero);
    }
    public static Vector3 GetVector3Attribute(SecurityElement node, string name, Vector3 defaultValue)
    {
        Vector3 v3 = defaultValue;

        if (node == null)
        {
            return v3;
        }
        if (name == null)
        {
            return v3;
        }
        string _str = Attribute(node, name);
        return ParseVector3(_str,defaultValue);
    }
    public static VInt2 GetVInt2Attribute(SecurityElement node, string name, VInt2 defaultValue)
    {
        VInt2 v2 = defaultValue;

        if (node == null)
        {
            return v2;
        }
        if (name == null)
        {
            return v2;
        }
        string _str = Attribute(node, name);
        return ParseVInt2(_str, defaultValue);
    }
    public static VInt3 GetVInt3Attribute(SecurityElement node, string name, VInt3 defaultValue)
    {
        VInt3 v3 = defaultValue;

        if (node == null)
        {
            return v3;
        }
        if (name == null)
        {
            return v3;
        }
        string _str = Attribute(node, name);
        return ParseVInt3(_str, defaultValue);
    }
    public static Color GetColorAttribute(SecurityElement node, string name, Color defaultValue)
    {
        Color color = defaultValue;
        if (node == null || string.IsNullOrEmpty(name))
        {
            return color;
        }
        string _str = Attribute(node, name);
        if (string.IsNullOrEmpty(_str))
        {
            return color;
        }
        return ParseColor(_str, defaultValue);
    }

    public static XMLNodeList GetNodeList(this SecurityElement node, string name)
    {
        if (node == null || node.Children == null || node.Children.Count <= 0)
        {
            return null;
        }
        XMLNodeList list = new XMLNodeList();
        for (int i = 0; i < node.Children.Count; i++)
        {
            SecurityElement child = node.Children[i] as SecurityElement;
            if (child.Tag == name)
            {
                list.Add(child);
            }
        }
        return list;
    }
    public static bool ContainsKey(this SecurityElement node, string key)
    {
        if (node.Attributes != null && node.Attributes.ContainsKey(key))
        {
            return true;
        }
        return node.SearchForChildByTag(key) != null;
    }

    public static XmlElement SetAttributeVector3(this XmlElement ele, string key, Vector3 value)
    {
        if (key == null || ele == null)
        {
            return ele;
        }
        string svalue = "" + value.x + "," + value.y + "," + value.z;
        ele.SetAttribute(key, svalue);
        return ele;
    }
    public static XmlElement SetAttributeVector2(this XmlElement ele, string key, Vector2 value)
    {
        if (key == null || ele == null)
        {
            return ele;
        }
        string svalue = "" + value.x + "," + value.y;
        ele.SetAttribute(key, svalue);
        return ele;
    }
    public static XmlElement SetAttributeColor(XmlElement ele, string key, Color value)
    {
        if (key == null || ele == null)
        {
            return ele;
        }
        string svalue = string.Format("{0},{1},{2},{3}",value.r,value.g,value.b,value.a);
        ele.SetAttribute(key, svalue);
        return ele;
    }

    public static int ParseObjInt(object obj, int defaultValue = 0)
    {
        if (obj == null)
        {
            return defaultValue;
        }

        return ParseInt(obj.ToString(), defaultValue);
    }
    public static byte ParseByte(string text, byte defaultValue = 0)
    {
        byte _return = 0;
        if (byte.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static short ParseShort(string text, short defaultValue = 0)
    {
        short _return = 0;
        if (short.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static int ParseInt(string text, int defaultValue = 0)
    {
        int _return = 0;
        if (int.TryParse(text, out _return))
            return _return;
        else
        {
            //兼容旧数据浮点数读取
            float _f_return = 0;
            if (float.TryParse(text, out _f_return))
            {
                return (int)(_f_return * 100);
            }
        }
        return defaultValue;
    }
    public static float ParseFloat(string text, float defaultValue = 0)
    {
        float _return = 0;
        if (float.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static long ParseLong(string text, long defaultValue = 0)
    {
        long _return = 0;
        if (long.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static double ParseDouble(string text, double defaultValue = 0)
    {
        double _return = 0;
        if (double.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static bool ParseBool(string text, bool defaultValue = false)
    {
        bool _return = false;
        if (bool.TryParse(text, out _return))
            return _return;
        else
            return defaultValue;
    }
    public static Vector3 ParseVector3(string _str, Vector3 defaultValue)
    {
        Vector3 v3 = defaultValue;
        if (string.IsNullOrEmpty(_str))
        {
            return v3;
        }
        string[] strs = _str.Split(',');
        int len = strs.Length;
        if (len == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                string ele = strs[i];
                if (ele == null)
                {
                    ele = "0";
                }
                float num = 0;
                float.TryParse(ele, out num);
                switch (i)
                {
                    case 0:
                        v3.x = num;
                        break;
                    case 1:
                        v3.y = num;
                        break;
                    default:
                        v3.z = num;
                        break;
                }
            }
        }
        return v3;
    }
    public static Vector2 ParseVector2(string _str, Vector2 defaultValue)
    {
        Vector2 v2 = defaultValue;
        if (string.IsNullOrEmpty(_str))
        {
            return v2;
        }
        string[] strs = _str.Split(',');
        int len = strs.Length;
        if (len == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                string ele = strs[i];
                if (ele == null)
                {
                    ele = "0";
                }
                float num = 0;
                float.TryParse(ele, out num);
                switch (i)
                {
                    case 0:
                        v2.x = num;
                        break;
                    case 1:
                        v2.y = num;
                        break;
                }
            }
        }
        return v2;
    }
    public static VInt3 ParseVInt3(string _str, VInt3 defaultValue)
    {
        VInt3 v3 = defaultValue;
        if (string.IsNullOrEmpty(_str))
        {
            return v3;
        }
        string[] strs = _str.Split(',');
        int len = strs.Length;
        if (len == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                string ele = strs[i];
                if (ele == null)
                {
                    ele = "0";
                }
                int num = 0;
                int.TryParse(ele, out num);
                switch (i)
                {
                    case 0:
                        v3.x = num;
                        break;
                    case 1:
                        v3.y = num;
                        break;
                    default:
                        v3.z = num;
                        break;
                }
            }
        }
        return v3;
    }
    public static VInt2 ParseVInt2(string _str, VInt2 defaultValue)
    {
        VInt2 v2 = defaultValue;

        if (string.IsNullOrEmpty(_str))
        {
            return v2;
        }
        string[] strs = _str.Split(',');
        int len = strs.Length;
        if (len == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                string ele = strs[i];
                if (ele == null)
                {
                    ele = "0";
                }
                int num = 0;
                int.TryParse(ele, out num);
                switch (i)
                {
                    case 0:
                        v2.x = num;
                        break;
                    case 1:
                        v2.y = num;
                        break;
                }
            }
        }
        return v2;
    }
    public static Color ParseColor(string _str, Color defaultValue)
    {
        Color color = defaultValue;
        if (string.IsNullOrEmpty(_str))
        {
            return color;
        }
        string[] strs = _str.Split(',');
        int len = strs.Length;
        if (len == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                string ele = strs[i];
                if (ele == null)
                {
                    ele = "0";
                }
                float num = 0;
                float.TryParse(ele, out num);
                switch (i)
                {
                    case 0:
                        color.r = num;
                        break;
                    case 1:
                        color.g = num;
                        break;
                    case 2:
                        color.b = num;
                        break;
                    default:
                        color.a = num;
                        break;
                }
            }
        }
        return color;
    }


}
