using UnityEngine;
using UnityEditor;
using System.IO;
using GStore;
using UnityEditor.IMGUI.Controls;

public class AssetBundleInsideTableWindow : SubWindow
{
    private TreeViewState m_TreeState;
    private TreeView m_TreeView;

    public BuiltInDataSettings Settings
    {
        get
        {
            return BuiltInDataSettings.Instance;
        }
    }

    public override string GetTitle()
    {
        return "内置表配置";
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginArea(rect);

        if (m_TreeState == null)
            m_TreeState = new TreeViewState();

        if (m_TreeView == null)
        {
            m_TreeView = new BuiltInDataTreeView(m_TreeState, this);
            m_TreeView.Reload();
        }
        Rect treeRect = new Rect(0, 0, rect.width, rect.height);
        m_TreeView.OnGUI(treeRect);

        GUILayout.EndArea();
    }
}
