using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 构建方法
/// </summary>
public sealed class ProjectBuildMethod
{
    public int order;
    public string name;
    public bool required;
    public Func<bool> func;
    public bool selected = false;

    private ProjectBuildMethod() { }

    private static List<ProjectBuildMethod> s_List;
    public static List<ProjectBuildMethod> List
    {
        get
        {
            if (s_List == null)
            {
                s_List = Load();
            }
            return s_List;
        }
    }

    private static List<ProjectBuildMethod> Load()
    {
        var list = new List<ProjectBuildMethod>();

        //只查找静态方法
        System.Reflection.BindingFlags methodBindingFlags = System.Reflection.BindingFlags.Public
                                                        | System.Reflection.BindingFlags.NonPublic
                                                        | System.Reflection.BindingFlags.Static;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            //只查找编辑器代码
            if (assembly.GetName().Name != "Assembly-CSharp-Editor")
            {
                continue;
            }
            foreach (var type in assembly.GetTypes())
            {
                foreach (var methodInfo in type.GetMethods(methodBindingFlags))
                {
                    object[] customAttributes = methodInfo.GetCustomAttributes(typeof(ProjcetBuildMethodAttribute), false);
                    if (customAttributes == null || customAttributes.Length == 0)
                    {
                        continue;
                    }

                    foreach (object attr in customAttributes)
                    {
                        ProjcetBuildMethodAttribute callbackAttr = attr as ProjcetBuildMethodAttribute;
                        if (null == callbackAttr)
                            continue;
                        if (methodInfo.GetParameters().Length != 0)
                            continue;

                        ProjectBuildMethod method = new ProjectBuildMethod()
                        {
                            order = callbackAttr.order,
                            name = string.Format("{0}({1})", callbackAttr.description, methodInfo.Name),
                            required = callbackAttr.required,
                            func = () =>
                            {
                                if (methodInfo.ReturnType == typeof(bool))
                                {
                                    return (bool)methodInfo.Invoke(null, null);
                                }
                                else
                                {
                                    try { methodInfo.Invoke(null, null); }
                                    catch (Exception e)
                                    {
                                        Debug.LogException(e);
                                        return false;
                                    }
                                    return true;
                                }
                            }

                        };

                        list.Add(method);
                    }
                }
            }
        }
        list.Sort((a, b) => { return a.order.CompareTo(b.order); });

        return list;
    }
}

public class ProjcetBuildMethodAttribute : Attribute
{
    /// <summary>
    /// 执行优先级
    /// </summary>
    public int order;

    /// <summary>
    /// 描述
    /// </summary>
    public string description;

    /// <summary>
    /// 是否是一键打包时必须执行的步骤
    /// </summary>
    public bool required;
    public ProjcetBuildMethodAttribute(int order, string description, bool required = true)
    {
        this.order = order;
        this.description = description;
        this.required = required;
    }
}