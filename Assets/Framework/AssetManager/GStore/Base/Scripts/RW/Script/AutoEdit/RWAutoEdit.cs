using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections;

namespace GStore.RW
{
    public partial class RWAutoEdit : RWBaseObject
    {
        /// <summary>
        /// 是否获取反射属性
        /// </summary>
        public static bool IsUseReflectAttri = true;
#if UNITY_EDITOR
        protected List<FieldInfo> tagFieldList;
#endif
        public RWAutoEdit()
        {
            if (!IsUseReflectAttri) return;
#if UNITY_EDITOR
            //ProfilerTool.BeginSample("RWXmlBase:.ctor");
            tagFieldList = new List<FieldInfo>();

            //ProfilerTool.BeginSample("RWXmlBase:.GetFields");
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            //ProfilerTool.EndSample();

            foreach (FieldInfo item in fields)
            {
                //ProfilerTool.BeginSample("item.RTGetAttribute");
                RWAutoFieldAttribute rw = item.RTGetAttribute<RWAutoFieldAttribute>(false);
                //ProfilerTool.EndSample();

                if (rw != null)
                {
                    tagFieldList.Add(item);
                }
            }
            //ProfilerTool.EndSample();
#endif
        }

        #region 编辑器
#if UNITY_EDITOR
        /// <summary>
        /// 绘制场景
        /// </summary>
        /// <param name="sceneview"></param>
        public virtual void OnSceneGUI()
        { }
        /// <summary>
        /// 绘制节点属性
        /// </summary>
        public virtual void ShowInspector()
        {
            foreach (FieldInfo item in tagFieldList)
            {
                try
                {
                    ShowInsByFieldInfo(item);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Type {0} FieldName {1} Exception {2}", this.GetType(), item.Name, e);
                }

            }
        }

        public bool IntToBool(int intValue)
        {
            bool boolValue = false;
            if (intValue == 0)
            {
                boolValue = false;
            }
            else
            {
                boolValue = true;
            }
            return boolValue;
        }

        public int BoolToInt(bool boolValue)
        {
            int intValue = 0;
            if (!boolValue)
            {
                intValue = 0;
            }
            else
            {
                intValue = 1;
            }
            return intValue;
        }

        private void ShowInsByFieldInfo(FieldInfo fieldInfo, bool isIf = true)
        {
            FieldInfo item = fieldInfo;

            RWAutoFieldAttribute rw = item.RTGetAttribute<RWAutoFieldAttribute>(false);

            if (isIf)
            {
                if (!rw.isShow) return;
            }

            GUIContent contet = new GUIContent(rw.des, rw.tips);
            if (rw.nodeInsp == EditType.Null)
            {
                if (item.FieldType.IsEnum)
                {
                    rw.nodeInsp = EditType.EnumList;
                }
                if (item.FieldType == typeof(bool))
                {
                    rw.nodeInsp = EditType.Bool;
                }
                if (item.FieldType == typeof(int))
                {
                    rw.nodeInsp = EditType.EnumList;
                }
                if (item.FieldType == typeof(int))
                {
                    rw.nodeInsp = EditType.Int;
                }
                if (item.FieldType == typeof(float))
                {
                    rw.nodeInsp = EditType.Float;
                }
                if (item.FieldType == typeof(string))
                {
                    rw.nodeInsp = EditType.String;
                }
                if (item.FieldType == typeof(Vector2))
                {
                    rw.nodeInsp = EditType.Vector2;
                }
                if (item.FieldType == typeof(Vector3))
                {
                    rw.nodeInsp = EditType.Vector3;
                }
                if (item.FieldType == typeof(VInt2))
                {
                    rw.nodeInsp = EditType.VInt2;
                }
                if (item.FieldType == typeof(VInt3))
                {
                    rw.nodeInsp = EditType.VInt3;
                }
                if (item.FieldType == typeof(Color))
                {
                    rw.nodeInsp = EditType.Color;
                }
            }
            if (rw.nodeInsp == EditType.EnumList)
            {
                if (item.FieldType.IsEnum)
                {
                    EnmuGUIData data = GetEnumGUIData(item.FieldType);
                    item.SetValue(this, UnityEditor.EditorGUILayout.IntPopup(rw.des, (int)item.GetValue(this), data.strs, data.ints));
                }
            }
            else if ((rw.nodeInsp == EditType.Int))
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.IntField(contet, (int)item.GetValue(this)));
            }
            else if ((rw.nodeInsp == EditType.Float))
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.FloatField(contet, (float)item.GetValue(this)));
            }
            else if ((rw.nodeInsp == EditType.Bool))
            {
                if (item.FieldType == typeof(int))
                {
                    item.SetValue(this, BoolToInt(UnityEditor.EditorGUILayout.Toggle(contet, IntToBool((int)item.GetValue(this)))));
                }
                else if (item.FieldType == typeof(bool))
                {
                    item.SetValue(this, UnityEditor.EditorGUILayout.Toggle(contet, (bool)item.GetValue(this)));
                }
            }
            else if (rw.nodeInsp == EditType.Vector2)
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.Vector2Field(contet, (Vector2)item.GetValue(this)));
            }
            else if (rw.nodeInsp == EditType.Vector3)
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.Vector3Field(contet, (Vector3)item.GetValue(this)));
            }
            else if (rw.nodeInsp == EditType.String)
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.TextField(rw.des, (string)item.GetValue(this)));
            }
            else if (rw.nodeInsp == EditType.VInt3)
            {
                item.SetValue(this, VInt3.EditorGUIVInt3Field(contet, (VInt3)item.GetValue(this)));
            }
            else if (rw.nodeInsp == EditType.VInt2)
            {
                item.SetValue(this, VInt2.EditorGUIVInt2Field(contet, (VInt2)item.GetValue(this)));
            }
            else if (rw.nodeInsp == EditType.Color)
            {
                item.SetValue(this, UnityEditor.EditorGUILayout.ColorField(contet, (Color)item.GetValue(this)));
            }
        }

        /// <summary>
        /// 根据字段名显示该字段的编辑
        /// </summary>
        /// <param name="name"></param>
        protected void ShowInsByFieldName(string name)
        {
            FieldInfo fieldInfo = this.GetType().GetField(name);
            //Debug.LogErrorFormat("fieldInfo {0} this.GetType() {1}", fieldInfo != null, this.GetType());
            if (fieldInfo != null)
            {
                ShowInsByFieldInfo(fieldInfo, false);
            }
        }

        /// <summary>
        /// 获取枚举值的注释
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string GetEnumStr(System.Object obj)
        {
            System.Type typeClass = obj.GetType();
            if (typeClass.IsEnum)
            {
                var fileInfo = typeClass.GetField(obj.ToString());
                RWEnumAttribute[] aienum = (RWEnumAttribute[])fileInfo.GetCustomAttributes(typeof(RWEnumAttribute), false);
                if (aienum != null && aienum.Length > 0)
                {
                    return aienum[0].des;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取枚举类型的描述string[]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected EnmuGUIData GetEnumGUIData(Type type)
        {
            List<string> strList = new List<string>();
            List<int> intList = new List<int>();
            EnmuGUIData data;
            if (type.IsEnum)
            {
                FieldInfo[] enumList = type.GetFields();

                for (int i = 1; i < enumList.Length; i++)
                {
                    RWEnumAttribute attri = enumList[i].RTGetAttribute<RWEnumAttribute>(false);
                    if (attri != null)
                    {
                        //Debug.LogErrorFormat("des {0} int {1}", attri.des,(int)enumList[i].GetValue(this));
                        strList.Add(attri.des);
                        intList.Add((int)enumList[i].GetValue(this));
                    }
                }
            }
            data.strs = strList.ToArray();
            data.ints = intList.ToArray();
            return data;
        }
#endif
        #endregion
    }

#if UNITY_EDITOR
    public struct EnmuGUIData
    {
        public string[] strs;
        public int[] ints;
    }
#endif 
}
