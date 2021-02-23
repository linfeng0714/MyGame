using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LayerGenerator
{
    public const int customLayerBeginIndex = 8;

    public static void SetLayers(string[] customLayers)
    {
        List<string> list = GetList(customLayers);
        if (list.Count == 0)
        {
            return;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty it = tagManager.GetIterator();
        while (it.NextVisible(true))
        {
            if (it.name == "layers")
            {
                for (int i = customLayerBeginIndex; i < it.arraySize; i++)
                {
                    SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(dataPoint.stringValue))
                    {
                        dataPoint.stringValue = list[0];
                        list.RemoveAt(0);
                        if (list.Count <= 0)
                        {
                            tagManager.ApplyModifiedProperties();
                            return;
                        }
                    }
                }

                tagManager.ApplyModifiedProperties();
                if (list.Count > 0)
                {
                    Debug.LogFormat("<color=red>Layer不能超过31</color>");
                }
                break;
            }
        }
    }

    public static List<string> GetList(string[] customLayers)
    {
        List<string> list = new List<string>();
        foreach (string layer in customLayers)
        {
            if (IsHasLayer(layer) == false)
            {
                list.Add(layer);
            }
        }
        return list;
    }

    public static bool IsHasLayer(string layer)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
                return true;
        }
        return false;
    }

    public static void GenerateGameLayer()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty it = tagManager.GetIterator();
        while (it.NextVisible(true))
        {
            if (it.name == "layers")
            {
                StringWriter sw = new StringWriter();
                sw.WriteLine(@"/*
 * 自动生成代码
 * 请勿编辑
 * 详情可查看 LayerGenerator.cs
 */");
                sw.WriteLine("namespace GStore");
                sw.WriteLine("{");
                sw.WriteLine("\tpublic partial class LayerDefine");
                sw.WriteLine("\t{");

                sw.WriteLine("\t\t#region Unity Default Layer");
                for (int i = 0; i < 8; i++)
                {
                    SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                    string fieldname = string.IsNullOrEmpty(dataPoint.stringValue) ? "Layer" + i : dataPoint.stringValue.Replace(" ", "_");
                    sw.WriteLine(string.Format("\t\tpublic const int {0}\t\t\t=\t{1};", fieldname, i));

                }
                sw.WriteLine("\t\t#endregion");
                sw.WriteLine("\r\n");

                sw.WriteLine("\t\t#region Custom Layer");
                for (int i = 8; i < it.arraySize; i++)
                {
                    SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                    string fieldname = string.IsNullOrEmpty(dataPoint.stringValue) ? "Layer" + i : dataPoint.stringValue.Replace(" ", "_");
                    sw.WriteLine(string.Format("\t\tpublic const int {0}\t\t\t=\t{1};", fieldname, i));
                }
                sw.WriteLine("\t\t#endregion");

                sw.WriteLine("\t}");
                sw.WriteLine("}");
                File.WriteAllText(Application.dataPath + "/GStore-Custom/Base/LayerDefine.cs", sw.ToString(), System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();
                break;
            }
        }
    }
}
