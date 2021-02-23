using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AlwaysPackedSettings : ScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// 配置文件名
    /// </summary>
    public const string kConfigAssetName = "AlwaysPackedSettings";

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
    private static AlwaysPackedSettings s_Instance;
    public static AlwaysPackedSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<AlwaysPackedSettings>(ConfigAssetPath);
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
    public static AlwaysPackedSettings Create()
    {
        AlwaysPackedSettings settings = CreateInstance<AlwaysPackedSettings>();

        Directory.CreateDirectory(ProjectBuilderSettings.ConfigFolder);
        AssetDatabase.CreateAsset(settings, ConfigAssetPath);

        return settings;
    }

    #region 序列化
    [SerializeField]
    [HideInInspector]
    private List<string> m_Bundles = new List<string>();

    public HashSet<string> bundles = new HashSet<string>();

    public void OnBeforeSerialize()
    {
        m_Bundles = new List<string>(bundles);
    }

    public void OnAfterDeserialize()
    {
        bundles = new HashSet<string>(m_Bundles);
    }

    #endregion

    [MenuItem("GStore/Asset/小包快照 %Q")]
    public static void CaptureLoadedAssetBundle()
    {
        //Debug.Log("---");
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "请运行游戏", "确定");
            return;
        }
        Instance.bundles.Clear();
        foreach (var assetBundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            Instance.bundles.Add(GStore.VariantMapper.GetBundleNameWithoutVariant(assetBundle.name));
        }
        EditorUtility.SetDirty(Instance);
    }

    public bool Contains(string bundleName)
    {
        return bundles.Contains(bundleName);
    }

    public void Set(string bundleName, bool pack)
    {
        if (pack ? bundles.Add(bundleName) : bundles.Remove(bundleName))
        {
            EditorUtility.SetDirty(this);
        }
    }
}
