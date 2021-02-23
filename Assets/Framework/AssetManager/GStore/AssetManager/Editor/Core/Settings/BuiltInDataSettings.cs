using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuiltInDataSettings : ScriptableObject
{
    /// <summary>
    /// 配置文件名
    /// </summary>
    public const string kConfigAssetName = "BuiltInDataSettings";

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
    private static BuiltInDataSettings s_Instance;
    public static BuiltInDataSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<BuiltInDataSettings>(ConfigAssetPath);
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
    public static BuiltInDataSettings Create()
    {
        BuiltInDataSettings settings = CreateInstance<BuiltInDataSettings>();

        Directory.CreateDirectory(ProjectBuilderSettings.ConfigFolder);
        AssetDatabase.CreateAsset(settings, ConfigAssetPath);

        return settings;
    }



    #region 序列化
    [SerializeField]
    [HideInInspector]
    public List<string> paths = new List<string>();
    #endregion
}
