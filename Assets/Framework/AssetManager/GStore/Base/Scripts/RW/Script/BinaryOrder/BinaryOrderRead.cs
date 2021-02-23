using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GStore.RW
{
    public partial class RWBinaryOrder : IReaderAndWriter
    {
        public class ReadBinaryContext : RWContext
        {
            public ByteArray byteArray = null;
        }

        public class BinaryReader : ReadOrWriteBase
        {
            public ReadBinaryContext env;

            public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadInt8();
                base.Int8(ref v, fieldNum, fieldName);
            }

            public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadInt16();
                base.Int16(ref v, fieldNum, fieldName);
            }

            public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadInt32();
                base.Int32(ref v, fieldNum, fieldName);
            }

            public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadInt64();
                base.Int64(ref v, fieldNum, fieldName);
            }

            public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadDouble();
                base.Double(ref v, fieldNum, fieldName);
            }

            public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadFloat();
                base.Float(ref v, fieldNum, fieldName);
            }

            public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
            {
                
                ByteArray byteArray = env.byteArray;
                v = byteArray.ReadBoolean();
                base.Bool(ref v, fieldNum, fieldName);
            }

            public override void String(ref string v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byte isNull = byteArray.ReadInt8();
                if (isNull == NULL)
                {
                    v = "";
                }
                else
                {
                    v = byteArray.ReadString();
                }
                base.String(ref v, fieldNum, fieldName);
            }

            public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                byte isNull = byteArray.ReadInt8();
                if (isNull == NULL)
                {
                    v = null;
                }
                else
                {
                    if (v == null)
                    {
                        v = new byte[] { };
                    }
                    byteArray.ReadBytes(v);
                }
                base.Bytes(ref v, fieldNum, fieldName);
            }

            public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v.x = byteArray.ReadFloat();
                v.y = byteArray.ReadFloat();
                base.Vector2(ref v, fieldNum, fieldName);
            }

            public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v.x = byteArray.ReadFloat();
                v.y = byteArray.ReadFloat();
                v.z = byteArray.ReadFloat();
                base.Vector3(ref v, fieldNum, fieldName);
            }

            public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v.x = byteArray.ReadInt32();
                v.y = byteArray.ReadInt32();
                base.VInt_2(ref v, fieldNum, fieldName);
            }

            public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
            {
                ByteArray byteArray = env.byteArray;
                v.x = byteArray.ReadInt32();
                v.y = byteArray.ReadInt32();
                v.z = byteArray.ReadInt32();
                base.VInt_3(ref v, fieldNum, fieldName);
            }

            public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
            {
                
                ByteArray byteArray = env.byteArray;
                v.r = byteArray.ReadFloat();
                v.g = byteArray.ReadFloat();
                v.b = byteArray.ReadFloat();
                v.a = byteArray.ReadFloat();
                base.Color(ref v, fieldNum, fieldName);
            }

            public override void Super(ExchangeDelegate func, int fieldNum = -1, string fieldName = null)
            {
                base.Super(func, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                func(env, fieldNum, headStr);
            }

            public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
            {
                base.Object(ref v, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                byte isNull = byteArray.ReadInt8();
                if (isNull == NULL)
                {
                    v = null;
                }
                else
                {
                    string nameSpaceName = byteArray.ReadString();
                    int classNameId = byteArray.ReadInt16();
                    string className = byteArray.ReadString();
                    if (v == null)
                    {
                        v = (T)env.ObjectFactory(nameSpaceName,className, classNameId);
                    }
                    v.Order(env, fieldNum, headStr);
                }
            }

            public override void List<T>(ref List<T> v, ProtoTypeDelegate<T> func, int fieldNum = -1, string fieldName = null)
            {
                base.List(ref v, func, fieldNum, fieldName);
                ByteArray byteArray = env.byteArray;
                int count = byteArray.ReadInt32();
                if (v == null)
                    v = new List<T>();
                for (int i = 0; i < count; i++)
                {
                    T value = default(T);
                    func(ref value);
                    v.Add(value);
                }
            }

            public override bool SetHead(int fieldNum = -1, string fieldName = null)
            {
                return true;
            }
        }

        ReadBinaryContext rsc = new ReadBinaryContext();
        BinaryReader typeReader = new BinaryReader();

        public override RWContext ResetRead(byte[] bytes)
        {
            rsc.byteArray = new ByteArray();

            rsc.byteArray.RequestBufferSize((int)bytes.Length);
            rsc.byteArray.SetBuffer(bytes);
            rsc.byteArray.SetWriteOffset((int)bytes.Length);

            rsc.isEditor = false;
            typeReader.env = rsc;
            rsc.rwType = typeReader;
            rsc.isReadContext = true;

            return rsc;
        }

        public override RWContext ResetRead(Stream stream)
        {
            rsc.byteArray = new ByteArray();
            rsc.byteArray.RequestBufferSize((int)stream.Length);
            stream.Read(rsc.byteArray.GetBuffer(), 0, (int)stream.Length);
            rsc.byteArray.SetWriteOffset((int)stream.Length);
            rsc.rwType = typeReader;
            rsc.isReadContext = true;
            return rsc;
        }

        public override void ReadObjectByStream<T>(Stream stream, ref T o)
        {
            ResetRead(stream);
#if SerializeLOG
            Debug.LogWarningFormat("Begin Read");
#endif
            typeReader.Object(ref o);
#if SerializeLOG
            Debug.LogWarningFormat("End Read");
#endif
        }

        public override void ReadObjectByBytes<T>(byte[] bytes, ref T o)
        {
            ResetRead(bytes);
#if SerializeLOG
            Debug.LogWarningFormat("Begin Read");
#endif
            typeReader.Object(ref o);
#if SerializeLOG
            Debug.LogWarningFormat("End Read");
#endif
        }
    }
}
