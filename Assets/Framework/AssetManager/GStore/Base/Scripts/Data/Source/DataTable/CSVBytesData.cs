using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CSVBytesData
{
    List<TableField> fieldList;
    List<int> cursorIndex;
    private byte[] fieldData;

    private bool isLog = true;

    private int cursor;

    private string tableName;

    public void Init(byte[] allFieldData, List<TableField> fieldList,string tableName)
    {
        this.fieldData = allFieldData;
        this.fieldList = fieldList;
        this.tableName = tableName;
    }

    public void SetCursorIndex()
    {
        if (cursorIndex != null) return;
        cursorIndex = new List<int>();
        cursorIndex.Clear();
        int tempCursor = 0;
        foreach (TableField item in fieldList)
        {
            cursorIndex.Add(tempCursor);
#if TableLog
            Log("tempCursor {0} isList {1} baseType {2}", tempCursor,item.isList,item.fieldType);
#endif
            if (!item.isList)
            {
                switch (item.fieldType)
                {
                    case TableBaseType.BOOL:
                        tempCursor++;
                        break;
                    case TableBaseType.BYTE:
                        tempCursor++;
                        break;
                    case TableBaseType.SHORT:
                        tempCursor += 2;
                        break;
                    case TableBaseType.INT:
                        AttrType intAttrType = (AttrType)fieldData[tempCursor];
                        tempCursor++;
                        switch (intAttrType)
                        {
                            case AttrType.Type_Zero:
                                break;
                            case AttrType.Type_Byte:
                                tempCursor++;
                                break;
                            case AttrType.Type_SByte:
                                tempCursor++;
                                break;
                            case AttrType.Type_Short:
                                tempCursor += 2;
                                break;
                            case AttrType.Type_UShort:
                                tempCursor += 2;
                                break;
                            case AttrType.Type_Int:
                                tempCursor += 4;
                                break;
                            default:
                                throw new Exception("数据读取出错了");
                        }
                        break;
                    case TableBaseType.ULONG:
                        AttrType ulongAttrType = (AttrType)fieldData[tempCursor];
                        tempCursor++;
                        switch (ulongAttrType)
                        {
                            case AttrType.Type_Zero:
                                break;
                            case AttrType.Type_Byte:
                                tempCursor++;
                                break;
                            case AttrType.Type_UShort:
                                tempCursor += 2;
                                break;
                            case AttrType.Type_UInt:
                                tempCursor += 4;
                                break;
                            case AttrType.Type_ULong:
                                tempCursor += 8;
                                break;
                            default:
                                throw new Exception("数据读取出错了");
                        }
                        break;
                    case TableBaseType.FLOAT:
                        tempCursor += 4;
                        break;
                    case TableBaseType.STRING:
                        int length = BitConverter.ToInt32(fieldData, tempCursor);
                        tempCursor += 4;
                        tempCursor += length;
                        break;
                }
            }
            else
            {
                int listLength = BitConverter.ToInt32(fieldData, tempCursor);
                tempCursor += 4;
                switch (item.fieldType)
                {
                    case TableBaseType.BOOL:
                        tempCursor += listLength;
                        break;
                    case TableBaseType.BYTE:
                        tempCursor += listLength;
                        break;
                    case TableBaseType.SHORT:
                        tempCursor += listLength * 2;
                        break;
                    case TableBaseType.INT:
                        for (int i = 0; i < listLength; i++)
                        {
                            AttrType intAttrType = (AttrType)fieldData[tempCursor];
                            tempCursor++;
                            switch (intAttrType)
                            {
                                case AttrType.Type_Zero:
                                    break;
                                case AttrType.Type_Byte:
                                    tempCursor++;
                                    break;
                                case AttrType.Type_SByte:
                                    tempCursor++;
                                    break;
                                case AttrType.Type_Short:
                                    tempCursor += 2;
                                    break;
                                case AttrType.Type_UShort:
                                    tempCursor += 2;
                                    break;
                                case AttrType.Type_Int:
                                    tempCursor += 4;
                                    break;
                                default:
                                    throw new Exception("数据读取出错了");
                            }
                        }
                        break;
                    case TableBaseType.ULONG:
                        for (int i = 0; i < listLength; i++)
                        {
                            AttrType ulongAttrType = (AttrType)fieldData[tempCursor];
                            tempCursor++;
                            switch (ulongAttrType)
                            {
                                case AttrType.Type_Zero:
                                    break;
                                case AttrType.Type_Byte:
                                    tempCursor++;
                                    break;
                                case AttrType.Type_UShort:
                                    tempCursor += 2;
                                    break;
                                case AttrType.Type_UInt:
                                    tempCursor += 4;
                                    break;
                                case AttrType.Type_ULong:
                                    tempCursor += 8;
                                    break;
                                default:
                                    throw new Exception("数据读取出错了");
                            }
                        }
                        break;
                    case TableBaseType.FLOAT:
                        tempCursor += listLength * 4;
                        break;
                    case TableBaseType.STRING:
                        for (int i = 0; i < listLength; i++)
                        {
                            int length = BitConverter.ToInt32(fieldData, tempCursor);
                            tempCursor += 4;
                            tempCursor += length;
                        }
                        break;
                }
            }
        }
    }

    public void BeginLoad()
    {
        cursor = 0;
    }

    private AttrType ReadAttrType()
    {
        AttrType attr = (AttrType)fieldData[cursor];
        cursor++;
        return attr;
    }

    public bool ReadToBoolean()
    {
        bool boolValue = BitConverter.ToBoolean(fieldData, cursor);
        cursor += 1;
#if TableLog
        Log("boolValue {0}", boolValue);
#endif
        return boolValue;
    }

    public sbyte ReadToSByte()
    {
        sbyte sbyteValue;
        byte byteValue = fieldData[cursor];
        if (byteValue >= 128)
        {
            sbyteValue = (sbyte)(byteValue - 256);
        }
        else
        {
            sbyteValue = (sbyte)byteValue;
        }
        cursor++;
#if TableLog
        Log("sbyteValue {0}", sbyteValue);
#endif
        return sbyteValue;
    }

    public byte ReadToByte()
    {
        byte byteValue = fieldData[cursor];
        cursor++;
#if TableLog
        Log("byteValue {0}", byteValue);
#endif
        return byteValue;
    }

    public ushort ReadToUInt16()
    {
        ushort ushortValue = BitConverter.ToUInt16(fieldData, cursor);
        cursor += 2;
#if TableLog
        Log("ushortValue {0}", ushortValue);
#endif
        return ushortValue;
    }

    public short ReadToInt16()
    {
        short shortValue = BitConverter.ToInt16(fieldData, cursor);
        cursor += 2;
#if TableLog
        Log("shortValue {0}", shortValue);
#endif
        return shortValue;
    }

    public uint ReadToUInt32()
    {
        uint uintValue = BitConverter.ToUInt32(fieldData, cursor);
        cursor += 4;
#if TableLog
        Log("uintValue {0}", uintValue);
#endif
        return uintValue;
    }

    public int ReadToInt32()
    {
        int intValue = BitConverter.ToInt32(fieldData, cursor);
        cursor += 4;
#if TableLog
        Log("intValue {0}", intValue);
#endif
        return intValue;
    }

    public float ReadToSingle()
    {
        float floatValue = BitConverter.ToSingle(fieldData, cursor);
        cursor += 4;
#if TableLog
        Log("floatValue {0}", floatValue);
#endif
        return floatValue;
    }

    public ulong ReadToUInt64()
    {
        ulong ulongValue = BitConverter.ToUInt64(fieldData, cursor);
        cursor += 8;
#if TableLog
        Log("ulongValue {0}", ulongValue);
#endif
        return ulongValue;
    }

    public long ReadToInt64()
    {
        long longValue = BitConverter.ToInt64(fieldData, cursor);
        cursor += 8;
#if TableLog
        Log("longValue {0}", longValue);
#endif
        return longValue;
    }

    public string ReadString()
    {
        int length = BitConverter.ToInt32(fieldData, cursor);
        cursor += 4;
#if TableLog
        Log("string length {0}", length);
#endif
        string strValue = Encoding.UTF8.GetString(fieldData, cursor, length).Replace("\\n", "\n");
        cursor += length;
#if TableLog
        Log("string stringValue {0}",strValue);
#endif
        return strValue;
    }

    public int ReadIntValue()
    {
        AttrType intAttrType = ReadAttrType();
#if TableLog
        Log("------intAttrType------ {0}", intAttrType);
#endif
        switch (intAttrType)
        {
            case AttrType.Type_Zero:
                return (int)0;
            case AttrType.Type_Byte:
                return (int)ReadToByte();
            case AttrType.Type_SByte:
                return (int)ReadToSByte();
            case AttrType.Type_Short:
                return (int)ReadToInt16();
            case AttrType.Type_UShort:
                return (int)ReadToUInt16();
            case AttrType.Type_Int:
                return (int)ReadToInt32();
            default:
                throw new Exception("数据读取出错了");
        }
    }

    public ulong ReadULongValue()
    {
        AttrType ulongAttrType = ReadAttrType();
#if TableLog
        Log("------ulongAttrType------ {0}", ulongAttrType);
#endif
        switch (ulongAttrType)
        {
            case AttrType.Type_Zero:
                return (ulong)0;
            case AttrType.Type_Byte:
                return (ulong)ReadToByte();
            case AttrType.Type_UShort:
                return (ulong)ReadToUInt16();
            case AttrType.Type_UInt:
                return (ulong)ReadToUInt32();
            case AttrType.Type_ULong:
                return (ulong)ReadToUInt64();
            default:
                throw new Exception("数据读取出错了");
        }
    }

    public int ReadListLength()
    {
        int length = BitConverter.ToInt32(fieldData, cursor);
        cursor += 4;
#if TableLog
        Log("======listLength===== {0}", length);
#endif
        return length;
    }

    #region Lua访问接口

    public bool ReadBool(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return ReadToBoolean();
    }

    public byte ReadByte(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return  ReadToByte();
    }

    public short ReadShort(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return ReadToInt16();
    }

    public int ReadInt(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return ReadIntValue();
    }

    public ulong ReadULong(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return ReadULongValue();
    }

    public float ReadFloat(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        return ReadToSingle();
    }

    public string ReadString(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        string str = ReadString();
        if (fieldList != null && index < fieldList.Count)
        {
            if (fieldList[index].define == TableDefine.BeTranslate)
            {
                if (CSVHelper.textFilter != null)
                {
                    str = CSVHelper.textFilter(str, tableName);
                }
            }
        }
        return str;
    }

    public List<bool> ReadListBool(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<bool> boolList = new List<bool>();
        for (int i = 0; i < length; i++)
        {
            boolList.Add(ReadToBoolean());
        }
        return boolList;
    }

    public List<byte> ReadListByte(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<byte> byteList = new List<byte>();
        for (int i = 0; i < length; i++)
        {
            byteList.Add(ReadToByte());
        }
        return byteList;
    }

    public List<short> ReadListShort(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<short> shortList = new List<short>();
        for (int i = 0; i < length; i++)
        {
            shortList.Add(ReadToInt16());
        }
        return shortList;
    }

    public void SetListCursorIndex(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
    }

    public List<int> ReadListInt(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<int> intList = new List<int>();
        for (int i = 0; i < length; i++)
        {
            intList.Add(ReadIntValue());
        }
        return intList;
    }

    public List<ulong> ReadListULong(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<ulong> ulongList = new List<ulong>();
        for (int i = 0; i < length; i++)
        {
            ulongList.Add(ReadULongValue());
        }
        return ulongList;
    }

    public List<string> ReadListStr(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<string> strList = new List<string>();
        for (int i = 0; i < length; i++)
        {
            strList.Add(ReadString());
        }
        return strList;
    }

    public List<float> ReadListFloat(int index)
    {
        SetCursorIndex();
        cursor = cursorIndex[index];
        int length = ReadListLength();
        List<float> floatList = new List<float>();
        for (int i = 0; i < length; i++)
        {
            floatList.Add(ReadToSingle());
        }
        return floatList;
    }
#endregion


#if TableLog
    public void Log(string str, params object[] args)
    {
        if (isLog)
        {
            Debug.LogErrorFormat(str,args);
        }
    }

    public void LogAllBytes()
    {
        Log("LogAllBytes {0}", GetStrByByteList(fieldData));
    }

    public static string GetStrByByteList(byte[] byteList)
    {
        string str = "";
        if (byteList != null)
        {
            for (int i = 0; i < byteList.Length; i++)
            {
                str += string.Format("{0}|", byteList[i]);
            }
            //return byteList[0].ToString();
        }
        return str;
    }

    public static string GetStrByByteList(List<byte> byteList)
    {
        string str = "";
        if (byteList != null)
        {
            for (int i = 0; i < byteList.Count; i++)
            {
                str += string.Format("{0}|", byteList[i]);
            }
            //return byteList[0].ToString();
        }
        return str;
    }
#endif
}
