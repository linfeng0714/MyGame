using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GStore.RW
{
    public partial class RWBinaryOrder : IReaderAndWriter
    {
        public class WriteBinaryContext : RWContext
        {
            public ByteArray byteArray = null;
        }

        public class BinaryWriter : ReadOrWriteBase
        {
            
            public WriteBinaryContext env;

            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
            {
               ByteArray byteArray = env.byteArray;
                byteArray.WriteInt8(v);
                base.Int8(ref v, fieldNum, fieldName);
            }

            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteInt16(v);
                base.Int16(ref v, fieldNum, fieldName);
            }

            public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteInt32(v);
                base.Int32(ref v, fieldNum, fieldName);
            }

            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteInt64(v);
                base.Int64(ref v, fieldNum, fieldName);
            }

            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteDouble(v);
                base.Double(ref v, fieldNum, fieldName);
            }

            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteFloat(v);
                base.Float(ref v, fieldNum, fieldName);
            }

            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteBoolean(v);
                base.Bool(ref v, fieldNum, fieldName);
            }

            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;

                if (v == null || v == "")
                {
                    byteArray.WriteInt8(NULL);
                }
                else
                {
                    byteArray.WriteInt8(NOTNULL);
                    byteArray.WriteString(v);
                }
                
                base.String(ref v, fieldNum, fieldName);
            }

            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                if (v == null)
                {
                    byteArray.WriteInt8(NULL);
                }
                else
                {
                    byteArray.WriteInt8(NOTNULL);
                    byteArray.WriteBytes(v);
                }
               
                base.Bytes(ref v, fieldNum, fieldName);
            }

            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteFloat(v.x);
                byteArray.WriteFloat(v.y);
                base.Vector2(ref v, fieldNum, fieldName);
            }

            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteFloat(v.x);
                byteArray.WriteFloat(v.y);
                byteArray.WriteFloat(v.z);
                base.Vector3(ref v, fieldNum, fieldName);
            }

            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteInt32(v.x);
                byteArray.WriteInt32(v.y);
                base.VInt_2(ref v, fieldNum, fieldName);
            }

            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteInt32(v.x);
                byteArray.WriteInt32(v.y);
                byteArray.WriteInt32(v.z);
                base.VInt_3(ref v, fieldNum, fieldName);
            }

            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byteArray.WriteFloat(v.r);
                byteArray.WriteFloat(v.g);
                byteArray.WriteFloat(v.b);
                byteArray.WriteFloat(v.a);
                base.Color(ref v, fieldNum, fieldName);
            }

            public override void Super(ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
            {
                base.Super(func, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                func(env, fieldNum);
            }

            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
            {
                base.Object(ref v, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                if (v == null)
                {
                    byteArray.WriteInt8(NULL);
                }
                else
                {
                    byteArray.WriteInt8(NOTNULL);
                    string nameSpace = typeof(T).Namespace;
                    if (string.IsNullOrEmpty(nameSpace))
                    {
                        nameSpace = NoNameSpace;
                    }
                    byteArray.WriteString(nameSpace);
                    byteArray.WriteInt16((short)v.ClassNameID());
                    byteArray.WriteString(v.GetType().FullName);
                    v.Order(env, -1);
                }
            }

            public override void List<T>(ref List<T> v, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
            {
                base.List(ref v, func, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                if (v != null)
                {
                    byteArray.WriteInt32(v.Count);
                    for (int i = 0; i < v.Count; i++)
                    {
                        T value = v[i];
                        func(ref value, -1);
                    }
                }
                else
                {
                    byteArray.WriteInt32(0);
                }
            }

            public override bool SetHead(int fieldNum = -1, string fieldName = null)
            {
                return true;
            }
        }

        WriteBinaryContext wsc = new WriteBinaryContext();
        BinaryWriter typeWriter = new BinaryWriter();

        public override RWContext ResetWrite()
        {
            wsc.byteArray = new ByteArray();

            wsc.isEditor = false;
            typeWriter.env = wsc;
            wsc.rwType = typeWriter;
            rsc.isReadContext = false;
            return wsc;
        }

        public override void WriteObjectToStream(RWBaseObject o, Stream stream)
        {
            ResetWrite();
#if SerializeLOG
            Debug.LogWarningFormat("Begin Write");
#endif
            typeWriter.Object(ref o);
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
            typeWriter.Object(ref o);
#if SerializeLOG
            Debug.LogWarningFormat("End Write");
#endif
            return WriteToBytes();
        }


        public override byte[] WriteToBytes()
        {
            byte[] bytes = wsc.byteArray.ReadData();
            return bytes;
        }

        public override void WriteToStream(Stream stream)
        {
            stream.Write(wsc.byteArray.GetBuffer(), 0, wsc.byteArray.DataSize());
        }

        public override void WriteToFile(string path)
        {
            byte[] bytes = wsc.byteArray.ReadData();
            File.WriteAllBytes(path, bytes);
        }
    }
}
