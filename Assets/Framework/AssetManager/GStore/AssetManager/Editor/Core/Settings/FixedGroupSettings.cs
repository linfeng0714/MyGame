using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

[Serializable]
public class FixedGroupSettings : ScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// 配置文件名
    /// </summary>
    public const string kConfigAssetName = "FixedGroupSettings";

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static string ConfigAssetPath
    {
        get
        {
            return ProjectBuilderSettings.ConfigFolder + "/" + kConfigAssetName + ".asset";
        }
    }

    /// <summary>
    /// 单例
    /// </summary>
    private static FixedGroupSettings s_Instance;
    public static FixedGroupSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<FixedGroupSettings>(ConfigAssetPath);
                if (s_Instance == null)
                {
                    s_Instance = Create();
                }
            }

            return s_Instance;
        }
    }

    /// <summary>
    /// 创建配置
    /// </summary>
    /// <returns></returns>
    public static FixedGroupSettings Create()
    {
        FixedGroupSettings settings = CreateInstance<FixedGroupSettings>();

        Directory.CreateDirectory(ProjectBuilderSettings.ConfigFolder);
        AssetDatabase.CreateAsset(settings, ConfigAssetPath);

        return settings;
    }

    public Dictionary<string, FixedGroup> GetAllGroups()
    {
        Dictionary<string, FixedGroup> allGroups = new Dictionary<string, FixedGroup>(StringComparer.OrdinalIgnoreCase);
        //添加固定包配置
        foreach (var kvp in groups)
        {
            allGroups[kvp.Value.folder] = kvp.Value;
        }
        //添加asset表中的文件夹
        foreach (var kvp in GStore.AssetTable.FolderMap)
        {
            var group = new FixedGroup() { folder = kvp.Value.assetPath };
            allGroups[group.folder] = group;
        }
        return allGroups;
    }

    #region 序列化
    [SerializeField]
    [HideInInspector]
    private List<FixedGroup> m_Groups;
    public Dictionary<string, FixedGroup> groups = new Dictionary<string, FixedGroup>(StringComparer.OrdinalIgnoreCase);

    public void OnBeforeSerialize()
    {
        m_Groups = new List<FixedGroup>(groups.Values);
    }

    public void OnAfterDeserialize()
    {
        groups = m_Groups.ToDictionary(group => group.folder);
    }
    #endregion
}
