using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using GStore;
using UnityEngine;
#pragma warning disable 0414

public class StaticReferenceWindow : EditorWindow
{
    private const int k_SearchHeight = 20;
    const float k_SplitterWidth = 2f;

    [MenuItem("GStore/Asset/Debug/StaticReferences")]
    static void ShowWindow()
    {
        var window = GetWindow<StaticReferenceWindow>();
        window.titleContent = new GUIContent("Static Ref");
        window.Show();
    }

    private void OnEnable()
    {
        m_Position = GetSubWindowArea();
    }

    private void OnGUI()
    {
        var searchRect = new Rect(0, 0, position.width, k_SearchHeight);

        if (m_TreeState == null)
            m_TreeState = new TreeViewState();


        if (m_TreeView == null)
        {
            m_SearchField = new SearchField();

            m_TreeView = new ReferenceTreeView(m_TreeState, this);
            m_TreeView.Reload();
        }

        m_Position = GetSubWindowArea();

        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        var rect = new Rect(
            m_Position.x + k_SplitterWidth,
            m_Position.y,
            m_Position.width - k_SplitterWidth * 2,
            m_Position.height);

        TopToolbar(searchRect);
        m_TreeView.OnGUI(rect);
    }

    private Rect GetSubWindowArea()
    {
        float padding = k_SearchHeight;
        Rect subPos = new Rect(0, padding, position.width, position.height - padding);
        return subPos;
    }

    Rect m_Position;

    [NonSerialized]
    List<GUIStyle> m_SearchStyles;
    [NonSerialized]
    private GUIStyle m_ButtonStyle;
    private TreeViewState m_TreeState;
    private ReferenceTreeView m_TreeView;
    private SearchField m_SearchField;

    private int m_State = 1;

    private void TopToolbar(Rect toolbarPos)
    {
        if (m_SearchStyles == null)
        {
            m_SearchStyles = new List<GUIStyle>();
            m_SearchStyles.Add(GetStyle("ToolbarSeachTextFieldPopup")); //GetStyle("ToolbarSeachTextField");
            m_SearchStyles.Add(GetStyle("ToolbarSeachCancelButton"));
            m_SearchStyles.Add(GetStyle("ToolbarSeachCancelButtonEmpty"));
        }
        if (m_ButtonStyle == null)
            m_ButtonStyle = GetStyle("ToolbarButton");

        GUILayout.BeginArea(new Rect(0, 0, toolbarPos.width, k_SearchHeight));

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            if (GUILayout.Button("Take Sample", m_ButtonStyle, GUILayout.ExpandWidth(false)))
            {
                StaticReferenceFinder.Find(m_State);
                m_TreeView.Reload();
            }
            GUILayout.Space(5);

            string[] assemblies = StaticReferenceFinder.s_Assemblies;
            for (int i = 0; i < assemblies.Length; i++)
            {
                DataUtil.SetBit(ref m_State, i + 1, GUILayout.Toggle(DataUtil.IsBit(m_State, i + 1), assemblies[i], m_ButtonStyle, GUILayout.ExpandWidth(false)));
            }

            GUILayout.Label(string.Empty);

            Rect searchRect = GUILayoutUtility.GetRect(0, toolbarPos.width * 0.6f, 16f, 16f, m_SearchStyles[0], GUILayout.MinWidth(65), GUILayout.MaxWidth(300));

            //Rect searchRect = GUILayoutUtility.GetRect(0, toolbarPos.width, 16f, 16f, m_SearchStyles[0], GUILayout.MinWidth(65));

            var baseSearch = m_TreeView.searchString;

            var searchString = m_SearchField.OnGUI(searchRect, baseSearch, m_SearchStyles[0], m_SearchStyles[1], m_SearchStyles[2]);

            if (baseSearch != searchString)
            {
                m_TreeView.searchString = searchString;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
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

    public class ReferenceTreeView : TreeView
    {
        private readonly StaticReferenceWindow m_Window;

        internal ReferenceTreeView(TreeViewState state, StaticReferenceWindow window)
         : base(state)
        {
            this.m_Window = window;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);

            foreach (var typeReferences in StaticReferenceFinder.s_References)
            {
                TreeViewItem typeItem = new TreeViewItem(typeReferences.type.GetHashCode(), 0, typeReferences.type.ToString());
                root.AddChild(typeItem);

                foreach (var fieldReferences in typeReferences.fields)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 0; i < fieldReferences.fieldStack.Count; i++)
                    {
                        sb.Append(".");
                        sb.Append(fieldReferences.fieldStack[i].Name);
                    }
                    string name = sb.ToString();
                    TreeViewItem fieldItem = new TreeViewItem(fieldReferences.GetHashCode(), 1, name);
                    typeItem.AddChild(fieldItem);

                    foreach (var objectReferences in fieldReferences.objects)
                    {
                        BuildObjectReferences(objectReferences, fieldItem);
                    }
                }
            }

            return root;
        }

        private void BuildObjectReferences(StaticReferenceFinder.ObjectReferences objectReferences, TreeViewItem parent)
        {
            string name = string.Empty;

            if (objectReferences.obj == null)
            {
                try { name = string.Format("Destroyed Object:({0})", objectReferences.obj.GetType()); } catch { }
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }
            }
            else
            {
                name = objectReferences.obj.ToString();
            }

            ReferenceTreeViewItem referenceItem = new ReferenceTreeViewItem(objectReferences, name, parent.depth + 1);
            parent.AddChild(referenceItem);

            if (objectReferences.dependencies != null && objectReferences.dependencies.Count > 0)
            {
                foreach (var dependence in objectReferences.dependencies)
                {
                    BuildObjectReferences(dependence, referenceItem);
                }
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        //TODO
        //protected override void SingleClickedItem(int id)
        //{
        //    var item = FindItemInVisibleRows(id);
        //    if (item != null)
        //    {
        //        GameObject gameObject = item.GetGameObject();
        //        if (gameObject != null)
        //        {
        //            EditorGUIUtility.PingObject(gameObject);
        //        }

        //        if (m_IsSubView == false)
        //        {
        //            m_Window.SetSubViewSelection(item);
        //        }
        //    }
        //}
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count != 1)
            {
                return;
            }
            int id = selectedIds[0];
            var item = FindItemInVisibleRows(id);
            if (item != null)
            {
                UnityEngine.Object asset = item.reference.obj;
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItemInVisibleRows(id);
            if (item != null)
            {
                UnityEngine.Object asset = item.reference.obj;
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }
        }

        ReferenceTreeViewItem FindItemInVisibleRows(int id)
        {
            var rows = GetRows();
            foreach (var r in rows)
            {
                if (r.id == id)
                {
                    return r as ReferenceTreeViewItem;
                }
            }
            return null;
        }
    }

    public class ReferenceTreeViewItem : TreeViewItem
    {
        public StaticReferenceFinder.ObjectReferences reference;

        public ReferenceTreeViewItem(StaticReferenceFinder.ObjectReferences reference, string name, int depth) : base(reference.obj.GetHashCode(), depth, name)
        {
            this.reference = reference;
            this.icon = EditorGUIUtility.ObjectContent(reference.obj, reference.obj.GetType()).image as Texture2D;
        }
    }
}

