//using System;
//using System.IO;
//using System.Collections.Generic;
//using UnityEngine;

//namespace GStore.Persister
//{
//    public class Binary : IPersister
//    {
//        public class ReadBinaryContext : IOContext
//        {
//            public IO.ByteArray byteArray = null;
//        }

//        public class WriteBinaryContext : IOContext
//        {
//            public IO.ByteArray byteArray = null;
//        }

//        public class BinaryWriter : ProtoBase
//        {
//            public WriteBinaryContext env;

//            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(fieldNum, fieldName);

//                byteArray.WriteByte(v);
//            }

//            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(fieldNum, fieldName);

//                byteArray.WriteInt16(v);
//            }

//            public override void Int32(ref Int32 v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteInt32(v);
//            }

//            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);

//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteInt64(v);
//            }

//            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);

//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteDouble(v);
//            }

//            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteFloat(v);
//            }

//            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteBoolean(v);

//            }

//            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                if (v == null || v == "")
//                    return;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteString(v);
//            }

//            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteBytes(v);
//            }

//            public override void Vector2(ref Vector2 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteFloat(o.x);
//                byteArray.WriteFloat(o.y);
//            }

//            public override void Vector3(ref Vector3 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteFloat(o.x);
//                byteArray.WriteFloat(o.y);
//                byteArray.WriteFloat(o.z);
//            }

//            public override void VInt_2(ref VInt2 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteInt32(o.x);
//                byteArray.WriteInt32(o.y);
//            }

//            public override void VInt_3(ref VInt3 o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteInt32(o.x);
//                byteArray.WriteInt32(o.y);
//                byteArray.WriteInt32(o.z);
//            }

//            public override void Color(ref Color o, int fieldNum = -1, string fieldName = null)
//            {
//                if (o == null)
//                    return;
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                byteArray.WriteFloat(o.r);
//                byteArray.WriteFloat(o.g);
//                byteArray.WriteFloat(o.b);
//                byteArray.WriteFloat(o.a);
//            }

//            public override void List<T>(ref List<T> list, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum, fieldName);

//                if (list != null)
//                {
//                    byteArray.WriteInt32(list.Count);
//                    for (int i = 0; i < list.Count; i++)
//                    {
//                        T value = list[i];
//                        func(context, ref value, -1);
//                    }
//                }
//                else
//                {
//                    byteArray.WriteInt32(0);
//                }
//            }

//            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, fieldNum,fieldName);

//                string nameSpace = typeof(T).Namespace;
//                if (string.IsNullOrEmpty(nameSpace))
//                {
//                    nameSpace = NoNameSpace;
//                }
//                byteArray.WriteString(nameSpace);
//                byteArray.WriteInt16((short)v.ClassNameID());
//                byteArray.WriteString(v.GetType().FullName);

//                v.Order(context, -1);

//                SetEnd(context);
//            }

//            public override void Super( ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                SetHead(context, 0, BASE);

//                func(context, fieldNum);

//                SetEnd(context);
//            }

//            public override bool SetHead( int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                //if (fieldNum > 0)
//                //{
//                //    byteArray.WriteInt8((byte)fieldNum);
//                //}
//                if (!string.IsNullOrEmpty(fieldName))
//                {
//                    byteArray.WriteString(fieldName);
//                    return true;
//                }
//                return false;
//            }

//            public override void SetEnd( int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;

//                //byteArray.WriteInt8(FIELDEND_VALUE);
//                byteArray.WriteString(FIELDEND);
//            }
//        }

//        public class BinaryReader : IProtoType
//        {
//            public ReadBinaryContext env;

//            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadInt8();
//#if SerializeLOG
//                Debug.LogFormat("Int8 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
//            {
//                base.Int8(ref v, fieldNum, fieldName);
//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadInt16();
//#if SerializeLOG
//                Debug.LogFormat("Int16 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v); 
//#endif
//            }

//            public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadInt32();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadInt64();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadDouble();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadFloat();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadBoolean();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                v = byteArray.ReadString();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
//            {

//                IO.ByteArray byteArray = env.byteArray;
//                byteArray.ReadBytes(v);
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Vector2();


//                IO.ByteArray byteArray = env.byteArray;

//                if (fieldNum > 0)
//                    byteArray.WriteInt8((byte)fieldNum);

//                v.x = byteArray.ReadFloat();
//                v.y = byteArray.ReadFloat();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Vector3();


//                IO.ByteArray byteArray = env.byteArray;

//                if (fieldNum > 0)
//                    byteArray.WriteInt8((byte)fieldNum);

//                v.x = byteArray.ReadFloat();
//                v.y = byteArray.ReadFloat();
//                v.z = byteArray.ReadFloat();
//                base.Int8(ref v, fieldNum, fieldName);

//            }

//            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new VInt2();


//                IO.ByteArray byteArray = env.byteArray;

//                if (fieldNum > 0)
//                    byteArray.WriteInt8((byte)fieldNum);

//                v.x = byteArray.ReadInt32();
//                v.y = byteArray.ReadInt32();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new VInt3();


//                IO.ByteArray byteArray = env.byteArray;

//                if (fieldNum > 0)
//                    byteArray.WriteInt8((byte)fieldNum);

//                v.x = byteArray.ReadInt32();
//                v.y = byteArray.ReadInt32();
//                v.z = byteArray.ReadInt32();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
//            {
//                if (v == null)
//                    v = new Color();


//                IO.ByteArray byteArray = env.byteArray;

//                if (fieldNum > 0)
//                    byteArray.WriteInt8((byte)fieldNum);

//                v.r = byteArray.ReadInt32();
//                v.g = byteArray.ReadInt32();
//                v.b = byteArray.ReadInt32();
//                v.a = byteArray.ReadInt32();
//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void List<T>(ref List<T> list, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
//            {
//                IO.ByteArray byteArray = env.byteArray;

//                int count = byteArray.ReadInt32();


//                if (list == null)
//                    list = new List<T>();

//                for (int i = 0; i < count; i++)
//                {
//                    T value = new T();
//                    func(context, ref value);
//                    list.Add(value);
//                }

//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
//            {


//                IO.ByteArray byteArray = env.byteArray;

//                string nameSpaceName = byteArray.ReadString();
//                int classNameId = byteArray.ReadInt16();
//                string className = byteArray.ReadString();

//                if (v == null)
//                    v = (T)env.ObjectFactory(nameSpaceName, classNameId, className);




//                while (SetHead(context, fieldNum, fieldName))
//                {
//                    v.SwitchOrder(context, -1, headStr);
//                }

//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override void Super( ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
//            {
//                IO.ByteArray byteArray = env.byteArray;


//                while (SetHead(context, fieldNum, fieldName))
//                {
//                    func(context, -1, headStr);
//                }

//                base.Int8(ref v, fieldNum, fieldName);
//            }

//            public override bool SetHead( int fieldNum = -1, string fieldName = null)
//            {
//                IO.ByteArray byteArray = env.byteArray;

//                headStr = byteArray.ReadString();
//                if (headStr != FIELDEND)
//                {
//                    return true;
//                }
//                return false;
//            }

//            public override void SetEnd( int fieldNum = -1, string fieldName = null)
//            {
//                IO.ByteArray byteArray = env.byteArray;

//                //byteArray.WriteInt8(FIELDEND_VALUE);
//                byteArray.ReadString();
//            }
//        }

//        WriteBinaryContext wsc = new WriteBinaryContext();
//        BinaryWriter typeWriter = new BinaryWriter();

//        ReadBinaryContext rsc = new ReadBinaryContext();
//        BinaryReader typeReader = new BinaryReader();

//        public void Read(Stream stream, ref Persistence o)
//        {
//            rsc.byteArray = new IO.ByteArray();

//            rsc.byteArray.RequestBufferSize((int)stream.Length);
//            stream.Read(rsc.byteArray.GetBuffer(), 0, (int)stream.Length);
//            rsc.byteArray.SetWriteOffset((int)stream.Length);

//            rsc.protoType = typeReader;

//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Read"); 
//#endif
//            typeReader.Object(rsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Read"); 
//#endif
//        }

//        public void Write(Persistence o, Stream byteArray)
//        {
//            wsc.byteArray = new IO.ByteArray();
//            wsc.protoType = typeWriter;

//#if SerializeLOG
//            Debug.LogWarningFormat("Begin Write"); 
//#endif
//            typeWriter.Object(wsc, ref o);
//#if SerializeLOG
//            Debug.LogWarningFormat("End Write"); 
//#endif

//            byteArray.Write(wsc.byteArray.GetBuffer(), 0, wsc.byteArray.DataSize());

//        }

//        public override IOContext ResetWrite()
//        {
//            wsc.byteArray = new IO.ByteArray();
//            wsc.protoType = typeWriter;
//            return wsc;
//        }

//        public override IOContext ResetRead(byte[] bytes)
//        {
//            rsc.byteArray = new IO.ByteArray();

//            rsc.byteArray.RequestBufferSize((int)bytes.Length);
//            rsc.byteArray.SetBuffer(bytes);
//            rsc.byteArray.SetWriteOffset((int)bytes.Length);

//            rsc.protoType = typeReader;

//            return rsc;
//        }

//        public override IOContext ResetRead(Stream stream)
//        {
//            rsc.byteArray = new IO.ByteArray();

//            rsc.byteArray.RequestBufferSize((int)stream.Length);
//            stream.Read(rsc.byteArray.GetBuffer(), 0, (int)stream.Length);
//            rsc.byteArray.SetWriteOffset((int)stream.Length);

//            rsc.protoType = typeReader;

//            return rsc;
//        }

//        public override byte[] WriteToBytes()
//        {
//            byte[] bytes = wsc.byteArray.ReadData();
//            return bytes;
//        }

//        public override void WriteToStream(Stream stream)
//        {
//            stream.Write(wsc.byteArray.GetBuffer(), 0, wsc.byteArray.DataSize());
//        }

//        public override void WriteToFile(string path)
//        {
//            byte[] bytes = wsc.byteArray.ReadData();
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
//    }
//}
