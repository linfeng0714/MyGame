using GStore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ProjectBuilderSettings : ScriptableObject
{
    /// <summary>
    /// 模块根目录
    /// </summary>
    public const string kModuleRootFolder = "Assets/GStore/AssetManager/Editor";

    /// <summary>
    /// 配置文件名
    /// </summary>
    public const string kConfigAssetName = "ProjectBuilderSettings";

    /// <summary>
    /// 配置目录
    /// </summary>
    public static string ConfigFolder
    {
        get
        {
            return kModuleRootFolder + "/Configs";
        }
    }

    /// <summary>
    /// 输出目录
    /// </summary>
    public static string OutputFolder
    {
        get
        {
            return kModuleRootFolder + "/Output";
        }
    }

    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static string ConfigAssetPath
    {
        get
        {
            return ConfigFolder + "/" + kConfigAssetName + ".asset";
        }
    }

    /// <summary>
    /// Shader的AssetBundleName
    /// </summary>
    [HideInInspector]
    public string shaderAssetBundleName { get { return "shader" + AssetPathDefine.assetBundleExtension; } }

    /// <summary>
    /// 固定包配置相对路径
    /// </summary>
    [HideInInspector]
    public string fixedBundleTablePath = ConfigFolder + "/FixedBundleTable.json";

    ///// <summary>
    ///// 常驻内存的AB列表
    ///// </summary>
    //[HideInInspector]
    //public string residentBundleListPath = ConfigFolder + "/" + AssetPathDefine.residentBundleTableName;

    /// <summary>
    /// BundleTable
    /// </summary>
    [HideInInspector]
    public string bundleTableFile = OutputFolder + "/BundleTable/" + AssetPathDefine.bundleTableFileName;

    /// <summary>
    /// DepBundleTable
    /// </summary>
    [HideInInspector]
    public string depBundleTableFile = OutputFolder + "/BundleTable/" + AssetPathDefine.depBundleTableFileName;

    /// <summary>
    /// 丢失资源的文件记录
    /// </summary>
    [HideInInspector]
    public string missingAssetRecordFile = OutputFolder + "/Logs/MissingFiles.json";

    /// <summary>
    /// 文件引用记录
    /// </summary>
    [HideInInspector]
    public string referenceTableFile = OutputFolder + "/ReferenceTable.json";

    /// <summary>
    /// 共享AB包名
    /// </summary>
    [HideInInspector]
    public string sharedAssetBundleName = "sharedassets";

    /// <summary>
    /// 打包选项
    /// </summary>
    [HideInInspector]
    public int buildOptions = 0;

    #region 序列化

    /// <summary>
    /// 提交AssetBundleName - 测试功能，仅OSX下有效
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("m_CommitAssetBundleName")]
    [HideInInspector]
    public bool commitAssetBundleName = true;

    /// <summary>
    /// 默认变体名
    /// </summary>
    [Header("默认变体后缀名")]
    [SerializeField]
    [FormerlySerializedAs("m_DefaultVariantName")]
    public string defaultVariantName = ".normal";

    /// <summary>
    /// 合并依赖AB
    /// </summary>
    [Header("合并依赖包")]
    [SerializeField]
    [FormerlySerializedAs("m_CombineDependenceAssetBundle")]
    public bool combineDependenceAssetBundle = true;

    /// <summary>
    /// 粒度限制(KB)
    /// </summary>
    [Header("粒度限制(KB)")]
    [SerializeField]
    [FormerlySerializedAs("m_FinenessLimit")]
    public int finenessLimit = 1024;

    #endregion

    /// <summary>
    /// 单例
    /// </summary>
    private static ProjectBuilderSettings s_Instance;
    public static ProjectBuilderSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<ProjectBuilderSettings>(ConfigAssetPath);
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
    public static ProjectBuilderSettings Create()
    {
        ProjectBuilderSettings settings = CreateInstance<ProjectBuilderSettings>();

        Directory.CreateDirectory(ConfigFolder);
        AssetDatabase.CreateAsset(settings, ConfigAssetPath);

        return settings;
    }
}
