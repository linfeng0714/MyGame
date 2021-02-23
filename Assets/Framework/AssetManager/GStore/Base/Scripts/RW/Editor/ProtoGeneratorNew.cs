//using GStore.Persister;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using UnityEditor;
//using UnityEngine;

//public static class ProtoGeneratorNew
//{
//    static string GeneratePath = string.Format(Application.dataPath + "/Gstore/BattleEngine/Tool_BehaviorTree/AutoCode_Proto.cs");

//    static Dictionary<string, List<Type>> nameSpaceType = new Dictionary<string, List<Type>>();
//    static Dictionary<Type, string> typeStringDict = new Dictionary<Type, string>();

//    static ProtoGeneratorNew()
//    {
//        typeStringDict.Add(typeof(byte), "Int8");
//        typeStringDict.Add(typeof(short), "Int16");
//        typeStringDict.Add(typeof(int), "Int32");
//        typeStringDict.Add(typeof(long), "Int64");
//        typeStringDict.Add(typeof(string), "String");
//        typeStringDict.Add(typeof(float), "Float");
//        typeStringDict.Add(typeof(double), "Double");
//        typeStringDict.Add(typeof(bool), "Bool");
//        typeStringDict.Add(typeof(UnityEngine.Vector3), "Vector3");
//    }

//    [MenuItem("GStore/BattleEngine/代码生成/生成序列化代码")]
//    public static void GenerateProtoAutoCode()
//    {
//        CollectAllProtoClass();

//        StringBuilder sb = new StringBuilder();
//        sb.Append(GenerateNewClass());

//        foreach (string nameSpace in nameSpaceType.Keys)
//        {
//            string nameSpaceClass = "";
//            foreach (Type type in nameSpaceType[nameSpace])
//            {
//                nameSpaceClass += GenerateAClass(type);
//            }

//            sb.Append(string.Format(
//@"namespace {0}
//{{
//{1}
//}}

//", nameSpace,nameSpaceClass)
//);
//        }

//        System.IO.File.WriteAllText(GeneratePath, sb.ToString());
//    }

//    public static void CollectAllProtoClass()
//    {
//        Assembly assembly = Assembly.Load("Assembly-CSharp");
//        Type[] types = assembly.GetTypes();
//        foreach (Type item in types)
//        {
//            ProtoClassAttribute attri = item.RTGetAttribute<ProtoClassAttribute>(false);
//            if (attri != null)
//            {
//                string nameSpace = item.Namespace;
//                if (!nameSpaceType.ContainsKey(nameSpace))
//                {
//                    List<Type> typeList = new List<Type>();
//                    typeList.Add(item);
//                    nameSpaceType.Add(nameSpace, typeList);
//                }
//                else
//                {
//                    nameSpaceType[nameSpace].Add(item);
//                }
//            }
//        }
//    }

//    public static string GenerateNewClass()
//    {
//        string allNameSpace = "";
//        foreach (string nameSpace in nameSpaceType.Keys)
//        {
//            string allCases = "";
//            foreach (Type type in nameSpaceType[nameSpace])
//            {
//                ProtoClassAttribute attri = type.RTGetAttribute<ProtoClassAttribute>(false);
//                string aCase = string.Format(@"
//                case {0}:return new {1}();
//"
//                , attri.id, type.FullName);
//                aCase = aCase.TrimStart(Environment.NewLine.ToArray());
//                allCases += aCase;
//            }
//            allCases = allCases.TrimEnd(Environment.NewLine.ToArray());

//            string aNameSpace = string.Format(@"
//        if(nameSpace == ""{0}"")
//        {{
//            switch(id)
//            {{
//{1}
//            }}
//        }}
//"
//            , nameSpace, allCases);
//            aNameSpace = aNameSpace.TrimStart(Environment.NewLine.ToArray());
//            allNameSpace += aNameSpace;
//        }

//        string generateNewClass = string.Format(@"
//using GStore.Persister;

//public static class PersistenceTool
//{{
//    public static Persistence GetPersistence(string nameSpace,int id)
//    {{
//{0}            
//        return null;
//    }}
//}}"
//        , allNameSpace);
//        return generateNewClass;
//    }

//    public static string GenerateAClass(Type type)
//    {
//        string classLine = "";
//        if (type.BaseType == typeof(System.Object) || type.BaseType == typeof(Persister.Persistence))
//        {
//            classLine = string.Format("public partial class {0} : Persistence", type.Name);
//        }
//        else
//        {
//            classLine = string.Format("public partial class {0}", type.Name);
//        }
//        ProtoClassAttribute attri = type.RTGetAttribute<ProtoClassAttribute>(false);

//        string aClass = string.Format(
//@"{0}
//{{
//    public override int ClassNameID() {{ return {1}; }}
//    public override string NameSpace() {{ return ""{2}""; }}
//{3}
//{4}
//{5}
//}}
//"
//        , classLine, attri.id,type.Namespace, GenerateOrderField(type), GenerateSwitchOrderField(type), GenerateCloneField(type));
//        return aClass;
//    }

//    public static string GenerateOrderField(Type type)
//    {
//        FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
//        string fields = "";
//        foreach (var info in infos)
//        {
//            object[] attributes = info.GetCustomAttributes(typeof(ProtoFieldAttribute), true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoFieldAttribute attr = attributes[0] as ProtoFieldAttribute;

//            if (info.FieldType.IsGenericType)
//            {
//                Type t = info.FieldType.GetGenericArguments()[0];
//                fields += string.Format(@"
//        c.protoType.{0}(c, ref this.{1},c.protoType.{3},{2}, ""{1}"");
//"
//                , GetTypeString(info.FieldType), info.Name, attr.id, GetTypeString(t)).TrimStart(Environment.NewLine.ToArray());
//            }
//            else
//            {
//                fields += string.Format(@"
//        c.protoType.{0}(c, ref this.{1},{2}, ""{1}"");
//"
//                , GetTypeString(info.FieldType), info.Name, attr.id).TrimStart(Environment.NewLine.ToArray());
//            }
//        }

//        if (type.BaseType != null)
//        {
//            object[] attributes = type.BaseType.GetCustomAttributes(typeof(ProtoClassAttribute), true);

//            if (attributes.Length > 0)
//                fields += string.Format(@"
//        c.protoType.Super(c, base.Order, fieldNum);
//"
//                ).TrimStart(Environment.NewLine.ToArray());
//        }

//        string orderFields = string.Format(@"
//    public override void Order(IOContext c, int fieldNum = -1, string fieldName = null) 
//    {{
//{0}
//    }}"
//        , fields);

//        return orderFields;
//    }

//    public static string GenerateSwitchOrderField(Type type)
//    {
//        FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
//        string allCases = "";
//        foreach (var info in infos)
//        {
//            object[] attributes = info.GetCustomAttributes(typeof(ProtoFieldAttribute), true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoFieldAttribute attr = attributes[0] as ProtoFieldAttribute;

//            if (info.FieldType.IsGenericType)
//            {
//                Type t = info.FieldType.GetGenericArguments()[0];
//                allCases += string.Format(@"
//            case {4} : c.protoType.{0}(c, ref this.{1},c.protoType.{3},{2}, ""{1}"");break;
//"
//                , GetTypeString(info.FieldType), info.Name, attr.id, GetTypeString(t), attr.id).TrimStart(Environment.NewLine.ToArray());
//            }
//            else
//            {
//                allCases += string.Format(@"
//            case {2} : c.protoType.{0}(c, ref this.{1},{2}, ""{1}"");break;
//"               , GetTypeString(info.FieldType), info.Name, attr.id).TrimStart(Environment.NewLine.ToArray());
//            }
//        }

//        string switchOrderField = string.Format(@"
//    public override void SwitchOrder(IOContext c, int fieldNum = -1, string fieldName = null) 
//    {{
//        switch (fieldNum) 
//        {{
//{0}
//            case 0: c.protoType.Super(c, base.SwitchOrder, fieldNum); break;
//            default: c.protoType.SkipField(c); break;
//        }}
//    }}"
//        , allCases);
//        return switchOrderField;
//    }

//    public static string GenerateCloneField(Type type)
//    {
//        FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
//        string allFieldClone = "";
//        foreach (var info in infos)
//        {
//            allFieldClone += string.Format(@"
//        clone.{0} = {0};
//",info.Name).TrimStart(Environment.NewLine.ToArray()); 
//        }

//        string cloneFun = string.Format(@"
//    public override Persistence Clone()
//    {{
//        {0} clone = new {0}();
//{1}
//        return clone;
//    }}
//",type.FullName,allFieldClone);

//        return cloneFun;
//    }

//    public static string GetTypeString(Type fieldType)
//    {
//        if (fieldType.IsValueType)
//        {
//            if (fieldType.IsEnum)
//            {
//                return "Enum";
//            }

//            string typeString = null;
//            typeStringDict.TryGetValue(fieldType, out typeString);
//            return typeString;
//        }
//        else if (fieldType.IsGenericType)
//        {
//            return "List";
//        }
//        else if (fieldType.IsClass)
//        {
//            if (fieldType == typeof(string))
//                return "String";

//            return "Object";
//        }

//        return null;

//    }



//}
