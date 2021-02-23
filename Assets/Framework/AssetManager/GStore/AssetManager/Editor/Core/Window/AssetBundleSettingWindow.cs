using UnityEngine;
using UnityEditor;
using System.IO;
using GStore;
using System.Collections.Generic;
using System;

public class AssetBundleSettingWindow : SubWindow
{
    private List<ProjectBuildMethod> m_Methods;

    public ProjectBuilderSettings Settings
    {
        get
        {
            return ProjectBuilderSettings.Instance;
        }
    }

    public override string GetTitle()
    {
        return "打包配置";
    }

    GUIStyle m_LableStyle;

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal("Box");
        {
            GUILayout.Label("打包配置:");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(Settings, typeof(ProjectBuilderSettings), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.Label("");
            if (GUILayout.Button("修改", GUILayout.ExpandWidth(false)))
            {
                EditorGUIUtility.PingObject(Settings);
                Selection.activeObject = Settings;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical("Box");
        {
            DrawGameSetting();
        }
        GUILayout.EndVertical();

        DrawOptionalBuild();//可选操作

        if (m_LableStyle == null)
        {
            m_LableStyle = new GUIStyle(GUI.skin.label);
            m_LableStyle.richText = true;
        }

        GUILayout.BeginVertical("Box");
        GUILayout.Label("说明：通过在Editor方法上标注<b><color=cyan>[ProjectBuildMethod]</color></b>, 可以将方法注册到构建列表，一键打包时自动调用。", m_LableStyle);
        GUILayout.EndVertical();

        GUILayout.EndArea();
    }

    void DrawGameSetting()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("游戏配置:");
        if (GUILayout.Button("刷新", GUILayout.ExpandWidth(false)))
        {
            GameSettingSetup.Setup();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical("Box");
        EditorGUIUtility.labelWidth = 80;
        EditorGUILayout.LabelField("是否开启热更", GameSetting.hotUpdateMode.ToString());
        EditorGUILayout.LabelField("是否开启加密", GameSetting.encrypt.ToString());
        EditorGUILayout.LabelField("是否开启小包", GameSetting.smallPackage.ToString());
        EditorGUILayout.LabelField("HotUpdateConfig.xml配置", GameSetting.hotUpdateConfigId.ToString());
        EditorGUIUtility.labelWidth = 0;
        GUILayout.EndVertical();


    }

    void DrawOptionalBuild()
    {
        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        GUILayout.Label("构建步骤：");

        if (GUILayout.Button("执行勾选", GUILayout.ExpandWidth(false)))
        {
            ExecuteAction(OptionalBuild);
        }
        if (GUILayout.Button("一键出包", GUILayout.ExpandWidth(false)))
        {
            ExecuteAction(BuildTools.BuildPlayerProcessor);
        }

        GUILayout.EndHorizontal();

        if (m_Methods == null)
        {
            m_Methods = ProjectBuildMethod.List;

            for (int i = 0; i < m_Methods.Count; i++)
            {
                ProjectBuildMethod method = m_Methods[i];
                method.selected = DataUtil.IsBit(Settings.buildOptions, 1 << i);
            }
        }

        for (int i = 0; i < m_Methods.Count; i++)
        {
            ProjectBuildMethod method = m_Methods[i];

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label(method.order.ToString(), GUILayout.Width(20));
            method.selected = GUILayout.Toggle(method.selected, "", GUILayout.Width(20));
            EditorGUILayout.LabelField(method.name);
            if (GUILayout.Button("执行"))
            {
                ExecuteAction(() =>
                {
                    if (method.func() == false)
                    {
                        EditorUtility.DisplayDialog("失败", string.Format("执行 {0} 失败, 操作终止!", method.name), "确认");
                    }
                });
            }
            GUILayout.EndHorizontal();

            int lastOptions = Settings.buildOptions;
            DataUtil.SetBit(ref Settings.buildOptions, 1 << i, method.selected);
            if (lastOptions != Settings.buildOptions)
            {
                EditorUtility.SetDirty(Settings);
            }
        }

        GUILayout.EndVertical();
    }

    public static void OptionalBuild()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        for (int i = 0; i < ProjectBuildMethod.List.Count; i++)
        {
            ProjectBuildMethod method = ProjectBuildMethod.List[i];
            int key = 1 << i;

            if (DataUtil.IsBit((int)ProjectBuilderSettings.Instance.buildOptions, key))
            {
                sw.Reset();
                sw.Start();
                if (method.func() == false)
                {
                    throw new ProjectBuildException("Execute {0} Failed, Abort！", method.name);
                }
                sw.Stop();
                Debug.LogWarningFormat("Execute {0} Success, Cost Time:{1}", method.name, sw.Elapsed.ToString());
            }
        }
    }

    protected void ExecuteAction(System.Action action)
    {
        EditorApplication.delayCall = () =>
        {
            EditorApplication.delayCall = null;
            if (action != null)
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                try
                {
                    watch.Start();

                    action();

                    watch.Stop();
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        };
        EditorUtility.DisplayProgressBar("等待操作执行", "", 0);
    }
}

