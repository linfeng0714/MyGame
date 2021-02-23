﻿using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

//带数据的TreeViewItem
public class AssetViewItem : TreeViewItem
{
    public ReferenceCache.AssetDescription data;
}

//资源引用树
public class AssetTreeView : TreeView
{
    //图标宽度
    const float kIconWidth = 18f;
    //列表高度
    const float kRowHeights = 20f;

    private ReferenceFinderWindow window;
    public AssetViewItem assetRoot;

    private GUIStyle stateGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };

    //列信息
    enum MyColumns
    {
        Name,
        Path,
        State,
    }

    public AssetTreeView(ReferenceFinderWindow window, TreeViewState state, MultiColumnHeader multicolumnHeader) : base(state, multicolumnHeader)
    {
        this.window = window;
        rowHeight = kRowHeights;
        columnIndexForTreeFoldouts = 0;
        showAlternatingRowBackgrounds = true;
        showBorder = false;
        customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
        extraSpaceBeforeIconAndLabel = kIconWidth;
    }
    //响应双击事件
    protected override void DoubleClickedItem(int id)
    {
        var item = (AssetViewItem)FindItem(id, rootItem);
        //在ProjectWindow中高亮双击资源
        if (item != null)
        {
            var assetObject = AssetDatabase.LoadAssetAtPath(item.data.path, typeof(UnityEngine.Object));
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = assetObject;
            EditorGUIUtility.PingObject(assetObject);
        }
    }

    protected override void ContextClickedItem(int id)
    {
        //base.ContextClickedItem(id);

        List<AssetViewItem> selectedNodes = new List<AssetViewItem>();
        foreach (var nodeId in GetSelection())
        {
            var item = FindItemInVisibleRows(nodeId); //TODO - this probably makes off-screen but selected items not get added to list.
            if (item != null)
                selectedNodes.Add(item);
        }
        if (selectedNodes.Count == 0)
            return;

        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Set as Root"), false, () =>
        {
            window.UpdateSelectedAssets(selectedNodes);
        });

        menu.ShowAsContext();
    }

    AssetViewItem FindItemInVisibleRows(int id)
    {
        var rows = GetRows();
        foreach (var r in rows)
        {
            if (r.id == id)
            {
                return r as AssetViewItem;
            }
        }
        return null;
    }

    //生成ColumnHeader
    public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
    {
        var columns = new[]
        {
            //图标+名称
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 200,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false
            },
            //路径
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Path"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 360,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false
    },
            //状态
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("State"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 60,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = true,
                canSort = false
            },
        };
        var state = new MultiColumnHeaderState(columns);
        return state;
    }

    protected override TreeViewItem BuildRoot()
    {
        return assetRoot;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (AssetViewItem)args.item;
        for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
        {
            CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
        }
    }

    //绘制列表中的每项内容
    void CellGUI(Rect cellRect, AssetViewItem item, MyColumns column, ref RowGUIArgs args)
    {
        CenterRectUsingSingleLineHeight(ref cellRect);
        switch (column)
        {
            case MyColumns.Name:
                {
                    var iconRect = cellRect;
                    iconRect.x += GetContentIndent(item);
                    iconRect.width = kIconWidth;
                    if (iconRect.x < cellRect.xMax)
                    {
                        var icon = GetIcon(item.data.path);
                        if (icon != null)
                            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                }
                break;
            case MyColumns.Path:
                {
                    GUI.Label(cellRect, item.data.path);
                }
                break;
            case MyColumns.State:
                {
                    GUI.Label(cellRect, ReferenceCache.GetInfoByState(item.data.state), stateGUIStyle);
                }
                break;
        }
    }

    //根据资源信息获取资源图标
    private Texture2D GetIcon(string path)
    {
        return AssetDatabase.GetCachedIcon(path) as Texture2D;
    }
}
