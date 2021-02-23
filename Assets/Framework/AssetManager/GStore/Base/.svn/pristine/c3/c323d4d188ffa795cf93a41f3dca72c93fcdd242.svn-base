//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Reflection;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml.Serialization;

//public static class ProtoGenerator
//{
//    //static string savaPath = "Assets/";

//    static Dictionary<Type,string> typeStringDict = new Dictionary<Type, string>();

//    static Dictionary<string,int> stringPool = new Dictionary<string,int>();

//    static ProtoGenerator()
//    {
//        typeStringDict.Add(typeof(byte), "Int8");
//        typeStringDict.Add(typeof(short), "Int16");
//        typeStringDict.Add(typeof(int),"Int32");
//        typeStringDict.Add(typeof(long), "Int64");
//        typeStringDict.Add(typeof(string), "String");
//        typeStringDict.Add(typeof(float), "Float");
//        typeStringDict.Add(typeof(double), "Double");
//        typeStringDict.Add(typeof(bool), "Bool");
//        typeStringDict.Add(typeof(Vector3), "Vector3");
//    }

//    [Serializable]
//    public class Test
//    {
//        [SerializeField]
//        public int a = 0;
//        [SerializeField]
//        public float b = 0.0f;
//        [SerializeField]
//        public string c = "SSSWWWWAAS";
//        public float d = 23423.0f;
//    }

//    [MenuItem("GStore/Step1")]
//    public static void Step1()
//    {
//        var xmlSerializer = new BinaryFormatter();

//        var obj = new Test();
//        obj.a = 123456;
//        obj.b = 548.3454f;

//        //Stream s = new MemoryStream();

//        string filepath = Path.Combine(Application.dataPath, "Serilize.txt");
//        FileStream s = File.Open(filepath, FileMode.OpenOrCreate);

//        xmlSerializer.Serialize(s,obj);

//        s.Flush();
//        s.Close();

//        //s.Position = 0;
//        //var sr = new StreamReader(s);
//        //string c = sr.ReadToEnd();

//        //Debug.Log(c);



//        //BinaryFormatter

//    }


//    /// 获取一个命名空间下的所有类
//    public static List<Type> GetTypes(string spacename)
//    {
//        List<Type> lt = new List<Type>();

//        try
//        {
//            foreach (var item in Assembly.Load("Assembly-CSharp").GetTypes())
//            {
//                //if (!string.IsNullOrEmpty(item.Namespace) && item.Namespace.StartsWith(spacename))
//                //{
//                    lt.Add(item);
//                //}
//            }
//        }
//        catch {

//        }

//        return lt;
//    }

//    [MenuItem("GStore/Are you Ok!")]
//    public static void Generate()
//    {
//        Debug.Log("OK!");

//        stringPool.Clear();

//        string spacename = "ActorAnimatorSystem";

//        var types = GetTypes(spacename);

//        string filepath = Path.Combine(Path.Combine(Application.dataPath,"../"),"Serilize.cs");
//        FileStream fs = File.Open(filepath, FileMode.OpenOrCreate);

//        var sw = new StreamWriter(fs);

//        var typeList = new List<Type>();

//        foreach (var type in types)
//        {
//            object[] attributes = type.GetCustomAttributes(typeof(ProtoClassAttribute),true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoClassAttribute attr = attributes[0] as ProtoClassAttribute;

//            FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
//            if (infos.Length < 1)
//                continue;

//            bool hasAny = false;

//            foreach(var i in infos)
//            {
//                object[] attrs = i.GetCustomAttributes(typeof(ProtoFieldAttribute), true);

//                if (attrs.Length > 0)
//                {
//                    hasAny = true;
//                    break;
//                }
//            }

//            if (hasAny)
//                typeList.Add(type);
//        }

//        GenerateFileHead(sw, spacename, typeList);

//        for (int i = 0; i < typeList.Count; i++)
//        {
//            Type t = typeList[i];
//            GenerateClass(sw, t);
//        }

//        GenerateFileTrail(sw);

//        stringPool.Clear();

//        sw.Flush();
//        sw.Close();
   
//    }

//    static public void GenerateFileHead(StreamWriter sw,string spacename,List<Type> types)
//    {
//        sw.WriteLine("using GStore.Persister;\n");
//        sw.WriteLine(string.Format("namespace {0}",spacename));
//        sw.WriteLine("{");
//        sw.WriteLine("    static public class TypeInfo {");
//        sw.WriteLine("      public static string Assembly(int id) {");
        
//        sw.WriteLine("          switch (id) { ");

//        foreach (var type in types)
//        {
//            object[] attributes = type.GetCustomAttributes(typeof(ProtoClassAttribute), true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoClassAttribute attr = attributes[0] as ProtoClassAttribute;

//            string assemblyName = type.Assembly.GetName().Name;
//            string className = type.FullName;

//            if (!stringPool.ContainsKey(assemblyName))
//            {
//                sw.WriteLine(string.Format("            case {0}: return \"{1}\";",stringPool.Count + 1, assemblyName));
//                stringPool.Add(assemblyName,stringPool.Count + 1);
//            }

//            if (!stringPool.ContainsKey(className))
//            {
//                sw.WriteLine(string.Format("            case {0}: return \"{1}\";", stringPool.Count + 1, className));
//                stringPool.Add(className, stringPool.Count + 1);
//            }

//            //sw.WriteLine(string.Format("            case {0}: return new {1}();",attr.id,type.FullName));
//        }
        
//        sw.WriteLine("          } ");
//        sw.WriteLine("          return null;");
//        sw.WriteLine("      }");
//        sw.WriteLine("    }");
        
//    }

//    static public void GenerateFileTrail(StreamWriter sw)
//    {
//        sw.WriteLine("}");
//    }

//    static public void GenerateClass(StreamWriter sw, Type type)
//    {
//        if (type.BaseType == typeof(System.Object) || type.BaseType == typeof(Persister.Persistence))
//        {
//            sw.WriteLine(string.Format("    public partial class {0} : Persistence {{", type.Name));
//        }
//        else {
//            sw.WriteLine(string.Format("    public partial class {0}  {{", type.Name));
//        }

//        int classId = -1;
//        int assemblyId = -1;

//        stringPool.TryGetValue(type.FullName,out classId);
//        stringPool.TryGetValue(type.Assembly.GetName().Name, out assemblyId);

//        //sw.WriteLine(string.Format("      public override int ClassID() {{ return {0}; }}", attr.id));
//        sw.WriteLine(string.Format("      public override int ClassNameID() {{ return {0}; }}", classId/*attr.id*/));
//        sw.WriteLine(string.Format("      public override int AssemblyNameID() {{ return {0}; }}", assemblyId/*attr.id*/));

//        GenerateOrderField(sw,type);
//        GenerateSwitchOrderField(sw, type);
//        sw.WriteLine("    }");
//    }

//    static public void GenerateOrderField(StreamWriter sw, Type type)
//    {
//        sw.WriteLine("      public override void Order(IOContext c, int fieldNum = -1, string fieldName = null) {");
//        sw.WriteLine("        c.BegineClass(ClassID());");


//        FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

//        foreach (var info in infos)
//        {
//            object[] attributes = info.GetCustomAttributes(typeof(ProtoFieldAttribute), true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoFieldAttribute attr = attributes[0] as ProtoFieldAttribute;
//            //sw.WriteLine("       Field:" + info.FieldType.Name + " - " + info.Name);

//            if (info.FieldType.IsGenericType)
//            {
//                Type t = info.FieldType.GetGenericArguments()[0];
//                sw.WriteLine(string.Format("        c.protoType.{0}(c, ref this.{1},c.protoType.{3},{2}, \"{1}\");", GetTypeString(info.FieldType), info.Name, attr.id, GetTypeString(t)));
//            }
//            else
//            {
//                sw.WriteLine(string.Format("        c.protoType.{0}(c, ref this.{1},{2}, \"{1}\");", GetTypeString(info.FieldType), info.Name, attr.id));
//            }  
//        }

//        if (type.BaseType != null)
//        {
//            object[] attributes = type.BaseType.GetCustomAttributes(typeof(ProtoClassAttribute), true);

//            if(attributes.Length > 0)
//                sw.WriteLine(string.Format("        c.protoType.Super(c, base.Order, fieldNum);"));
//        }

//        sw.WriteLine("      }");
//    }

//    static public void GenerateSwitchOrderField(StreamWriter sw, Type type)
//    {
//        sw.WriteLine("      public override void SwitchOrder(IOContext c, int fieldNum = -1, string fieldName = null) {");
//        sw.WriteLine("          c.BegineClass(ClassID());");
//        sw.WriteLine("          switch (fieldNum) {");
        
//        FieldInfo[] infos = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

//        foreach (var info in infos)
//        {
//            object[] attributes = info.GetCustomAttributes(typeof(ProtoFieldAttribute), true);

//            if (attributes.Length == 0)
//                continue;

//            ProtoFieldAttribute attr = attributes[0] as ProtoFieldAttribute;

//            if (info.FieldType.IsGenericType)
//            {
//                Type t = info.FieldType.GetGenericArguments()[0];
//                sw.WriteLine(string.Format("            case {4} : c.protoType.{0}(c, ref this.{1},c.protoType.{3},{2}, \"{1}\");break;", GetTypeString(info.FieldType), info.Name, attr.id, GetTypeString(t),attr.id));
//            }
//            else
//            {
//                sw.WriteLine(string.Format("            case {2} :c.protoType.{0}(c, ref this.{1},{2}, \"{1}\");break;", GetTypeString(info.FieldType), info.Name, attr.id));
//            }
//        }

//        sw.WriteLine("            case 0: { c.protoType.Super(c, base.SwitchOrder, fieldNum); break; }");
//        sw.WriteLine("            default: c.protoType.SkipField(c); break;");
//        sw.WriteLine("          }");
//        sw.WriteLine("      }");
//    }

//    static public string GetTypeString(Type fieldType)
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