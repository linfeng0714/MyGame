using UnityEngine;
using UnityEditor;
using System.IO;
using GStore;
using UnityEditor.IMGUI.Controls;

public class AssetBundleFixedBundleWindow : SubWindow
{
    private TreeViewState m_TreeState;
    private FixedGroupTreeView m_TreeView;
    private MultiColumnHeaderState m_Mchs;


    public FixedGroupSettings Settings
    {
        get
        {
            return FixedGroupSettings.Instance;
        }
    }

    public override string GetTitle()
    {
        return "固定包配置";
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginArea(rect);


        if (m_TreeState == null)
            m_TreeState = new TreeViewState();

        var headerState = FixedGroupTreeView.CreateDefaultMultiColumnHeaderState();
        if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_Mchs, headerState))
            MultiColumnHeaderState.OverwriteSerializedFields(m_Mchs, headerState);
        m_Mchs = headerState;

        if (m_TreeView == null)
        {
            m_TreeView = new FixedGroupTreeView(m_TreeState, m_Mchs, this);
            m_TreeView.Reload();
        }
        Rect treeRect = new Rect(0, 0, rect.width, rect.height);
        m_TreeView.OnGUI(treeRect);

        GUILayout.EndArea();
    }

    private bool IsFolderExists(string folder)
    {
        return Directory.Exists(AssetPathDefine.resFolder + folder);
    }
}
