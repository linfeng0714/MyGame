using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Text;
using System.Linq;
using Object = UnityEngine.Object;

/// <summary>
/// 查找静态引用
/// </summary>
public partial class StaticReferenceFinder
{
    public static List<TypeReferences> s_References = new List<TypeReferences>();

    static HashSet<object> s_Check = new HashSet<object>();
    public static void Find(int state)
    {
        s_Check.Clear();
        s_References.Clear();

        for (int i = 0; i < s_Assemblies.Length; i++)
        {
            if (GStore.DataUtil.IsBit(state, i + 1))
            {
                LoadAssembly(s_Assemblies[i]);
            }
        }
    }

    public static string[] s_Assemblies = new string[] { "Assembly-CSharp", "Assembly-CSharp-firstpass" };

    static void LoadAssembly(string name)
    {
        Assembly assembly = null;
        try
        {
            assembly = Assembly.Load(name);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        finally
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.ContainsGenericParameters)
                    {
                        continue;
                    }
                    try
                    {
                        FieldInfo[] listFieldInfo = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        TypeReferences typeReferences = new TypeReferences() { type = type };
                        foreach (FieldInfo fieldInfo in listFieldInfo)
                        {
                            if (!fieldInfo.FieldType.IsValueType && !fieldInfo.FieldType.ContainsGenericParameters)
                            {
                                FieldReferences fieldReferences = new FieldReferences() { };
                                fieldReferences.fieldStack.Add(fieldInfo);
                                SearchProperties(fieldInfo.GetValue(null), fieldReferences, typeReferences);
                            }
                        }
                        if (typeReferences.foundObject)
                        {
                            s_References.Add(typeReferences);
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }

    static void SearchProperties(object obj, FieldReferences fieldReferences, TypeReferences typeReferences)
    {
        //忽略脚本
        if (obj is MonoScript)
        {
            return;
        }
        if (obj != null && s_Check.Add(obj))
        {
            if (obj is UnityEngine.Object)
            {
                if (obj is Component)
                {
                    return;
                }

                typeReferences.foundObject = true;
                UnityEngine.Object unityObject = obj as UnityEngine.Object;

                var objectReferences = new ObjectReferences() { obj = unityObject };

                if (fieldReferences.objects.Add(objectReferences) == false)
                {
                    return;
                }

                if (unityObject != null && EditorUtility.IsPersistent(unityObject))
                {
                    HashSet<string> assetPaths = new HashSet<string>();
                    assetPaths.Add(AssetDatabase.GetAssetPath(unityObject));
                    UnityEngine.Object[] depen = EditorUtility.CollectDependencies(new UnityEngine.Object[] { unityObject });
                    foreach (var item in depen)
                    {
                        //忽略脚本
                        if (item is MonoScript)
                        {
                            continue;
                        }
                        string assetPath = AssetDatabase.GetAssetPath(item);
                        if (assetPaths.Add(assetPath))
                        {
                            if (objectReferences.dependencies == null)
                            {
                                objectReferences.dependencies = new List<ObjectReferences>();
                            }
                            objectReferences.dependencies.Add(new ObjectReferences() { obj = item });
                        }
                    }
                }
                typeReferences.fields.Add(fieldReferences);
            }
            else if (obj is IDictionary)
            {
                IDictionary dictionary = (obj as IDictionary);
                foreach (var key in dictionary.Keys)
                {
                    SearchProperties(key, fieldReferences, typeReferences);
                    SearchProperties(dictionary[key], fieldReferences, typeReferences);
                }
            }
            else if (obj is IEnumerable)
            {
                foreach (object child in (obj as IEnumerable))
                {
                    SearchProperties(child, fieldReferences, typeReferences);
                }
            }
            else if (obj is System.Object)
            {
                if (!obj.GetType().IsValueType)
                {
                    FieldInfo[] fieldInfos = obj.GetType().GetFields();
                    var fieldStack = fieldReferences.fieldStack;
                    foreach (FieldInfo fieldInfo in fieldInfos)
                    {
                        object o = fieldInfo.GetValue(obj);
                        if (o != obj)
                        {
                            FieldReferences field = new FieldReferences() { fieldStack = new List<FieldInfo>(fieldStack) };
                            field.fieldStack.Add(fieldInfo);
                            SearchProperties(fieldInfo.GetValue(obj), field, typeReferences);
                        }
                    }
                }
            }
        }
    }

    public class TypeReferences
    {
        public bool foundObject = false;
        public Type type;
        public HashSet<FieldReferences> fields = new HashSet<FieldReferences>();
    }

    public class FieldReferences
    {
        public List<FieldInfo> fieldStack = new List<FieldInfo>();
        public HashSet<ObjectReferences> objects = new HashSet<ObjectReferences>();
    }

    public class ObjectReferences : IEquatable<ObjectReferences>
    {
        public Object obj;
        public List<ObjectReferences> dependencies;

        public bool Equals(ObjectReferences other)
        {
            return other.obj == obj;
        }

        public override int GetHashCode()
        {
            return obj.GetHashCode();
        }
    }
}

