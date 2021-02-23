using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;
using System.IO;
using AssetBundleBrowser.AssetBundleModel;
using GStore;

namespace AssetBundleBrowser
{
    internal class AssetBundleTree : TreeView
    {
        AssetBundleManageTab m_Controller;
        private bool m_ContextOnItem = false;
        //List<UnityEngine.Object> m_EmptyObjectList = new List<UnityEngine.Object>();

        #region Good Modeule

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                //new MultiColumnHeaderState.Column(),
            };
            retVal[0].headerContent = new GUIContent("包名", "最终的ab包名");
            retVal[0].minWidth = 320;
            retVal[0].width = 320;
            retVal[0].maxWidth = 400;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("打包前大小", "打包ab前总文件大小");
            retVal[1].minWidth = 80;
            retVal[1].width = 80;
            retVal[1].maxWidth = 80;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("打包后大小", "打包ab后的文件大小");
            retVal[2].minWidth = 80;
            retVal[2].width = 80;
            retVal[2].maxWidth = 80;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            //retVal[3].headerContent = new GUIContent("常驻内存", "是否常驻内存中不销毁");
            //retVal[3].minWidth = 50;
            //retVal[3].width = 60;
            //retVal[3].maxWidth = 60;
            //retVal[3].headerTextAlignment = TextAlignment.Left;
            //retVal[3].canSort = true;
            //retVal[3].autoResize = true;

            retVal[3].headerContent = new GUIContent("小包", "是否是小包合并打包");
            retVal[3].minWidth = 50;
            retVal[3].width = 60;
            retVal[3].maxWidth = 60;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = true;
            retVal[3].allowToggleVisibility = true;

            return retVal;
        }
        enum MyColumns
        {
            Tag,
            OriginSize,
            FinalSize,
            //DontDestroy,
            AlwaysPacked
        }
        internal enum SortOption
        {
            Tag,
            OriginSize,
            FinalSize,
            //DontDestroy,
            AlwaysPacked

        }
        SortOption[] m_SortOptions =
        {
            SortOption.Tag,
            SortOption.OriginSize,
            SortOption.FinalSize,
            //SortOption.DontDestroy,
            SortOption.AlwaysPacked
        };

        #endregion


        internal AssetBundleTree(TreeViewState state, MultiColumnHeaderState mchs, AssetBundleManageTab ctrl) : base(state, new MultiColumnHeader(mchs))
        {
            AssetBundleModel.Model.Rebuild();
            m_Controller = ctrl;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortedColumnIndex = 0;
            multiColumnHeader.IsSortedAscending(0);
            multiColumnHeader.sortingChanged += OnSortingChanged;

        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item != null && item.displayName.Length > 0;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            var bundleItem = item as AssetBundleModel.BundleTreeItem;
            return bundleItem.bundle.DoesItemMatchSearch(search);
        }

        /*private bool isFirst = true;
        private int index = 0;*/
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item as AssetBundleModel.BundleTreeItem, args.GetColumn(i), ref args);

            }

            /*var bundleItem = (args.item as AssetBundleModel.BundleTreeItem);
            if (args.item.icon == null)
                extraSpaceBeforeIconAndLabel = 16f;
            else
                extraSpaceBeforeIconAndLabel = 0f;

            Color old = GUI.color;
            if ((bundleItem.bundle as AssetBundleModel.BundleVariantFolderInfo) != null)
                GUI.color = AssetBundleModel.Model.k_LightGrey; //new Color(0.3f, 0.5f, 0.85f);
            base.RowGUI(args);
            GUI.color = old;

            var message = bundleItem.BundleMessage();
            if(message.severity != MessageType.None)
            {
                var size = args.rowRect.height;
                var right = args.rowRect.xMax;
                Rect messageRect = new Rect(right - size, args.rowRect.yMin, size, size);
                GUI.Label(messageRect, new GUIContent(message.icon, message.message ));
            }*/
        }

        private void CellGUI(Rect cellRect, AssetBundleModel.BundleTreeItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);


            switch (column)
            {
                case 0:
                    var bundleItem = (args.item as AssetBundleModel.BundleTreeItem);
                    if (args.item.icon == null)
                        extraSpaceBeforeIconAndLabel = 16f;
                    else
                        extraSpaceBeforeIconAndLabel = 0f;

                    Color old = GUI.color;
                    if ((bundleItem.bundle as AssetBundleModel.BundleVariantFolderInfo) != null)
                        GUI.color = AssetBundleModel.Model.k_LightGrey; //new Color(0.3f, 0.5f, 0.85f);
                    base.RowGUI(args);
                    /*GUI.color = old;

                    var message = bundleItem.BundleMessage();
                    if (message.severity != MessageType.None)
                    {
                        var size = args.rowRect.height;
                        var right = args.rowRect.xMax;
                        Rect messageRect = new Rect(right - size, args.rowRect.yMin, size, size);
                        GUI.Label(messageRect, new GUIContent(message.icon, message.message));
                    }*/

                    /*if (AssetBundleBuildEditor.instance.m_ManageTab.hasSearch)
                    {
                        UnityEngine.Debug.Log("---show size---" + GetSizeString(item.bundle.finalSize));
                    }*/

                    /*var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                    DefaultGUI.Label(
                        new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                        item.displayName,
                        args.selected,
                        args.focused);*/
                    break;
                case 1:

                    //UnityEngine.Debug.Log("---item.bundle.displayName---" + item.bundle.displayName);
                    DefaultGUI.Label(cellRect, GetSizeString(item.bundle.originSize), args.selected, args.focused);
                    break;
                case 2:

                    DefaultGUI.Label(cellRect, GetSizeString(item.bundle.finalSize), args.selected, args.focused);
                    break;
                //case 3:
                //    Rect toggleRect = cellRect;
                //    toggleRect.x += GetContentIndent(item);
                //    toggleRect.width = 18;
                //    if (toggleRect.xMax < cellRect.xMax)
                //    {
                //        if (hasSearch)
                //        {
                //            dicBundleInfo[item.bundle.m_Name.shortName].isDontDestroy = EditorGUI.Toggle(toggleRect, dicBundleInfo[item.bundle.m_Name.shortName].isDontDestroy);
                //        }
                //        else
                //        {
                //            item.bundle.isDontDestroy = EditorGUI.Toggle(toggleRect, item.bundle.isDontDestroy);
                //        }
                //    }
                //    break;
                case 3:
                    Rect toggleRect2 = cellRect;
                    toggleRect2.x += GetContentIndent(item);
                    toggleRect2.width = 18;
                    if (toggleRect2.xMax < cellRect.xMax)
                    {
                        item.bundle.isAlwaysPacked = EditorGUI.Toggle(toggleRect2, item.bundle.isAlwaysPacked);
                    }
                    //item.bundle.isAlwaysPacked = EditorGUI.Toggle(toggleRect2, item.bundle.isAlwaysPacked);
                    break;
            }
            GUI.color = oldColor;
        }

        internal string GetSizeString(long fileSize)
        {
            if (fileSize == 0)
                return "--";
            return EditorUtility.FormatBytes(fileSize);
        }

        public void CalculatePackageSize()
        {
            int totalCount = rootItem.children.Count;
            int curIndex = 0;
            foreach (var item in rootItem.children)
            {
                curIndex++;
                BundleTreeItem bundleItem = item as AssetBundleModel.BundleTreeItem;
                if (bundleItem == null)
                {
                    continue;
                }
                var bundleInfo = bundleItem.bundle;
                EditorTools.DisplayProgressBar("AssetBundles计算中...  (" + curIndex + "/" + totalCount + ")", "正在计算AB  " + bundleInfo.displayName, curIndex / (float)totalCount);
                bundleInfo.Update();
            }
            EditorTools.ClearProgressBar();
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
            if (args.newName.Length > 0 && args.newName != args.originalName)
            {
                args.newName = args.newName.ToLower();
                args.acceptedRename = true;

                AssetBundleModel.BundleTreeItem renamedItem = FindItem(args.itemID, rootItem) as AssetBundleModel.BundleTreeItem;
                args.acceptedRename = AssetBundleModel.Model.HandleBundleRename(renamedItem, args.newName);
                ReloadAndSelect(renamedItem.bundle.nameHashCode, false);
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            AssetBundleModel.Model.Refresh();
            var root = AssetBundleModel.Model.CreateBundleTreeView();
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {

            var selectedBundles = new List<AssetBundleModel.BundleInfo>();
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var item = FindItem(id, rootItem) as AssetBundleModel.BundleTreeItem;
                    if (item != null && item.bundle != null)
                    {
                        item.bundle.RefreshAssetList();
                        selectedBundles.Add(item.bundle);
                    }
                }
            }

            m_Controller.UpdateSelectedBundles(selectedBundles);
        }


        public override void OnGUI(Rect rect)
        {
            //Debug.Log("----rect----" +　rect.ToString());
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }


        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }

            List<AssetBundleModel.BundleTreeItem> selectedNodes = new List<AssetBundleModel.BundleTreeItem>();
            GenericMenu menu = new GenericMenu();

            if (!AssetBundleModel.Model.DataSource.IsReadOnly())
            {
                menu.AddItem(new GUIContent("Add new bundle"), false, CreateNewBundle, selectedNodes);
                menu.AddItem(new GUIContent("Add new folder"), false, CreateFolder, selectedNodes);
            }

            menu.AddItem(new GUIContent("Reload all data"), false, ForceReloadData, selectedNodes);
            menu.ShowAsContext();
        }

        protected override void ContextClickedItem(int id)
        {
            if (AssetBundleModel.Model.DataSource.IsReadOnly())
            {
                return;
            }

            m_ContextOnItem = true;
            List<AssetBundleModel.BundleTreeItem> selectedNodes = new List<AssetBundleModel.BundleTreeItem>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as AssetBundleModel.BundleTreeItem);
            }

            // 禁用右键菜单
            /*GenericMenu menu = new GenericMenu();
            
            if(selectedNodes.Count == 1)
            {
                if ((selectedNodes[0].bundle as AssetBundleModel.BundleFolderConcreteInfo) != null)
                {
                    menu.AddItem(new GUIContent("Add Child/New Bundle"), false, CreateNewBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Child/New Folder"), false, CreateFolder, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                }
                else if( (selectedNodes[0].bundle as AssetBundleModel.BundleVariantFolderInfo) != null)
                {
                    menu.AddItem(new GUIContent("Add Child/New Variant"), false, CreateNewVariant, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                    menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                }
                else
                {
                    var variant = selectedNodes[0].bundle as AssetBundleModel.BundleVariantDataInfo;
                    if (variant == null)
                    {
                        menu.AddItem(new GUIContent("Add Sibling/New Bundle"), false, CreateNewSiblingBundle, selectedNodes);
                        menu.AddItem(new GUIContent("Add Sibling/New Folder"), false, CreateNewSiblingFolder, selectedNodes);
                        menu.AddItem(new GUIContent("Convert to variant"), false, ConvertToVariant, selectedNodes);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add Sibling/New Variant"), false, CreateNewSiblingVariant, selectedNodes);
                    }
                }
                if(selectedNodes[0].bundle.IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
                    menu.AddItem(new GUIContent("Move duplicates to new bundle"), false, DedupeAllBundles, selectedNodes);
                //menu.AddItem(new GUIContent("Rename"), false, RenameBundle, selectedNodes);
                //menu.AddItem(new GUIContent("Delete " + selectedNodes[0].displayName), false, DeleteBundles, selectedNodes);
                
            }
            else if (selectedNodes.Count > 1)
            { 
                menu.AddItem(new GUIContent("Move duplicates shared by selected"), false, DedupeOverlappedBundles, selectedNodes);
                menu.AddItem(new GUIContent("Move duplicates existing in any selected"), false, DedupeAllBundles, selectedNodes);
                menu.AddItem(new GUIContent("Delete " + selectedNodes.Count + " selected bundles"), false, DeleteBundles, selectedNodes);
            }
            menu.ShowAsContext();*/
        }
        void ForceReloadData(object context)
        {
            AssetBundleModel.Model.ForceReloadData(this);
        }

        void CreateNewSiblingFolder(object context)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                AssetBundleModel.BundleFolderConcreteInfo folder = null;
                folder = selectedNodes[0].bundle.parent as AssetBundleModel.BundleFolderConcreteInfo;
                CreateFolderUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }
        void CreateFolder(object context)
        {
            AssetBundleModel.BundleFolderConcreteInfo folder = null;
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                folder = selectedNodes[0].bundle as AssetBundleModel.BundleFolderConcreteInfo;
            }
            CreateFolderUnderParent(folder);
        }
        void CreateFolderUnderParent(AssetBundleModel.BundleFolderConcreteInfo folder)
        {
            var newBundle = AssetBundleModel.Model.CreateEmptyBundleFolder(folder);
            ReloadAndSelect(newBundle.nameHashCode, true);
        }
        void RenameBundle(object context)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                BeginRename(FindItem(selectedNodes[0].bundle.nameHashCode, rootItem));
            }
        }

        void CreateNewSiblingBundle(object context)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                AssetBundleModel.BundleFolderConcreteInfo folder = null;
                folder = selectedNodes[0].bundle.parent as AssetBundleModel.BundleFolderConcreteInfo;
                CreateBundleUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }
        void CreateNewBundle(object context)
        {
            AssetBundleModel.BundleFolderConcreteInfo folder = null;
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                folder = selectedNodes[0].bundle as AssetBundleModel.BundleFolderConcreteInfo;
            }
            CreateBundleUnderParent(folder);
        }

        void CreateBundleUnderParent(AssetBundleModel.BundleFolderInfo folder)
        {
            var newBundle = AssetBundleModel.Model.CreateEmptyBundle(folder);
            ReloadAndSelect(newBundle.nameHashCode, true);
        }


        void CreateNewSiblingVariant(object context)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                AssetBundleModel.BundleVariantFolderInfo folder = null;
                folder = selectedNodes[0].bundle.parent as AssetBundleModel.BundleVariantFolderInfo;
                CreateVariantUnderParent(folder);
            }
            else
                Debug.LogError("could not add 'sibling' with no bundles selected");
        }
        void CreateNewVariant(object context)
        {
            AssetBundleModel.BundleVariantFolderInfo folder = null;
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count == 1)
            {
                folder = selectedNodes[0].bundle as AssetBundleModel.BundleVariantFolderInfo;
                CreateVariantUnderParent(folder);
            }
        }
        void CreateVariantUnderParent(AssetBundleModel.BundleVariantFolderInfo folder)
        {
            if (folder != null)
            {
                var newBundle = AssetBundleModel.Model.CreateEmptyVariant(folder);
                ReloadAndSelect(newBundle.nameHashCode, true);
            }
        }

        void ConvertToVariant(object context)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            if (selectedNodes.Count == 1)
            {
                var bundle = selectedNodes[0].bundle as AssetBundleModel.BundleDataInfo;
                var newBundle = AssetBundleModel.Model.HandleConvertToVariant(bundle);
                int hash = 0;
                if (newBundle != null)
                    hash = newBundle.nameHashCode;
                ReloadAndSelect(hash, true);
            }
        }

        void DedupeOverlappedBundles(object context)
        {
            DedupeBundles(context, true);
        }
        void DedupeAllBundles(object context)
        {
            DedupeBundles(context, false);
        }
        void DedupeBundles(object context, bool onlyOverlappedAssets)
        {
            var selectedNodes = context as List<AssetBundleModel.BundleTreeItem>;
            var newBundle = AssetBundleModel.Model.HandleDedupeBundles(selectedNodes.Select(item => item.bundle), onlyOverlappedAssets);
            if (newBundle != null)
            {
                var selection = new List<int>();
                selection.Add(newBundle.nameHashCode);
                ReloadAndSelect(selection);
            }
            else
            {
                if (onlyOverlappedAssets)
                    Debug.LogWarning("There were no duplicated assets that existed across all selected bundles.");
                else
                    Debug.LogWarning("No duplicate assets found after refreshing bundle contents.");
            }
        }

        void DeleteBundles(object b)
        {
            var selectedNodes = b as List<AssetBundleModel.BundleTreeItem>;
            AssetBundleModel.Model.HandleBundleDelete(selectedNodes.Select(item => item.bundle));
            ReloadAndSelect(new List<int>());


        }
        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<AssetBundleModel.BundleTreeItem> selectedNodes = new List<AssetBundleModel.BundleTreeItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as AssetBundleModel.BundleTreeItem);
                }
                DeleteBundles(selectedNodes);
            }
        }

        class DragAndDropData
        {
            internal bool hasBundleFolder = false;
            internal bool hasScene = false;
            internal bool hasNonScene = false;
            internal bool hasVariantChild = false;
            internal List<AssetBundleModel.BundleInfo> draggedNodes;
            internal AssetBundleModel.BundleTreeItem targetNode;
            internal DragAndDropArgs args;
            internal string[] paths;

            internal DragAndDropData(DragAndDropArgs a)
            {
                args = a;
                draggedNodes = DragAndDrop.GetGenericData("AssetBundleModel.BundleInfo") as List<AssetBundleModel.BundleInfo>;
                targetNode = args.parentItem as AssetBundleModel.BundleTreeItem;
                paths = DragAndDrop.paths;

                if (draggedNodes != null)
                {
                    foreach (var bundle in draggedNodes)
                    {
                        if ((bundle as AssetBundleModel.BundleFolderInfo) != null)
                        {
                            hasBundleFolder = true;
                        }
                        else
                        {
                            var dataBundle = bundle as AssetBundleModel.BundleDataInfo;
                            if (dataBundle != null)
                            {
                                if (dataBundle.isSceneBundle)
                                    hasScene = true;
                                else
                                    hasNonScene = true;

                                if ((dataBundle as AssetBundleModel.BundleVariantDataInfo) != null)
                                    hasVariantChild = true;
                            }
                        }
                    }
                }
                else if (DragAndDrop.paths != null)
                {
                    foreach (var assetPath in DragAndDrop.paths)
                    {
                        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                            hasScene = true;
                        else
                            hasNonScene = true;
                    }
                }
            }

        }
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            // 禁用拖拽
            return DragAndDropVisualMode.Rejected;

            /*DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;
            DragAndDropData data = new DragAndDropData(args);
            
            if (AssetBundleModel.Model.DataSource.IsReadOnly ()) {
                return DragAndDropVisualMode.Rejected;
            }

            if ( (data.hasScene && data.hasNonScene) ||
                (data.hasVariantChild) )
                return DragAndDropVisualMode.Rejected;
            
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                    visualMode = HandleDragDropUpon(data);
                    break;
                case DragAndDropPosition.BetweenItems:
                    visualMode = HandleDragDropBetween(data);
                    break;
                case DragAndDropPosition.OutsideItems:
                    if (data.draggedNodes != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            AssetBundleModel.Model.HandleBundleReparent(data.draggedNodes, null);
                            Reload();
                        }
                    }
                    else if(data.paths != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            DragPathsToNewSpace(data.paths, null);
                        }
                    }
                    break;
            }
            return visualMode;*/
        }

        private DragAndDropVisualMode HandleDragDropUpon(DragAndDropData data)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.Copy;//Move;
            var targetDataBundle = data.targetNode.bundle as AssetBundleModel.BundleDataInfo;
            if (targetDataBundle != null)
            {
                if (targetDataBundle.isSceneBundle)
                {
                    if (data.hasNonScene)
                        return DragAndDropVisualMode.Rejected;
                }
                else
                {
                    if (data.hasBundleFolder)
                    {
                        return DragAndDropVisualMode.Rejected;
                    }
                    else if (data.hasScene && !targetDataBundle.IsEmpty())
                    {
                        return DragAndDropVisualMode.Rejected;
                    }

                }


                if (data.args.performDrop)
                {
                    if (data.draggedNodes != null)
                    {
                        AssetBundleModel.Model.HandleBundleMerge(data.draggedNodes, targetDataBundle);
                        ReloadAndSelect(targetDataBundle.nameHashCode, false);
                    }
                    else if (data.paths != null)
                    {
                        AssetBundleModel.Model.MoveAssetToBundle(data.paths, targetDataBundle.m_Name.bundleName, targetDataBundle.m_Name.variant);
                        AssetBundleModel.Model.ExecuteAssetMove();
                        ReloadAndSelect(targetDataBundle.nameHashCode, false);
                    }
                }

            }
            else
            {
                var folder = data.targetNode.bundle as AssetBundleModel.BundleFolderInfo;
                if (folder != null)
                {
                    if (data.args.performDrop)
                    {
                        if (data.draggedNodes != null)
                        {
                            AssetBundleModel.Model.HandleBundleReparent(data.draggedNodes, folder);
                            Reload();
                        }
                        else if (data.paths != null)
                        {
                            DragPathsToNewSpace(data.paths, folder);
                        }
                    }
                }
                else
                    visualMode = DragAndDropVisualMode.Rejected; //must be a variantfolder

            }
            return visualMode;
        }
        private DragAndDropVisualMode HandleDragDropBetween(DragAndDropData data)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.Copy;//Move;

            var parent = (data.args.parentItem as AssetBundleModel.BundleTreeItem);

            if (parent != null)
            {
                var variantFolder = parent.bundle as AssetBundleModel.BundleVariantFolderInfo;
                if (variantFolder != null)
                    return DragAndDropVisualMode.Rejected;

                if (data.args.performDrop)
                {
                    var folder = parent.bundle as AssetBundleModel.BundleFolderConcreteInfo;
                    if (folder != null)
                    {
                        if (data.draggedNodes != null)
                        {
                            AssetBundleModel.Model.HandleBundleReparent(data.draggedNodes, folder);
                            Reload();
                        }
                        else if (data.paths != null)
                        {
                            DragPathsToNewSpace(data.paths, folder);
                        }
                    }
                }
            }

            return visualMode;
        }

        private string[] dragToNewSpacePaths = null;
        private AssetBundleModel.BundleFolderInfo dragToNewSpaceRoot = null;
        private void DragPathsAsOneBundle()
        {
            var newBundle = AssetBundleModel.Model.CreateEmptyBundle(dragToNewSpaceRoot);
            AssetBundleModel.Model.MoveAssetToBundle(dragToNewSpacePaths, newBundle.m_Name.bundleName, newBundle.m_Name.variant);
            AssetBundleModel.Model.ExecuteAssetMove();
            ReloadAndSelect(newBundle.nameHashCode, true);
        }
        private void DragPathsAsManyBundles()
        {
            List<int> hashCodes = new List<int>();
            foreach (var assetPath in dragToNewSpacePaths)
            {
                var newBundle = AssetBundleModel.Model.CreateEmptyBundle(dragToNewSpaceRoot, System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower());
                AssetBundleModel.Model.MoveAssetToBundle(assetPath, newBundle.m_Name.bundleName, newBundle.m_Name.variant);
                hashCodes.Add(newBundle.nameHashCode);
            }
            AssetBundleModel.Model.ExecuteAssetMove();
            ReloadAndSelect(hashCodes);
        }

        private void DragPathsToNewSpace(string[] paths, AssetBundleModel.BundleFolderInfo root)
        {
            dragToNewSpacePaths = paths;
            dragToNewSpaceRoot = root;
            if (paths.Length > 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create 1 Bundle"), false, DragPathsAsOneBundle);
                var message = "Create ";
                message += paths.Length;
                message += " Bundles";
                menu.AddItem(new GUIContent(message), false, DragPathsAsManyBundles);
                menu.ShowAsContext();
            }
            else
                DragPathsAsManyBundles();
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            return;
            /*if (args.draggedItemIDs == null)
                return;

            DragAndDrop.PrepareStartDrag();

            var selectedBundles = new List<AssetBundleModel.BundleInfo>();
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItem(id, rootItem) as AssetBundleModel.BundleTreeItem;
                selectedBundles.Add(item.bundle);
            }
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = m_EmptyObjectList.ToArray();
            DragAndDrop.SetGenericData("AssetBundleModel.BundleInfo", selectedBundles);
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;//Move;
            DragAndDrop.StartDrag("AssetBundleTree");*/
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        internal void Refresh()
        {
            var selection = GetSelection();
            Reload();
            SelectionChanged(selection);
        }

        private void ReloadAndSelect(int hashCode, bool rename)
        {
            var selection = new List<int>();
            selection.Add(hashCode);
            ReloadAndSelect(selection);
            if (rename)
            {
                BeginRename(FindItem(hashCode, rootItem), 0.25f);
            }
        }
        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
        }

        /// <summary>
        /// 排序模块
        /// </summary>
        /// <param name="multiColumnHeader"></param>
        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            //if(rootItem != null)
            SortIfNeeded(rootItem, GetRows());
        }
        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (root == null)
            {
                return;
            }
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }
        void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<AssetBundleModel.BundleTreeItem> bundleList = new List<AssetBundleModel.BundleTreeItem>();
            foreach (var item in rootItem.children)
            {
                bundleList.Add(item as AssetBundleModel.BundleTreeItem);
            }
            var orderedItems = InitialOrder(bundleList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<AssetBundleModel.BundleTreeItem> InitialOrder(IEnumerable<AssetBundleModel.BundleTreeItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Tag:
                    return myTypes.Order(l => l.bundle.displayName, ascending);
                case SortOption.OriginSize:
                    return myTypes.Order(l => l.bundle.originSize, ascending);
                case SortOption.FinalSize:
                    return myTypes.Order(l => l.bundle.finalSize, ascending);
                //case SortOption.DontDestroy:
                //    return myTypes.Order(l => l.bundle.isDontDestroy, ascending);
                case SortOption.AlwaysPacked:
                    return myTypes.Order(l => l.bundle.isAlwaysPacked, ascending);
                default:
                    return myTypes.Order(l => l.bundle.displayName, ascending);
            }

        }
    }
}
