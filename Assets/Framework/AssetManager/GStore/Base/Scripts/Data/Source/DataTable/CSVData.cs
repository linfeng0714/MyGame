using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CSVData
{
    /// <summary>
    /// 表的所有行的二进制数据，注意只有是单例时，cvsTable才有值，否则是null
    /// </summary>
    public CSVTable csvTable;

    /// <summary>
    /// 表中某一行的二进制数据，注意只有是非单例时才有数据，否则是null
    /// </summary>
    public CSVBytesData bytesData;

    public void LoadCSVTable()
    {
        if (csvTable == null)
        {
            csvTable = CSVManager.Instance.GetCSVTable(Name());
            CSVManager.Instance.AddCSVData(Name(),this);
        }
    }

    public virtual void UnLoadData(bool isRemove = true)
    {
        if (csvTable != null)
        {
            csvTable.UnLoad();
            csvTable = null;
        }
    }

    public CSVBytesData GetCSVBytesData(ulong key)
    {
        return csvTable.GetCSVBytesData(key);
    }

    protected virtual string Name()
    {
        return "";
    }

    public Dictionary<ulong, CSVBytesData> GetAllCSVBytesData()
    {
        return csvTable.GetAllCSVBytesData();
    }

    #region Lua访问接口
    public bool ReadBool(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadBool(index);
        }
        return false;
    }

    public byte ReadByte(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadByte(index);
        }
        return 0;
    }

    public short ReadShort(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadShort(index);
        }
        return 0;
    }

    public int ReadInt(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadInt(index);
        }
        return 0;
    }

    public ulong ReadULong(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadULong(index);
        }
        return 0;
    }

    public float ReadFloat(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadFloat(index);
        }
        return 0;
    }

    public string ReadString(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadString(index);
        }
        return "";
    }

    public List<bool> ReadListBool(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListBool(index);
        }
        return null;
    }

    public List<byte> ReadListByte(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListByte(index);
        }
        return null;
    }

    public List<short> ReadListShort(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListShort(index);
        }
        return null;
    }

    public List<int> ReadListInt(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListInt(index);
        }
        return null;
    }

    public List<ulong> ReadListULong(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListULong(index);
        }
        return null;
    }

    public List<string> ReadListStr(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListStr(index);
        }
        return null;
    }

    public List<float> ReadListFloat(int index)
    {
        if (bytesData != null)
        {
            return bytesData.ReadListFloat(index);
        }
        return null;
    }
    #endregion
}
