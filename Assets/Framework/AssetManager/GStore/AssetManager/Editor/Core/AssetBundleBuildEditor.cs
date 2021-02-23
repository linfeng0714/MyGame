/*打包工具说明
黄晓文 2018.5

打包步骤(2.0):
    1.读入固定包配置。
    2.读出Asset表所有的资源。
    3.读出每个Asset表记录的资源的所有依赖作为待选资源,并记录引用数。
    4.设置固定包的AB名字。
    5.设置Asset表记录的资源的AB名字。
    6.被引用大于1的资源如果有在资源表记录则跳过，没有则设置AB名字。
    7.被引用小于等于1的资源清除AB名字。
    8.粒度控制，合并依赖包。
    9.清除多余的ABName。
    10.输出AB。
    11.清理输出目录。

打包工具版本:
2018.5.23
2.0:
添加：初始完成

2018.6.29
3.0：
添加：打包场景特殊处理
添加：分析直接依赖，相比递归依赖，粒度更大，依赖包数量少，依赖关系更合理。
添加：合并依赖包，粒度控制。
添加：整合AB浏览器。 ---by邓贤福

2018.7.2
移除：选择配置保存路径功能。
优化：重新导入shader的实现方式。
添加：预估打包后AB大小，粒度控制值更接近打包后的数据。
bugfix：避免循环依赖造成异常。
bugfix：编辑器界面无法修改粒度控制值。

2018.7.3
添加：资源查看器排序、保存常驻内存、小包配表保存。  
修改：将之前老的xml配置表统一改成json格式             --By Flamesky

2018.7.5
添加：资源查看器增加打包前包体大小统计列 
fix ：资源查看器搜索时候显示包体大小异常              --By Flamesky

2018.12.4
3.2：
添加：AssetBundle变体支持
修改：BundleTable改用ScriptObject存储

2019.12.26
3.3:
优化：配置信息由json改为ScriptableObject
优化：界面简化
优化：利用ReferenceCache优化依赖分析速度
新增：资源引用计数管理
    新增：GameObject引用计数器
    新增：Asset引用计数器
    新增：AssetBundle引用计数器
    新增：切场景自动释放引用的策略
新增：ReferenceFinder资源引用查看器
新增：导入资源时自动根据asset表配置abName
删除：常驻AB列表，目前统一使用引用计数管理。

2020.1.6
3.4:
优化：支持使用Texture[SpriteName]格式加载子Sprite
优化：支持通过asset表添加文件夹来定义固定包
优化：assetPath忽略大小写错误
修改：切换场景时正确卸载场景Bundle
修改：正确处理Bundle LoadAll 和Asset LoadAll的区别
新增：延时释放Asset引用的机制，避免短时间内反复加载

*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class AssetBundleBuildEditor : EditorWindow
{
    public const string ver = "3.4(2020.1.6)";

    public const int Header = 20;

    private List<SubWindow> m_SubWindows = new List<SubWindow>();
    private string[] m_SubWindowTitles;
    private int m_ToolbarOption = 0;

    [MenuItem("GStore/Asset/Build")]
    static void ShowWindow()
    {
        var window = GetWindow<AssetBundleBuildEditor>();
        window.titleContent = new GUIContent("Build");
        window.minSize = new Vector2(400, 360);
        window.Show();
    }

    void OnEnable()
    {
        m_SubWindows.Add(new AssetBundleSettingWindow());
        m_SubWindows.Add(new AssetBundleFixedBundleWindow());
        m_SubWindows.Add(new AssetBundleInsideTableWindow());
        m_SubWindows.Add(new AssetBundleBrowserWindow(this));

        m_SubWindowTitles = new string[m_SubWindows.Count];
        for (int i = 0; i < m_SubWindowTitles.Length; i++)
        {
            m_SubWindowTitles[i] = m_SubWindows[i].GetTitle();
        }

        GameSettingSetup.Setup();
    }

    private void Update()
    {
        m_SubWindows[m_ToolbarOption].OnUpdate();
    }

    [NonSerialized]
    private GUIStyle m_ButtonStyle;
    void OnGUI()
    {
        if (m_ButtonStyle == null)
            m_ButtonStyle = GetStyle("ToolbarButton");

        GUILayout.BeginArea(new Rect(0, 0, position.width, Header));
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            m_ToolbarOption = GUILayout.Toolbar(m_ToolbarOption, m_SubWindowTitles, m_ButtonStyle, GUILayout.ExpandWidth(false));
            GUILayout.Label("");
            GUILayout.Label(string.Format("Ver={0}", ver), GUILayout.ExpandWidth(false));
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        Rect windowRect = new Rect(0, Header, position.width, position.height - Header);

        m_SubWindows[m_ToolbarOption].OnGUI(windowRect);
    }

    GUIStyle GetStyle(string styleName)
    {
        GUIStyle s = UnityEngine.GUI.skin.FindStyle(styleName);
        if (s == null)
            s = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        if (s == null)
        {
            Debug.LogError("Missing built-in guistyle " + styleName);
            s = new GUIStyle();
        }
        return s;
    }
}