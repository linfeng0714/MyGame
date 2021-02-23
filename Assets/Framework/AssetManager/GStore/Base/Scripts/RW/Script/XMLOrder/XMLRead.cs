using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using UnityEngine;
using System.Security;

namespace GStore.RW
{
    public partial class RWXMLOrder : IReaderAndWriter
    {
        public class ReadXMLContext : RWContext
        {
            public XmlDocument xmlDoc = new XmlDocument();

            public SecurityElementInfo xmlParentElement = null;
            public Stack<SecurityElementInfo> endXmlParentElementStack = new Stack<SecurityElementInfo>();

            public string listItemValue = "";
        }

        public class XMLReader : ReadOrWriteBase
        {
            public ReadXMLContext env;

            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetByteAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseByte(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Int8(ref v, fieldNum, fieldName);
            }

            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetShortAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseShort(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Int16(ref v, fieldNum, fieldName);
            }

            public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetIntAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseInt(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Int32(ref v, fieldNum, fieldName);
            }

            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetLongAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseLong(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Int64(ref v, fieldNum, fieldName);
            }

            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetDoubleAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseDouble(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Double(ref v, fieldNum, fieldName);
            }

            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetFloatAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseFloat(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Float(ref v, fieldNum, fieldName);
            }

            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetBoolAttribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = XMLTool.ParseBool(env.listItemValue);
                    env.listItemValue = null;
                }
                base.Bool(ref v, fieldNum, fieldName);
            }

            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.Attribute(xmlParentElement, fieldName);
                }
                else
                {
                    v = env.listItemValue;
                    env.listItemValue = null;
                }
                base.String(ref v, fieldNum, fieldName);
            }

            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                string value = null;
                value = XMLTool.Attribute(xmlParentElement, fieldName);
                if (value != null)
                {
                    v = Convert.FromBase64String(value);
                }
                base.Bytes(ref v, fieldNum, fieldName);
            }

            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetVector2Attribute(xmlParentElement, fieldName, UnityEngine.Vector2.zero);
                }
                else
                {
                    v = XMLTool.ParseVector2(env.listItemValue, UnityEngine.Vector2.zero);
                    env.listItemValue = null;
                }
                base.Vector2(ref v, fieldNum, fieldName);
            }

            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
            {
                if (v == null)
                    return;

                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetVector3Attribute(xmlParentElement, fieldName, UnityEngine.Vector3.zero);
                }
                else
                {
                    v = XMLTool.ParseVector3(env.listItemValue, UnityEngine.Vector3.zero);
                    env.listItemValue = null;
                }
                base.Vector3(ref v, fieldNum, fieldName);
            }

            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetVInt2Attribute(xmlParentElement, fieldName, new VInt2());
                }
                else
                {
                    v = XMLTool.ParseVInt2(env.listItemValue, VInt2.zero);
                    env.listItemValue = null;
                }
                base.VInt_2(ref v, fieldNum, fieldName);
            }

            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetVInt3Attribute(xmlParentElement, fieldName, new VInt3());
                }
                else
                {
                    v = XMLTool.ParseVInt3(env.listItemValue, VInt3.zero);
                    env.listItemValue = null;
                }
                base.VInt_3(ref v, fieldNum, fieldName);
            }

            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
            {
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    v = XMLTool.GetColorAttribute(xmlParentElement, fieldName, UnityEngine.Color.white);
                }
                else
                {
                    v = XMLTool.ParseColor(env.listItemValue, UnityEngine.Color.white);
                    env.listItemValue = null;
                }
                base.Color(ref v, fieldNum, fieldName);
            }

            public override void Super(ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
            {
                base.Super(func, fieldNum, fieldName);
                func(env, fieldNum);
            }

            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
            {
                base.Object(ref v, fieldNum, fieldName);
                bool isHas = SetHead(fieldNum, fieldName);
                if (!isHas)
                    return;
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                string nameSpace = xmlParentElement.Attribute("__nameSpace");
                int classId = XMLTool.GetIntAttribute(xmlParentElement, "__classID");
                string className = xmlParentElement.Attribute("__className");
                if (v == null)
                    v = (T)env.ObjectFactory(nameSpace, className, classId);
                v.Order(env, fieldNum, fieldName);
                SetEnd(fieldNum, fieldName);
            }

            public override void List<T>(ref List<T> v, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
            {
                base.List(ref v, func, fieldNum, fieldName);
                SecurityElement xmlParentElement = env.xmlParentElement.ele;
                Type t = typeof(T);
                bool isHas = SetHead(fieldNum, fieldName);
                if (!isHas)
                    return;
                if (v == null)
                    v = new List<T>();
                if (env.xmlParentElement.ele == null)
                    return;
                if (t.IsValueType)
                {
                    string content = env.xmlParentElement.ele.Attribute("items");
                    if (!string.IsNullOrEmpty(content))
                    {
                        string[] splits = content.Split('|');
                        for (int i = 0; i < splits.Length; i++)
                        {
                            env.listItemValue = splits[i];
                            T o = default(T);
                            func(ref o, -1);
                            v.Add(o);
                        }
                    }
                }
                else
                {
                    while (SetHead(-1, "item"))
                    {
                        T o = default(T);
                        func(ref o, -1, "ListObject");
                        v.Add(o);
                        SetEnd(-1, "item");
                    }
                }
                SetEnd(fieldNum, fieldName);
            }

            public override bool SetHead(int fieldNum = -1, string fieldName = null)
            {
                base.SetHead(fieldNum, fieldName);
                SecurityElementInfo xmlParentElement = env.xmlParentElement;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    SecurityElementInfo objectElement = new SecurityElementInfo();
                    if (xmlParentElement.nodeIndex.ContainsKey(fieldName))
                    {
                        XMLNodeList list = xmlParentElement.ele.GetNodeList(fieldName);
                        int i = xmlParentElement.nodeIndex[fieldName];
                        if (list != null && list.Count > i)
                        {
                            objectElement.ele = list[i];
                            i++;
                            xmlParentElement.nodeIndex[fieldName] = i;
                        }
                    }
                    else
                    {
                        objectElement.ele = xmlParentElement.ele.SearchForChildByTag(fieldName);
                        if (objectElement.ele != null)
                            xmlParentElement.nodeIndex.Add(fieldName, 1);
                    }

                    if (objectElement.ele != null)
                    {
                        env.endXmlParentElementStack.Push(env.xmlParentElement);
                        env.xmlParentElement = objectElement;
                        return true;
                    }
                }
                return false;
            }

            public override void SetEnd(int fieldNum = -1, string fieldName = null)
            {
                base.SetEnd(fieldNum,fieldName);
                if (!string.IsNullOrEmpty(fieldName))
                {
                    env.xmlParentElement = env.endXmlParentElementStack.Pop();
                }
            }
        }

        public ReadXMLContext rsc = new ReadXMLContext();
        public XMLReader typeReader = new XMLReader();

        public override RWContext ResetRead(byte[] bytes)
        {
            XMLParser xml = new XMLParser();
            xml.Parse(XMLTool.ToString(bytes));
            SecurityElement rootNode = xml.ToXml();
            SecurityElementInfo rootNodeInfo = new SecurityElementInfo();
            rootNodeInfo.ele = rootNode;
            rsc.xmlParentElement = rootNodeInfo;
            typeReader.env = rsc;
            rsc.rwType = typeReader;
            rsc.isReadContext = true;
            return rsc;
        }

        public override RWContext ResetRead(Stream byteArray)
        {
            ByteArray byteArrayStream = byteArray as ByteArray;
            byte[] bytes = new byte[byteArray.Length];
            byteArrayStream.consume = false;
            byteArrayStream.Read(bytes, 0, bytes.Length);
            byteArrayStream.consume = true;
            return ResetRead(bytes);
        }

        public override void ReadObjectByStream<T>(Stream stream, ref T o)
        {
            ByteArray byteArrayStream = stream as ByteArray;
            byte[] bytes = byteArrayStream.ReadData();
            ReadObjectByBytes(bytes, ref o);
        }

        public override void ReadObjectByBytes<T>(byte[] bytes, ref T o)
        {
            ResetRead(bytes);
#if SerializeLOG
            Debug.LogWarningFormat("Begin Read");
#endif
            typeReader.Object(ref o, 1, "object");
#if SerializeLOG
            Debug.LogWarningFormat("End Read");
#endif
            wsc.xmlDoc.RemoveAll();
        }
    }

    public class SecurityElementInfo
    {
        public Dictionary<string, int> nodeIndex = new Dictionary<string, int>();
        public SecurityElement ele;
    }
}
