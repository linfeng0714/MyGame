using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TableField
{
    public string strDefine;
    public TableDefine define;
    public string strFieldType;
    public TableBaseType fieldType;
    public bool isBase = true;
    public bool isList = false;

    public const String BYTE = "byte";
    public const String SHORT = "short";
    public const String INT = "int";
    public const String FLOAT = "float";
    public const String BOOL = "bool";
    public const String STRING = "string";
    public const String ULONG = "ulong";

    public const String BeTranslate = "translate";
    public const String KEY = "key";
    public const String DEL = "del";

    public void SetDefine(string strDefine)
    {
        this.strDefine = strDefine;
        switch (strDefine)
        {
            case KEY:
                define = TableDefine.Key;
                break;
            case BeTranslate:
                define = TableDefine.BeTranslate;
                break;
            default:
                define = TableDefine.Null;
                break;
        }
    }

    public void SetFieldType(string strFieldType)
    {
        this.strFieldType = strFieldType;
        switch (strFieldType)
        {
            case BYTE:
                fieldType = TableBaseType.BYTE;
                break;
            case SHORT:
                fieldType = TableBaseType.SHORT;
                break;
            case INT:
                fieldType = TableBaseType.INT;
                break;
            case FLOAT:
                fieldType = TableBaseType.FLOAT;
                break;
            case BOOL:
                fieldType = TableBaseType.BOOL;
                break;
            case STRING:
                fieldType = TableBaseType.STRING;
                break;
            case ULONG:
                fieldType = TableBaseType.ULONG;
                break;
            default:
                throw new Exception("SetFieldType");
        }
    }
}

/// <summary>
/// 最终代码的类型
/// </summary>
public enum TableBaseType
{
    NULL,
    BYTE,
    SHORT,
    INT,
    FLOAT,
    BOOL,
    STRING,
    ULONG,
}

public enum TableDefine
{
    Null,
    Key,
    BeTranslate,
}

/// <summary>
/// 二进制存储类型
/// </summary>
public enum AttrType
{
    Type_Null,
    Type_Zero,
    Type_SByte,
    Type_Short,
    Type_Int,
    Type_Long,
    Type_ULong,
    Type_Byte,
    Type_UShort,
    Type_UInt,
}
