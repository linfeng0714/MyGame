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
        public class WriteXMLContext : RWContext
        {
            public XmlDocument xmlDoc = new XmlDocument();
            public XmlElement xmlParentElement = null;
            public Stack<XmlElement> endXmlParentElementStack = new Stack<XmlElement>();
        }

        public class XMLWriter : ReadOrWriteBase
        {
            public WriteXMLContext env;

            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
            {
                base.Int8(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
            {
                base.Int16(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Int32(ref Int32 v, int fieldNum = -1, string fieldName = null)
            {
                base.Int32(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;

                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
            {
                base.Int64(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
            {
                base.Double(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
            {
                base.Float(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
            {
                base.Bool(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
            {
                base.String(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v);
                }
            }

            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
            {
                base.Bytes(ref v, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    if (v != null)
                    {
                        string b64 = Convert.ToBase64String(v);
                        xmlParentElement.SetAttribute(fieldName, b64);
                    }
                }
            }

            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
            {
                base.Vector2(ref v, fieldNum, fieldName);
                if (v == null)
                    return;
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    XMLTool.SetAttributeVector2(xmlParentElement, fieldName, v);
                }
            }

            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
            {
                base.Vector3(ref v, fieldNum, fieldName);
                if (v == null)
                    return;
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    XMLTool.SetAttributeVector3(xmlParentElement, fieldName, v);
                }
            }

            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
            {
                base.VInt_2(ref v, fieldNum, fieldName);
                if (v == null)
                    return;
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
            {
                base.VInt_3(ref v, fieldNum, fieldName);
                if (v == null)
                    return;
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    xmlParentElement.SetAttribute(fieldName, v.ToString());
                }
            }

            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
            {
                base.Color(ref v, fieldNum, fieldName);
                if (v == null)
                    return;
                XmlElement xmlParentElement = env.xmlParentElement;
                if (fieldName != null)
                {
                    XMLTool.SetAttributeColor(xmlParentElement, fieldName, v);
                }
            }

            public override void Super(ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
            {
                base.Super(func, fieldNum, fieldName);
                func(env, fieldNum);
            }

            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
            {
                base.Object(ref v, fieldNum, fieldName);
                if (v == null) return;
                SetHead(fieldNum, fieldName);
                string nameSpace = v.GetType().Namespace;
                string className = v.GetType().FullName.ToString();
                if (string.IsNullOrEmpty(nameSpace))
                {
                    nameSpace = "NoNameSpace";
                }
                env.xmlParentElement.SetAttribute("__nameSpace", nameSpace);
                env.xmlParentElement.SetAttribute("__classID", v.ClassNameID().ToString());
                env.xmlParentElement.SetAttribute("__className", className);
                if (v != null)
                {
                    v.Order(env, fieldNum);
                }
                SetEnd(fieldNum, fieldName);
            }

            public override void List<T>(ref List<T> v, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
            {
                base.List(ref v, func, fieldNum, fieldName);
                XmlElement xmlParentElement = env.xmlParentElement;
                Type t = typeof(T);
                SetHead(fieldNum, fieldName);
                env.xmlParentElement.SetAttribute("__type", "List");
                if (v != null)
                {
                    if (t.IsValueType)
                    {
                        StringBuilder builder = new StringBuilder();

                        for (int i = 0; i < v.Count; i++)
                        {
                            T item = v[i];

                            builder.Append(item.ToString());
                            builder.Append("|");
                        }
                        string content = builder.ToString();
                        env.xmlParentElement.SetAttribute("items", content.TrimEnd('|'));
                    }
                    else
                    {
                        for (int i = 0; i < v.Count; i++)
                        {
                            SetHead(-1, "item");
                            T item = v[i];
                            func(ref item, -1, "ListObject");
                            SetEnd(-1, "item");
                        }
                    }
                }
                SetEnd(fieldNum, fieldName);
            }

            public override bool SetHead(int fieldNum = -1, string fieldName = null)
            {
                base.SetHead(fieldNum, fieldName);
                XmlElement objectXmlElement = env.xmlParentElement;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    objectXmlElement = env.xmlDoc.CreateElement(fieldName);
                    env.xmlParentElement.AppendChild(objectXmlElement);
                    env.endXmlParentElementStack.Push(env.xmlParentElement);
                    env.xmlParentElement = objectXmlElement;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override void SetEnd(int fieldNum = -1, string fieldName = null)
            {
                base.SetEnd(fieldNum, fieldName);

                if (!string.IsNullOrEmpty(fieldName))
                {
                    env.xmlParentElement = env.endXmlParentElementStack.Pop();
                }
            }

        }

        public WriteXMLContext wsc = new WriteXMLContext();
        public XMLWriter typeWriter = new XMLWriter();

        public override RWContext ResetWrite()
        {
            wsc.xmlDoc.RemoveAll();
            XmlDeclaration declaration = wsc.xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement rootNode = wsc.xmlDoc.CreateElement("root");
            wsc.xmlDoc.AppendChild(declaration);
            wsc.xmlDoc.AppendChild(rootNode);
            wsc.xmlParentElement = rootNode;
            typeWriter.env = wsc;
            wsc.rwType = typeWriter;
            rsc.isReadContext = false;
            return wsc;
        }

        public override byte[] WriteToBytes()
        {
            ByteArray byteArray = new ByteArray();
            wsc.xmlDoc.Save(byteArray);
            byte[] bytes = byteArray.ReadData();
            wsc.xmlDoc.RemoveAll();
            return bytes;
        }

        public override void WriteToStream(Stream stream)
        {
            wsc.xmlDoc.Save(stream);
            wsc.xmlDoc.RemoveAll();
        }

        public override void WriteToFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            wsc.xmlDoc.Save(path);
            wsc.xmlDoc.RemoveAll();
        }

        public override void WriteObjectToStream(RWBaseObject o, Stream stream)
        {
            ResetWrite();
#if SerializeLOG
            Debug.LogWarningFormat("Begin Write");
#endif
            typeWriter.Object(ref o, 1, "object");
#if SerializeLOG
            Debug.LogWarningFormat("End Write");
#endif
            WriteToStream(stream);
        }

        public override byte[] WriteObjectToBytes(RWBaseObject o)
        {
            ResetWrite();
#if SerializeLOG
            Debug.LogWarningFormat("Begin Write");
#endif
            typeWriter.Object(ref o, 1, "object");
#if SerializeLOG
            Debug.LogWarningFormat("End Write");
#endif
            return WriteToBytes();
        }
    }
}
