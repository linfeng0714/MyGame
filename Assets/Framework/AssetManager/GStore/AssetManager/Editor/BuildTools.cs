using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using GStore;



/// <summary>
/// 打包工具 - 包含了基本的打包流程。
/// 如果要添加步骤，可以使用ProjcetBuildMethodAttribute在BuildToolsSetup或其它Editor代码中添加，
/// 尽量不要直接修改此文件，方便以后升级模块可以直接替换。
/// </summary>
[InitializeOnLoad]
public static class BuildTools
{
    public delegate bool CopyDataBeforeBuild();
    public static CopyDataBeforeBuild copyDataBeforeBuild;

    [MenuItem("GStore/Asset/一键打包")]
    public static void BuildPlayerProcessor()
    {
        var methods = ProjectBuildMethod.List;

        foreach (var method in methods)
        {
            if (method.required)
            {
                method.func();
            }
        }
    }

    /// <summary>
    /// 打包AssetBundle - 提供给云构建的接口
    /// </summary>
    public static void BuildAssetBundles()
    {
        AssetBundleBuildTools.MarkAssets();
        AssetBundleBuildTools.BuildAssets();
    }

    /// <summary>
    /// 处理打包前的资源拷贝 - 提供给云构建的接口
    /// </summary>
    public static void CopyResourcesBeforeBuild()
    {
        //导入Bundle
        AssetBundleBuildTools.ImportAssets();

        //打包数据
        foreach (var method in ProjectBuildMethod.List)
        {
            if (method.order / 10 == 4 && method.required)
            {
                method.func();
            }
        }

        //打包前处理
        OnPreProcessBuild();
    }

    /// <summary>
    /// 打包前的处理
    /// </summary>
    public static void OnPreProcessBuild()
    {
        SetEnableAssetBundleScenes(false);

        StreamingAssets.Build();
    }

    /// <summary>
    /// 设置AB场景是否启用
    /// </summary>
    /// <param name="enable"></param>
    private static void SetEnableAssetBundleScenes(bool enable)
    {
        List<EditorBuildSettingsScene> buildSceneList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (var buildScene in buildSceneList)
        {
            if (buildScene.enabled == enable)
            {
                continue;
            }
            string assetPath = buildScene.path;
            if (File.Exists(assetPath) == false)
            {
                continue;
            }

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (string.IsNullOrEmpty(importer.assetBundleName))
            {
                continue;
            }
            buildScene.enabled = enable;
        }

        EditorBuildSettings.scenes = buildSceneList.ToArray();
    }

    static BuildTools()
    {
        GameSettingSetup.Setup();
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SetEnableAssetBundleScenes(true);
        }
    }

    /// <summary>
    /// 打包数据文件
    /// </summary>
    /// <returns></returns>
    [ProjcetBuildMethod(40, "打包内置数据")]
    private static bool PackBuiltInData()
    {
        if(copyDataBeforeBuild != null)
        {
            if(!copyDataBeforeBuild())
                return false;
        }
        
        //读取配置
        var builtInDataFiles = BuiltInDataSettings.Instance.paths;
        //复制固定数据文件Resources目录
        foreach (string assetPath in builtInDataFiles)
        {
            if (File.Exists(assetPath) == false)
            {
                Debug.LogWarningFormat("固定数据文件不存在！assetPath={0}", assetPath);
                continue;
            }

            string resourcesPath = assetPath.Replace(AssetPathDefine.resFolder, "Assets/Resources/");
            string directory = Path.GetDirectoryName(resourcesPath);
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(assetPath, resourcesPath, true);
        }

        return true;
    }

    /// <summary>
    /// 打包
    /// </summary>
    [ProjcetBuildMethod(50, "输出包体")]
    public static bool BuildPlayer()
    {
        OnPreProcessBuild();

        return true;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        string _output_path = GetTargetPlayerOuputPath(EditorUserBuildSettings.activeBuildTarget);

        string _directory = Path.GetDirectoryName(_output_path);
        if (Directory.Exists(_directory) == false)
        {
            Directory.CreateDirectory(_directory);
        }

        BuildOptions _options = BuildOptions.None;

        if (EditorUserBuildSettings.development == true)
        {
            _options |= BuildOptions.Development;
        }
        if (EditorUserBuildSettings.connectProfiler == true)
        {
            _options |= BuildOptions.ConnectWithProfiler;
        }
        if (EditorUserBuildSettings.allowDebugging == true)
        {
            _options |= BuildOptions.AllowDebugging;
        }

        var buildReport = BuildPipeline.BuildPlayer(EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes), _output_path, EditorUserBuildSettings.activeBuildTarget, _options);

#if UNITY_2018_4_OR_NEWER
        //打包过程中有错误信息
        if (buildReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError("Error MSG : " + buildReport.summary.result);
        }
#else
        //打包过程中有错误信息
        if (string.IsNullOrEmpty(buildReport) == false)
        {
            UnityEngine.Debug.LogError("Error MSG : " + buildReport);
        }
#endif

        sw.Stop();
        Debug.LogWarningFormat("BuildPlayer {0}, Cost Time:{1}", EditorUserBuildSettings.activeBuildTarget, sw.Elapsed.ToString());

        //打开所在目录
        EditorUtility.OpenWithDefaultApp(_directory);
        return true;
    }

    private static string GetTargetPlayerOuputPath(BuildTarget _target)
    {
        string _unity_project_path = Path.GetFullPath(Application.dataPath + "/../"); //项目路径
        string _output_path = _unity_project_path + "ExtraResources/Builds/" + _target + GetTargetPlayerName(_target);
        return _output_path;
    }

    private static string GetTargetPlayerName(BuildTarget _target)
    {
        switch (_target)
        {
            case BuildTarget.Android:
                return "/" + Application.productName + ".apk";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "/" + Application.productName + ".exe";
            default:
                return "";
        }
    }
}