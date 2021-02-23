using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using Object = UnityEngine.Object;
using UnityEditor;
using System.IO;
using GStore;

public class BuiltInDataTreeView : TreeView
{
    private AssetBundleInsideTableWindow m_Window;

    internal BuiltInDataTreeView(TreeViewState state, AssetBundleInsideTableWindow settings)
     : base(state)
    {
        this.m_Window = settings;
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem(-1, -1);
        foreach (var path in m_Window.Settings.paths)
        {
            var item = new TreeViewItem(path.GetHashCode(), 0, path);
            item.icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
            root.AddChild(item);
        }
        return root;
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        return base.BuildRows(root);
    }

    protected override void KeyEvent()
    {
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
        {
            List<TreeViewItem> selectedNodes = new List<TreeViewItem>();
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId);
                if (item != null)
                {
                    selectedNodes.Add(item);
                }
            }

            RemoveGroup(selectedNodes);
        }
    }

    protected void RemoveGroup(object context)
    {
        if (EditorUtility.DisplayDialog("确认删除?", "是否要删除选中的内置数据配置", "是", "否"))
        {
            List<TreeViewItem> selectedNodes = context as List<TreeViewItem>;
            if (selectedNodes == null)
                return;
            foreach (var item in selectedNodes)
            {
                m_Window.Settings.paths.Remove(item.displayName);
            }
            EditorUtility.SetDirty(m_Window.Settings);
            Reload();
        }
    }

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        if (selectedIds.Count != 1)
        {
            return;
        }
        int id = selectedIds[0];
        SingleClickedItem(id);
    }

    protected void SingleClickedItem(int id)
    {
        var item = FindItemInVisibleRows(id);
        if (item != null)
        {
            Object o = null;
            if (item.displayName != null)
                o = AssetDatabase.LoadAssetAtPath<Object>(item.displayName);

            if (o != null)
            {
                EditorGUIUtility.PingObject(o);
            }
        }
    }

    protected override void DoubleClickedItem(int id)
    {
        var item = FindItemInVisibleRows(id);
        if (item != null)
        {
            Object o = null;
            if (item.displayName != null)
                o = AssetDatabase.LoadAssetAtPath<Object>(item.displayName);

            if (o != null)
            {
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }
    }

    private List<string> m_SelectPaths = new List<string>();
    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

        m_SelectPaths.Clear();
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            foreach (var path in DragAndDrop.paths)
            {
                if (IsPathValid(path))
                {
                    m_SelectPaths.Add(path);
                }
            }
            if (m_SelectPaths.Count > 0)
            {
                visualMode = DragAndDropVisualMode.Copy;
            }
            else
            {
                visualMode = DragAndDropVisualMode.Rejected;
            }
        }

        if (args.performDrop && visualMode != DragAndDropVisualMode.Rejected)
        {
            foreach (var path in m_SelectPaths)
            {
                m_Window.Settings.paths.Add(path);
            }
            EditorUtility.SetDirty(m_Window.Settings);
            Reload();
        }

        return visualMode;
    }

    private bool IsPathValid(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        if (File.Exists(path) == false)
        {
            return false;
        }
        if (path.StartsWith(AssetPathDefine.resFolder) == false)
        {
            return false;
        }
        if (IsInResources(path))
        {
            return false;
        }

        return true;
    }

    internal static bool IsInResources(string path)
    {
        return path.Replace('\\', '/').ToLower().Contains("/resources/");
    }

    TreeViewItem FindItemInVisibleRows(int id)
    {
        var rows = GetRows();
        foreach (var r in rows)
        {
            if (r.id == id)
            {
                return r;
            }
        }
        return null;
    }
}
