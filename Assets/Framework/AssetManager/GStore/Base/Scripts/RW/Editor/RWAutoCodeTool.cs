using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GStore.RW
{
    public static class RWAutoCodeTool
    {
        static string GeneratePath
        {
            get { return Application.dataPath.Combine("GStore-Custom").Combine("Base").Combine("RW").Combine("AutoCode").Combine("RWAutoCode.cs"); }
        }


        static Dictionary<string, List<Type>> nameSpaceType = new Dictionary<string, List<Type>>();
        static Dictionary<Type, string> typeStringDict = new Dictionary<Type, string>();

        static RWAutoCodeTool()
        {
            typeStringDict.Add(typeof(byte), "Int8");
            typeStringDict.Add(typeof(short), "Int16");
            typeStringDict.Add(typeof(int), "Int32");
            typeStringDict.Add(typeof(long), "Int64");
            typeStringDict.Add(typeof(string), "String");
            typeStringDict.Add(typeof(float), "Float");
            typeStringDict.Add(typeof(double), "Double");
            typeStringDict.Add(typeof(bool), "Bool");
            typeStringDict.Add(typeof(UnityEngine.Vector2), "Vector2");
            typeStringDict.Add(typeof(UnityEngine.Vector3), "Vector3");
            typeStringDict.Add(typeof(VInt2), "VInt_2");
            typeStringDict.Add(typeof(VInt3), "VInt_3");
            typeStringDict.Add(typeof(UnityEngine.Color), "Color");
        }


        [MenuItem("GStore/序列化相关/生成序列化代码(名字)")]
        public static void GenerateProtoAutoCode()
        {
            nameSpaceType.Clear();
            CollectAllProtoClass();

            StringBuilder sb = new StringBuilder();
            sb.Append(GenerateNewClass());

            foreach (string nameSpace in nameSpaceType.Keys)
            {
                string nameSpaceClass = "";
                foreach (Type type in nameSpaceType[nameSpace])
                {
                    nameSpaceClass += GenerateAClass(type);
                }

                if (nameSpace == "NoNameSpace")
                {
                    sb.Append(string.Format(
    @"
{1}

", nameSpace, nameSpaceClass)
    );
                }
                else
                {
                    sb.Append(string.Format(
    @"
namespace {0}
{{
{1}
}}

", nameSpace, nameSpaceClass)
    );
                }

            }
            string dir = Path.GetDirectoryName(GeneratePath);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            System.IO.File.WriteAllText(GeneratePath, sb.ToString());
            AssetDatabase.Refresh();
        }

        public static void CollectAllProtoClass()
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            Type[] types = assembly.GetTypes();
            foreach (Type item in types)
            {
                if (item.IsSubclassOf(typeof(RWBaseObject)))
                {
                    string nameSpace = item.Namespace;
                    if (string.IsNullOrEmpty(nameSpace))
                    {
                        nameSpace = "NoNameSpace";
                    }
                    if (!nameSpaceType.ContainsKey(nameSpace))
                    {
                        List<Type> typeList = new List<Type>();
                        typeList.Add(item);
                        nameSpaceType.Add(nameSpace, typeList);
                    }
                    else
                    {
                        nameSpaceType[nameSpace].Add(item);
                    }
                }
            }
        }

        public static string GenerateNewClass()
        {
            string allNameSpace = "";
            foreach (string nameSpace in nameSpaceType.Keys)
            {
                string allCases = "";
                foreach (Type type in nameSpaceType[nameSpace])
                {
                    string aCase = string.Format(@"
                case ""{0}"":return new {1}();
"
                    , type.FullName, type.FullName);
                    aCase = aCase.TrimStart(Environment.NewLine.ToArray());
                    allCases += aCase;
                }
                allCases = allCases.TrimEnd(Environment.NewLine.ToArray());

                string aNameSpace = string.Format(@"
        if(nameSpace == ""{0}"")
        {{
            switch(fieldName)
            {{
{1}
            }}
        }}
"
                , nameSpace, allCases);
                aNameSpace = aNameSpace.TrimStart(Environment.NewLine.ToArray());
                allNameSpace += aNameSpace;
            }

            string generateNewClass = string.Format(@"
using GStore.RW;

public static class RWObjectCreate
{{
    public static RWBaseObject GetRWObject(string nameSpace,string fieldName,int id)
    {{
{0}            
        return null;
    }}
}}"
            , allNameSpace);
            return generateNewClass;
        }

        public static string GenerateAClass(Type type)
        {
            string classLine = "";
            classLine = string.Format(@"
    public partial class {0} : {1}", type.Name, type.BaseType).TrimStart(Environment.NewLine.ToArray());

            string aClass = string.Format(@"
{0}
    {{
        public override int ClassNameID() {{ return {1}; }}
{2}
{3}
{4}
    }}
"
            , classLine, 0, GenerateOrderField(type), GenerateSwitchOrderField(type), GenerateCloneField(type));
            return aClass;
        }

        public static string GenerateOrderField(Type type)
        {
            FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            string fields = "";
            string allFields = "";
            foreach (var info in infos)
            {
                RWAutoFieldAttribute attr = info.RTGetAttribute<RWAutoFieldAttribute>(false);
                if (attr == null)
                    continue;

                if (info.FieldType.IsGenericType)
                {
                    Type t = info.FieldType.GetGenericArguments()[0];
                    fields = string.Format(@"
            c.rwType.{0}(ref this.{1},c.rwType.{3},{2}, ""{1}"");"
                    , GetTypeString(info.FieldType), info.Name, 0, GetTypeString(t)).TrimStart(Environment.NewLine.ToArray());
                }
                else
                {
                    fields = string.Format(@"
            c.rwType.{0}(ref this.{1},{2}, ""{1}"");"
                    , GetTypeString(info.FieldType), info.Name, 0).TrimStart(Environment.NewLine.ToArray());
                }

                if (attr.isEditor)
                {
                    allFields += string.Format(@"
#if UNITY_EDITOR
            if(c.isEditor)
    {0}
#endif
", fields).TrimStart(Environment.NewLine.ToArray());
                }
                else
                {
                    allFields += fields + "\n";
                }
            }

            if (type.BaseType != null)
            {
                allFields += string.Format(@"
            c.rwType.Super(base.Order, fieldNum,""base"");
"
                    ).TrimStart(Environment.NewLine.ToArray());
            }

            string orderFields = string.Format(@"
        public override void Order(RWContext c, int fieldNum = -1, string fieldName = null) 
        {{
{0}
        }}"
            , allFields);

            return orderFields;
        }

        public static string GenerateSwitchOrderField(Type type)
        {
            FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            string acase = "";
            string allCases = "";
            foreach (var info in infos)
            {
                RWAutoFieldAttribute attr = info.RTGetAttribute<RWAutoFieldAttribute>(false);

                if (attr == null)
                    continue;

                if (info.FieldType.IsGenericType)
                {
                    Type t = info.FieldType.GetGenericArguments()[0];
                    acase = string.Format(@"
                case ""{4}"" : c.rwType.{0}(ref this.{1},c.rwType.{3},{2}, ""{1}"");break;"
                    , GetTypeString(info.FieldType), info.Name, 0, GetTypeString(t), info.Name).TrimStart(Environment.NewLine.ToArray());
                }
                else
                {
                    acase = string.Format(@"
                case ""{1}"" : c.rwType.{0}(ref this.{1},{2}, ""{1}"");break;"
                    , GetTypeString(info.FieldType), info.Name, 0).TrimStart(Environment.NewLine.ToArray());
                }

                if (attr.isEditor)
                {
                    allCases += string.Format(@"
#if UNITY_EDITOR
{0}
#endif
", acase).TrimStart(Environment.NewLine.ToArray());
                }
                else
                {
                    allCases += acase + "\n";
                }
            }

            string switchOrderField = string.Format(@"
        public override void SwitchOrder(RWContext c, int fieldNum = -1, string fieldName = null) 
        {{
            switch (fieldName) 
            {{
{0}
                case ""base"": c.rwType.Super(base.SwitchOrder, fieldNum); break;
                default: c.rwType.SkipField(); break;
            }}
        }}"
            , allCases);
            return switchOrderField;
        }

        public static string GenerateCloneField(Type type)
        {
            FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            string allFieldClone = "";
            foreach (var info in infos)
            {
                RWAutoFieldAttribute attr = info.RTGetAttribute<RWAutoFieldAttribute>(true);
                RWAutoFieldAttribute fieldArrt = info.RTGetAttribute<RWAutoFieldAttribute>(true);

                if (attr == null && fieldArrt == null)
                    continue;

                allFieldClone += string.Format(@"
            clone.{0} = {0};
", info.Name).TrimStart(Environment.NewLine.ToArray());
            }

            string cloneFun = string.Format(@"
        public override RWBaseObject GetCloneObj()
        {{
            {0} cloneObj = new {0}();
            AutoCloneData(cloneObj);
            return cloneObj;
        }}

        public override void AutoCloneData(RWBaseObject cloneObj)
        {{
            {0} clone = cloneObj as {0};
{1}
             base.AutoCloneData(clone);
        }}
", type.FullName, allFieldClone);

            return cloneFun;
        }

        public static string GetTypeString(Type fieldType)
        {
            if (fieldType.IsValueType)
            {
                if (fieldType.IsEnum)
                {
                    return "Enum";
                }

                string typeString = null;
                typeStringDict.TryGetValue(fieldType, out typeString);
                return typeString;
            }
            else if (fieldType == typeof(byte[]))
            {
                return "Bytes";
            }
            else if (fieldType.IsGenericType)
            {
                return "List";
            }
            else if (fieldType.IsClass)
            {
                if (fieldType == typeof(string))
                    return "String";

                return "Object";
            }

            return null;

        }



    } 
}
