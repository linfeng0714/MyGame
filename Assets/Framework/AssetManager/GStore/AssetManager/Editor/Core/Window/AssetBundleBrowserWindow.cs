using UnityEngine;
using UnityEditor;
using System.IO;
using GStore;
using UnityEditor.IMGUI.Controls;
using AssetBundleBrowser;
using System.Collections.Generic;

public class AssetBundleBrowserWindow : SubWindow
{
    private EditorWindow m_Window;

    [SerializeField]
    internal AssetBundleManageTab m_ManageTab;

    [SerializeField]
    int m_DataSourceIndex;

    [SerializeField]
    internal bool multiDataSource = false;
    List<AssetBundleBrowser.AssetBundleDataSource.ABDataSource> m_DataSourceList = null;

    public AssetBundleBrowserWindow(EditorWindow window)
    {
        m_Window = window;
    }

    public override string GetTitle()
    {
        return "AssetBundle浏览器";
    }

    public override void OnUpdate()
    {
        if (m_ManageTab != null)
        {
            m_ManageTab.Update();
        }
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginArea(rect);

        if (m_ManageTab == null)
        {
            m_ManageTab = new AssetBundleManageTab();
            m_ManageTab.OnEnable(rect, m_Window);
            InitDataSources();
        }
        m_ManageTab.OnGUI(rect);

        GUILayout.EndArea();
    }

    private void InitDataSources()
    {
        //determine if we are "multi source" or not...
        multiDataSource = false;
        m_DataSourceList = new List<AssetBundleBrowser.AssetBundleDataSource.ABDataSource>();
        foreach (var info in AssetBundleBrowser.AssetBundleDataSource.ABDataSourceProviderUtility.CustomABDataSourceTypes)
        {
            m_DataSourceList.AddRange(info.GetMethod("CreateDataSources").Invoke(null, null) as List<AssetBundleBrowser.AssetBundleDataSource.ABDataSource>);
        }

        if (m_DataSourceList.Count > 1)
        {
            multiDataSource = true;
            if (m_DataSourceIndex >= m_DataSourceList.Count)
                m_DataSourceIndex = 0;
            AssetBundleBrowser.AssetBundleModel.Model.DataSource = m_DataSourceList[m_DataSourceIndex];
        }
    }
}
