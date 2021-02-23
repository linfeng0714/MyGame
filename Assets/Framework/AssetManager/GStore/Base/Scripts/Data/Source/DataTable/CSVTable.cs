using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CSVTable
{
    private Dictionary<ulong, CSVBytesData> csvBytesDataDic = new Dictionary<ulong, CSVBytesData>();
    private List<TableField> fieldList = new List<TableField>();

    public void Init(Dictionary<ulong, CSVBytesData> csvBytesDataDic, List<TableField> fieldList)
    {
        this.csvBytesDataDic = csvBytesDataDic;
        this.fieldList = fieldList;
    }

    public virtual void UnLoad(bool isRemove = true)
    {
        csvBytesDataDic.Clear();
        fieldList.Clear();
    }

    public CSVBytesData GetByKey(int key1)
    {
        ulong key = CSVHelper.GetKey(key1);
        CSVBytesData csvBytesData = GetCSVBytesData(key);
        return csvBytesData;
    }

    public CSVBytesData GetByKey(int key1, int key2)
    {
        ulong key = CSVHelper.GetKey(key1,key2);
        CSVBytesData csvBytesData = GetCSVBytesData(key);
        return csvBytesData;
    }

    public CSVBytesData GetByKey(int key1, int key2, int key3)
    {
        ulong key = CSVHelper.GetKey(key1, key2, key3);
        CSVBytesData csvBytesData = GetCSVBytesData(key);
        return csvBytesData;
    }

    public CSVBytesData GetByKey(int key1, int key2, int key3, int key4)
    {
        ulong key = CSVHelper.GetKey(key1, key2, key3, key4);
        CSVBytesData csvBytesData = GetCSVBytesData(key);
        return csvBytesData;
    }

    public CSVBytesData GetByKey(int key1, int key2, int key3, int key4, int key5)
    {
        ulong key = CSVHelper.GetKey(key1, key2, key3, key4, key5);
        CSVBytesData csvBytesData = GetCSVBytesData(key);
        return csvBytesData;
    }

    public CSVBytesData GetCSVBytesData(ulong key)
    {
        CSVBytesData csvBytesData;
        if (csvBytesDataDic.TryGetValue(key, out csvBytesData))
        {
#if TableLog
            csvBytesData.LogAllBytes();
#endif
            return csvBytesData;
        }
        else
        {
            return null;
        }
    }

    public Dictionary<ulong, CSVBytesData> GetAllCSVBytesData()
    {
        return csvBytesDataDic;
    }
}
