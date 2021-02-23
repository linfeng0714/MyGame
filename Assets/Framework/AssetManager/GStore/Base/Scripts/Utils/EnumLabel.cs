using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;
    public int[] sortOrder = new int[0];
    public EnumLabelAttribute(string label)
    {
        this.label = label;
    }

    public EnumLabelAttribute(string label, params int[] order)
    {
        this.label = label;
        this.sortOrder = order;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    private Dictionary<string, string> customEnumNames = new Dictionary<string, string>();


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetUpCustomEnumNames(property, property.enumNames);

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            EditorGUI.BeginChangeCheck();
            string[] displayedOptions = property.enumNames
                    .Where(enumName => customEnumNames.ContainsKey(enumName))
                    .Select<string, string>(enumName => customEnumNames[enumName])
                    .ToArray();

            int[] indexArray = GetIndexArray(enumLabelAttribute.sortOrder);
            if (indexArray.Length != displayedOptions.Length)
            {
                indexArray = new int[displayedOptions.Length];
                for (int i = 0; i < indexArray.Length; i++)
                {
                    indexArray[i] = i;
                }
            }
            string[] items = new string[displayedOptions.Length];
            items[0] = displayedOptions[0];
            for (int i = 0; i < displayedOptions.Length; i++)
            {
                items[i] = displayedOptions[indexArray[i]];
            }
            int index = -1;
            for (int i = 0; i < indexArray.Length; i++)
            {
                if (indexArray[i] == property.enumValueIndex)
                {
                    index = i;
                    break;
                }
            }
            if ((index == -1) && (property.enumValueIndex != -1)) { SortingError(position, property, label); return; }
            index = EditorGUI.Popup(position, enumLabelAttribute.label, index, items);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0)
                    property.enumValueIndex = indexArray[index];
            }
        }
    }

    private EnumLabelAttribute enumLabelAttribute
    {
        get
        {
            return (EnumLabelAttribute)attribute;
        }
    }

    public void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
    {


        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false);
        foreach (EnumLabelAttribute customAttribute in customAttributes)
        {
            Type enumType = fieldInfo.FieldType;

            foreach (string enumName in enumNames)
            {
                FieldInfo field = enumType.GetField(enumName);
                if (field == null) continue;
                EnumLabelAttribute[] attrs = (EnumLabelAttribute[])field.GetCustomAttributes(customAttribute.GetType(), false);

                if (!customEnumNames.ContainsKey(enumName))
                {
                    foreach (EnumLabelAttribute labelAttribute in attrs)
                    {
                        customEnumNames.Add(enumName, labelAttribute.label);
                    }
                }
            }
        }
    }


    int[] GetIndexArray(int[] order)
    {
        int[] indexArray = new int[order.Length];
        for (int i = 0; i < order.Length; i++)
        {
            int index = 0;
            for (int j = 0; j < order.Length; j++)
            {
                if (order[i] > order[j])
                {
                    index++;
                }
            }
            indexArray[i] = index;
        }
        return (indexArray);
    }

    void SortingError(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (sorting error)"));
        EditorGUI.EndProperty();
    }
}
public class EnumLabel
{
    static public object GetEnum(Type type, SerializedObject serializedObject, string path)
    {
        SerializedProperty property = GetPropety(serializedObject, path);
        return System.Enum.GetValues(type).GetValue(property.enumValueIndex);
    }
    static public object DrawEnum(Type type, SerializedObject serializedObject, string path)
    {
        return DrawEnum(type, serializedObject, GetPropety(serializedObject, path));
    }
    static public object DrawEnum(Type type, SerializedObject serializedObject, SerializedProperty property)
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(property);
        serializedObject.ApplyModifiedProperties();
        return System.Enum.GetValues(type).GetValue(property.enumValueIndex);
    }
    static public SerializedProperty GetPropety(SerializedObject serializedObject, string path)
    {
        string[] contents = path.Split('/');
        SerializedProperty property = serializedObject.FindProperty(contents[0]);
        for (int i = 1; i < contents.Length; i++)
        {
            property = property.FindPropertyRelative(contents[i]);
        }
        return property;
    }

    public static void Popup<T>(string title, ref T selected) where T : struct
    {
        selected = Popup(title, selected);
    }

    public static T Popup<T>(string title, T selected)where T: struct
    {
        int index = 0;
        var array = Enum.GetValues(selected.GetType());
        int length = array.Length;

        string[] enumString = new string[length];
        for (int i = 0; i < length; i++)
        {
            FieldInfo[] fields = selected.GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals(array.GetValue(i).ToString()))
                {
                    object[] objs = field.GetCustomAttributes(typeof(EnumLabelAttribute), true);
                    if (objs != null && objs.Length > 0)
                    {
                        enumString[i] = ((EnumLabelAttribute)objs[0]).label;
                    }
                    else
                    {
                        enumString[i] = field.Name;
                    }
                    if (field.Name.Equals(selected.ToString()))
                    {
                        index = i;
                    }
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(title);
        index = EditorGUILayout.Popup(index, enumString);
        EditorGUILayout.EndHorizontal();

        return (T)array.GetValue(index);
    }
}
#endif