//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//namespace GStore.Persister
//{
//    public class Varint : IPersister
//    {
//        public class ReadBinaryContext : IOContext
//        {
//            public IO.VarintArray varintArray = null;
//        }

//        public class WriteBinaryContext : IOContext
//        {
//            public IO.VarintArray varintArray = null;
//        }

//        public class BinaryWriter : IProtoType
//        {
//            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Int8 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context,fieldNum,fieldName);

//                varintArray.WriteInt8(v);
//            }

//            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Int16 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.WriteInt16(v);
//            }

//            public override void Int32(ref Int32 v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Int32 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.WriteInt32(v);
//            }

//            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Int64 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");

//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.WriteInt64(v);
//            }

//            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Double fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteDouble(v);
//            }

//            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Float fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteFloat(v);
//            }

//            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Bool fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.WriteBoolean(v);
//            }

//            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("String fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (v == null || v == "")
//                    return;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.WriteString(v);
//            }

//            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Bytes fieldNum {0} fieldName {1} length {2}", fieldNum, fieldName, v.Length); 
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.writeBytes(v);
//            }

//            public override void Vector2(ref Vector2 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//#if SerializeLOG
//                Debug.LogFormat("Vector2 fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif

//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteFloat(o.x);
//                varintArray.byteArray.WriteFloat(o.y);
//            }

//            public override void Vector3(ref Vector3 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//#if SerializeLOG
//                Debug.LogFormat("Vector3 fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif

//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteFloat(o.x);
//                varintArray.byteArray.WriteFloat(o.y);
//                varintArray.byteArray.WriteFloat(o.z);
//            }

//            public override void VInt_2(ref VInt2 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//#if SerializeLOG
//                Debug.LogFormat("VInt2 fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif

//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteInt32(o.x);
//                varintArray.byteArray.WriteInt32(o.y);
//            }

//            public override void VInt_3(ref VInt3 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//#if SerializeLOG
//                Debug.LogFormat("VInt3 fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif


//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteInt32(o.x);
//                varintArray.byteArray.WriteInt32(o.y);
//                varintArray.byteArray.WriteInt32(o.z);
//            }

//            public override void Color(ref Color o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//#if SerializeLOG
//                Debug.LogFormat("Color fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif


//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                varintArray.byteArray.WriteFloat(o.r);
//                varintArray.byteArray.WriteFloat(o.g);
//                varintArray.byteArray.WriteFloat(o.b);
//                varintArray.byteArray.WriteFloat(o.a);
//            }

//            public override void List<T>(ref List<T> list, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                if (list != null)
//                {
//                    Debug.LogFormat("Object fieldNum {0} fieldName {1} type {2} count {3}", fieldNum, fieldName, typeof(T), list.Count);
//                }
//                else
//                {
//                    Debug.LogFormat("Object fieldNum {0} fieldName {1} type {2} ", fieldNum, fieldName, typeof(T), null);
//                }
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                if (list != null)
//                {
//                    varintArray.WriteInt32(list.Count);

//                    for (int i = 0; i < list.Count; i++)
//                    {
//                        T value = list[i];
//                        func(context, ref value, -1);
//                    }
//                }
//                else
//                {
//                    varintArray.WriteInt32(0);
//                }
//            }

//            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Object fieldNum {0} fieldName {1} type {2}", fieldNum, fieldName, v.GetType()); 
//#endif
//                if (v == null)
//                    return;

//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, fieldNum, fieldName);

//                string nameSpace = typeof(T).Namespace;
//                if (string.IsNullOrEmpty(nameSpace))
//                {
//                    nameSpace = "NoNameSpace";
//                }
//                varintArray.WriteString(nameSpace);
//                varintArray.WriteInt16((short)v.ClassNameID());
//                varintArray.WriteString(v.GetType().FullName);

//                v.Order(context, -1);

//                SetEnd(context);
//            }

//            public override void Super( ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Super fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                SetHead(context, 0, BASE);

//                func(context, fieldNum);

//                SetEnd(context);
//            }

//            public override bool SetHead( int fieldNum = -1, string fieldName = null)
//            {
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                //if (fieldNum > 0)
//                //{
//                //    byteArray.WriteInt8((byte)fieldNum);
//                //}
//                if (!string.IsNullOrEmpty(fieldName))
//                {
//                    varintArray.WriteString(fieldName);
//                    return true;
//                }
//                return false;
//            }

//            public override void SetEnd( int fieldNum = -1, string fieldName = null)
//            {
//                WriteBinaryContext env = context as WriteBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                //byteArray.WriteInt8(FIELDEND_VALUE);
//                varintArray.WriteString(FIELDEND);
//            }
//        }

//        public class BinaryReader : IProtoType
//        {
//            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadInt8();
//#if SerializeLOG
//                Debug.LogFormat("Int8 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadInt16();
//#if SerializeLOG
//                Debug.LogFormat("Int16 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadInt32();
//#if SerializeLOG
//                Debug.LogFormat("Int32 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadInt64();
//#if SerializeLOG
//                Debug.LogFormat("Int64 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.byteArray.ReadDouble();
//#if SerializeLOG
//                Debug.LogFormat("Double fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.byteArray.ReadFloat();
//#if SerializeLOG
//                Debug.LogFormat("Float fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadBoolean();
//#if SerializeLOG
//                Debug.LogFormat("Bool fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                v = varintArray.ReadString();
//#if SerializeLOG
//                Debug.LogFormat("String fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;
//                varintArray.byteArray.ReadBytes(v);
//#if SerializeLOG
//                Debug.LogFormat("Bytes fieldNum {0} fieldName {1} length {2}", fieldNum, fieldName, v.Length); 
//#endif
//            }

//            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Vector2();

//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (fieldNum > 0)
//                    varintArray.WriteInt8((byte)fieldNum);

//                v.x = varintArray.byteArray.ReadFloat();
//                v.y = varintArray.byteArray.ReadFloat();
//#if SerializeLOG
//                Debug.LogFormat("Vector2 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Vector3();

//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (fieldNum > 0)
//                    varintArray.WriteInt8((byte)fieldNum);

//                v.x = varintArray.byteArray.ReadFloat();
//                v.y = varintArray.byteArray.ReadFloat();
//                v.z = varintArray.byteArray.ReadFloat();
//#if SerializeLOG
//                Debug.LogFormat("Vector3 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new VInt2();

//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (fieldNum > 0)
//                    varintArray.WriteInt8((byte)fieldNum);

//                v.x = varintArray.byteArray.ReadInt32();
//                v.y = varintArray.byteArray.ReadInt32();
//#if SerializeLOG
//                Debug.LogFormat("VInt2 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new VInt3();

//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (fieldNum > 0)
//                    varintArray.WriteInt8((byte)fieldNum);

//                v.x = varintArray.byteArray.ReadInt32();
//                v.y = varintArray.byteArray.ReadInt32();
//                v.z = varintArray.byteArray.ReadInt32();
//#if SerializeLOG
//                Debug.LogFormat("VInt3 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Color();

//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                if (fieldNum > 0)
//                    varintArray.WriteInt8((byte)fieldNum);

//                v.r = varintArray.byteArray.ReadFloat();
//                v.g = varintArray.byteArray.ReadFloat();
//                v.b = varintArray.byteArray.ReadFloat();
//                v.a = varintArray.byteArray.ReadFloat();
//#if SerializeLOG
//                Debug.LogFormat("Color fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
//#endif
//            }

//            public override void List<T>(ref List<T> list, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                int count = varintArray.ReadInt32();
//#if SerializeLOG
//                Debug.LogFormat("List<T> fieldNum {0} fieldName {1} typeof(T) {2} count {3}", fieldNum, fieldName, typeof(T), count);
//#endif

//                if (list == null)
//                    list = new List<T>();

//                for (int i = 0; i < count; i++)
//                {
//                    T value = default(T);
//                    func(context, ref value, -1);
//                    list.Add(value);
//                }
//            }

//            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                string nameSpaceName = varintArray.ReadString();
//                int classNameId = varintArray.ReadInt16();
//                string className = varintArray.ReadString();

//                if(v == null)
//                    v = (T)env.ObjectFactory(nameSpaceName, classNameId, className);
//#if SerializeLOG
//                Debug.LogFormat("Object fieldNum {0} fieldName {1} type {2}", fieldNum, fieldName, v.GetType());
//#endif
//                //int num = 0;
//                //while ((num = context.protoType.NextField(context)) >= 0)
//                //{
//                //    v.SwitchOrder(context, num);
//                //}

//                while (SetHead(context, fieldNum, fieldName))
//                {
//                    v.SwitchOrder(context, -1, headStr);
//                }
//            }

//            public override void Super( ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
//            {
//#if SerializeLOG
//                Debug.LogFormat("Object fieldNum {0} fieldName {1}", fieldNum, fieldName);
//#endif
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                //int num = 0;
//                //while ((num = context.protoType.NextField(context)) >= 0)
//                //{
//                //    func(context, num);
//                //}

//                while (SetHead(context, fieldNum, fieldName))
//                {
//                    func(context, -1, headStr);
//                }
//            }

//            public override bool SetHead( int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                headStr = varintArray.ReadString();
//                if (headStr != FIELDEND)
//                {
//                    return true;
//                }
//                return false;
//            }

//            public override void SetEnd( int fieldNum = -1, string fieldName = null)
//            {
//                ReadBinaryContext env = context as ReadBinaryContext;
//                if (env == null) throw new Exception("IOContext Error.");
//                IO.VarintArray varintArray = env.varintArray;

//                //byteArray.WriteInt8(FIELDEND_VALUE);
//                varintArray.ReadString();
//            }
//        }

//        WriteBinaryContext wsc = new WriteBinaryContext();
//        BinaryWriter typeWriter = new BinaryWriter();

//        ReadBinaryContext rsc = new ReadBinaryContext();
//        BinaryReader typeReader = new BinaryReader();

//        public override IOContext ResetWrite()
//        {
//            wsc.varintArray = new IO.VarintArray();
//            wsc.protoType = typeWriter;

//            return wsc;
//        }

//        public override IOContext ResetRead(byte[] bytes)
//        {
//            rsc.varintArray = new IO.VarintArray();

//            rsc.varintArray.byteArray.RequestBufferSize((int)bytes.Length);
//            rsc.varintArray.byteArray.SetBuffer(bytes);
//            rsc.varintArray.byteArray.SetWriteOffset((int)bytes.Length);

//            rsc.protoType = typeReader;
//            return rsc;
//        }

//        public override IOContext ResetRead(Stream stream)
//        {
//            rsc.varintArray = new IO.VarintArray();

//            rsc.varintArray.byteArray.RequestBufferSize((int)stream.Length);
//            stream.Read(rsc.varintArray.byteArray.GetBuffer(), 0, (int)stream.Length);
//            rsc.varintArray.byteArray.SetWriteOffset((int)stream.Length);

//            rsc.protoType = typeReader;
//            return rsc;
//        }

//        public override byte[] WriteToBytes()
//        {
//            byte[] bytes = wsc.varintArray.byteArray.ReadData();
//            return bytes;
//        }

//        public override void WriteToStream(Stream stream)
//        {
//            stream.Write(wsc.varintArray.byteArray.GetBuffer(), 0, wsc.varintArray.byteArray.DataSize());
//        }

//        public override void WriteToFile(string path)
//        {
//            byte[] bytes = wsc.varintArray.byteArray.ReadData();
//            File.WriteAllBytes(path, bytes);
//        }

//        public override void ReadObjectByStream<T>(Stream stream, ref T o)
//        {
//            ResetRead(stream);
//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Read");
//#endif
//            typeReader.Object(rsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Read");
//#endif
//        }

//        public override void ReadObjectByBytes<T>(byte[] bytes, ref T o)
//        {
//            ResetRead(bytes);
//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Read");
//#endif
//            typeReader.Object(rsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Read");
//#endif
//        }

//        public override void WriteObjectToStream(Persistence o, Stream stream)
//        {
//            ResetWrite();
//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Write");
//#endif
//            typeWriter.Object(wsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Write");
//#endif
//            WriteToStream(stream);
//        }

//        public override byte[] WriteObjectToBytes(Persistence o)
//        {
//            ResetWrite();
//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Write");
//#endif
//            typeWriter.Object(wsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Write");
//#endif
//            return WriteToBytes();
//        }

//        //        public static byte[] Write(Persistence o)
//        //        {
//        //            ByteArray byteArray = new ByteArray();

//        //            wsc.varintArray = new IO.VarintArray();
//        //            wsc.protoType = typeWriter;

//        //#if SerializeLOG
//        //            Debug.LogWarningFormat("Begin Write");
//        //#endif
//        //            typeWriter.Object(wsc, ref o);
//        //#if SerializeLOG
//        //            Debug.LogWarningFormat("End Write");
//        //#endif

//        //            wsc.varintArray.byteArray.Read();
//        //            byteArray.Write(wsc.varintArray.byteArray.GetBuffer(), 0, wsc.varintArray.byteArray.DataSize());
//        //            return byteArray;
//        //        }

//    }
//}
