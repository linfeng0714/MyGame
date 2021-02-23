using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

public class FixedGroupTreeView : TreeView
{
    private AssetBundleFixedBundleWindow m_Window;
    private List<string> m_SelectFolders = new List<string>();

    enum ColumnId
    {
        Path,
        Type,
        Separately,
    }

    internal FixedGroupTreeView(TreeViewState state, MultiColumnHeaderState mchs, AssetBundleFixedBundleWindow settings)
            : base(state, new MultiColumnHeader(mchs))
    {
        this.m_Window = settings;
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem(-1, -1);
        foreach (var kvp in m_Window.Settings.GetAllGroups())
            AddGroupChildrenBuild(kvp.Value, root);

        return root;
    }

    void AddGroupChildrenBuild(FixedGroup group, TreeViewItem root)
    {
        var groupItem = new FixedAssetTreeViewItem(group, 0);
        root.AddChild(groupItem);
        if (group.Entries.Count > 0)
        {
            foreach (var entry in group.Entries)
            {
                var item = new FixedAssetTreeViewItem(entry, 1);
                groupItem.AddChild(item);
            }
        }
    }

    protected override void BeforeRowsGUI()
    {
        base.BeforeRowsGUI();
    }

    GUIStyle m_LabelStyle;
    GUIStyle m_LabelStyleRed;
    protected override void RowGUI(RowGUIArgs args)
    {
        if (m_LabelStyle == null)
        {
            m_LabelStyle = new GUIStyle("PR Label");
            if (m_LabelStyle == null)
                m_LabelStyle = UnityEngine.GUI.skin.GetStyle("Label");
            m_LabelStyleRed = new GUIStyle(m_LabelStyle);
            m_LabelStyleRed.normal.textColor = Color.red;
            m_LabelStyleRed.active.textColor = Color.red;
            m_LabelStyleRed.focused.textColor = Color.red;
            m_LabelStyleRed.hover.textColor = Color.red;
        }

        var item = args.item as FixedAssetTreeViewItem;
        if (item == null)
        {
            base.RowGUI(args);
        }
        else
        {
            using (new EditorGUI.DisabledScope(item.ReadOnly))
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
            }
        }

        //base.RowGUI(args);
    }

    private void CellGUI(Rect cellRect, FixedAssetTreeViewItem item, int column, ref RowGUIArgs args)
    {
        CenterRectUsingSingleLineHeight(ref cellRect);
        switch ((ColumnId)column)
        {
            case ColumnId.Path:
                if (Event.current.type == EventType.Repaint)
                {
                    bool valid = true;
                    if (item.group != null)
                    {
                        if (IsPathValidForGroup(item.group.folder) == false)
                        {
                            valid = false;
                        }
                    }

                    GUIStyle style = valid ? m_LabelStyle : m_LabelStyleRed;
                    style.Draw(cellRect, item.displayName, false, false, args.selected, args.focused);
                }
                break;
            case ColumnId.Type:
                if (item.assetIcon != null)
                    UnityEngine.GUI.DrawTexture(cellRect, item.assetIcon, ScaleMode.ScaleToFit, true);
                break;
            case ColumnId.Separately:
                if (item.group != null)
                {
                    item.group.separately = EditorGUI.Toggle(cellRect, item.group.separately);
                }
                break;
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
            if (item.entry != null)
                o = AssetDatabase.LoadAssetAtPath<Object>(item.entry);
            if (item.group != null)
            {
                o = AssetDatabase.LoadAssetAtPath<Object>(item.group.folder);
            }

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
            if (item.entry != null)
                o = AssetDatabase.LoadAssetAtPath<Object>(item.entry);
            if (item.group != null)
            {
                o = AssetDatabase.LoadAssetAtPath<Object>(item.group.folder);
            }

            if (o != null)
            {
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }
    }

    protected override void KeyEvent()
    {
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
        {
            List<FixedAssetTreeViewItem> selectedNodes = new List<FixedAssetTreeViewItem>();
            bool allGroups = true;
            bool allEntries = true;
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId);
                if (item != null)
                {
                    selectedNodes.Add(item);
                    if (item.entry == null)
                        allEntries = false;
                    else
                        allGroups = false;
                }
            }
            if (allEntries)
            {
                //RemoveEntry(selectedNodes);
            }
            if (allGroups)
            {
                RemoveGroup(selectedNodes);
            }
        }
    }

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

        m_SelectFolders.Clear();
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            foreach (var path in DragAndDrop.paths)
            {
                if (IsPathValidForGroup(path))
                {
                    m_SelectFolders.Add(path);
                }
            }
            if (m_SelectFolders.Count > 0)
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
            foreach (var folder in m_SelectFolders)
            {
                if (m_Window.Settings.groups.ContainsKey(folder) == false && GStore.AssetTable.FolderMap.ContainsKey(folder) == false)
                {
                    m_Window.Settings.groups.Add(folder, new FixedGroup() { folder = folder });
                }
            }
            EditorUtility.SetDirty(m_Window.Settings);
            Reload();
        }

        return visualMode;
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        return base.BuildRows(root);
    }

    protected void RemoveGroup(object context)
    {
        if (EditorUtility.DisplayDialog("确认删除?", "是否要删除选中的固定组配置", "是", "否"))
        {
            List<FixedAssetTreeViewItem> selectedNodes = context as List<FixedAssetTreeViewItem>;
            if (selectedNodes == null)
                return;
            foreach (var item in selectedNodes)
            {
                m_Window.Settings.groups.Remove(item.group.folder);
            }
            EditorUtility.SetDirty(m_Window.Settings);
            Reload();
        }
    }

    FixedAssetTreeViewItem FindItemInVisibleRows(int id)
    {
        var rows = GetRows();
        foreach (var r in rows)
        {
            if (r.id == id)
            {
                return r as FixedAssetTreeViewItem;
            }
        }
        return null;
    }

    public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
    {
        return new MultiColumnHeaderState(GetColumns());
    }

    static MultiColumnHeaderState.Column[] GetColumns()
    {
        var retVal = new[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                //new MultiColumnHeaderState.Column()
                //new MultiColumnHeaderState.Column(),
            };

        int counter = 0;

        retVal[counter].headerContent = new GUIContent("Path", "Current Path of asset");
        retVal[counter].minWidth = 100;
        retVal[counter].width = 300;
        retVal[counter].maxWidth = 1000;
        retVal[counter].headerTextAlignment = TextAlignment.Left;
        retVal[counter].canSort = true;
        retVal[counter].autoResize = true;
        counter++;

        retVal[counter].headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType"), "Asset type");
        retVal[counter].minWidth = 20;
        retVal[counter].width = 20;
        retVal[counter].maxWidth = 20;
        retVal[counter].headerTextAlignment = TextAlignment.Left;
        retVal[counter].canSort = false;
        retVal[counter].autoResize = true;
        counter++;

        retVal[counter].headerContent = new GUIContent("Separately", "Group can packed separately");
        retVal[counter].minWidth = 50;
        retVal[counter].width = 100;
        retVal[counter].maxWidth = 200;
        retVal[counter].headerTextAlignment = TextAlignment.Left;
        retVal[counter].canSort = true;
        retVal[counter].autoResize = true;

        return retVal;
    }

    public static bool IsPathValidForGroup(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        if (Directory.Exists(path) == false)
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
}

public class FixedAssetTreeViewItem : TreeViewItem
{
    public string entry;
    public FixedGroup group;
    public Texture2D assetIcon;

    public override string displayName
    {
        get
        {
            if (entry != null)
            {
                return entry;
            }
            if (group != null)
            {
                if (FixedGroupTreeView.IsPathValidForGroup(group.folder) == false)
                {
                    return string.Format("{0}(NotValid)", group.folder);
                }
                return string.Format("{0} ({1})", group.folder, group.Entries.Count);
            }
            return "Error";
        }
    }
    public bool ReadOnly
    {
        get
        {
            if (group == null)
            {
                FixedAssetTreeViewItem item = parent as FixedAssetTreeViewItem;
                return item.ReadOnly;
            }
            return GStore.AssetTable.FolderMap.ContainsKey(group.folder);
        }
    }

    public FixedAssetTreeViewItem(string assetPath, int d) : base(assetPath.GetHashCode(), d, null)
    {
        this.entry = assetPath;
        group = null;
        assetIcon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;
    }

    public FixedAssetTreeViewItem(FixedGroup g, int d) : base(g.folder.GetHashCode(), d, null)
    {
        entry = null;
        group = g;
        assetIcon = AssetDatabase.GetCachedIcon(group.folder) as Texture2D;
    }
}
