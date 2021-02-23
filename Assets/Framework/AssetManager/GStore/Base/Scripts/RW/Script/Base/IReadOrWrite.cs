using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace GStore.RW
{
    public class ReadOrWriteBase : IReadOrWrite
    {
#if SerializeLOG
        public static int logCount = 1000;
        public int logAcc = 0;
        //public Type write = typeof(RWBinaryOrder.BinaryWriter);
        //public Type read = typeof(RWBinaryOrder.BinaryReader);
        public Type write = typeof(RWXMLOrder.XMLWriter);
        public Type read = typeof(RWXMLOrder.XMLReader);
#endif

        public override void Int8(ref byte v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Int8 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Int16(ref short v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Int16 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Int32(ref int v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Int32 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Int64(ref long v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Int64 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Double(ref double v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Double fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Float(ref float v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Float fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Bool(ref bool v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Bool fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void String(ref string v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("String fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName, v);
                logAcc++;
            }
#endif
        }
        public override void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                if (v != null)
                {
                    Debug.LogFormat("Bytes fieldNum {0} fieldName {1} length {2}", fieldNum, fieldName, v.Length);
                }
                else
                {
                    Debug.LogFormat("Bytes fieldNum {0} fieldName {1} length {2}", fieldNum, fieldName, "null");
                }
               
                logAcc++;
            }
#endif
        }
        public override void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Vector2 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName,v);
                logAcc++;
            }
#endif
        }
        public override void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Vector3 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName,v);
                logAcc++;
            }
#endif
        }
        public override void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("VInt2 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName,v);
                logAcc++;
            }
#endif
        }
        public override void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("VInt3 fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName,v);
                logAcc++;
            }
#endif
        }
        public override void Color(ref Color v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Color fieldNum {0} fieldName {1} value {2}", fieldNum, fieldName,v);
                logAcc++;
            }
#endif
        }
        public override void List<T>(ref List<T> v, ProtoTypeDelegate<T> a, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("List<T> fieldNum {0} fieldName {1} T {2}", fieldNum, fieldName, typeof(T));
                logAcc++;
            }
#endif
        }
        public override void Object<T>(ref T v, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                string nameSpace = typeof(T).Namespace;
                string className = typeof(T).FullName.ToString();
                if (string.IsNullOrEmpty(nameSpace))
                {
                    nameSpace = "NoNameSpace";
                }
                Debug.LogFormat("Object<T> fieldNum {0} fieldName {1} T {2} nameSpace {3} claseName {4}", fieldNum, fieldName, typeof(T), nameSpace, className);
                logAcc++;
            }
#endif
        }
        public override void Super( ExchangeDelegate a, int fieldNum = -1, string fieldName = null)
        {
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Super fieldNum {0} fieldName {1} BaseType {2}", fieldNum, fieldName, this.GetType().BaseType);
                logAcc++;
            }
#endif
        }

        public override void Enum<T>(ref T v, int fieldNum = -1, string fieldName = null)
        {
            base.Enum(ref v, fieldNum, fieldName);
#if SerializeLOG
            if (logAcc <= logCount && (GetType() == write || GetType() == read))
            {
                Debug.LogFormat("Enum<T> fieldNum {0} fieldName {1} T {2} value {3}", fieldNum, fieldName, typeof(T),v);
                logAcc++;
            }
#endif
            
        }
    }

    public abstract class IReadOrWrite
    {
        //结束标识
        public const string FIELDEND = "FieldEnd";
        public const byte FIELDEND_VALUE = 255;

        //继承标识
        public const string BASE = "base";
        public const int BASE_VALUE = 0;

        //没有命名空间的标识
        public const string NoNameSpace = "NoNameSpace";

        public static readonly byte NULL = 255;
        public static readonly byte NOTNULL = 0;

        //头的值
        public int headNum = -1;
        public string headStr = "";

        public delegate void ListTypeDelegate<T>(ref T v, ProtoTypeDelegate<T> a, int fieldNum, string fieldName = null);
        public delegate void ProtoTypeDelegate<T>(ref T v, int fieldNum = -1, string fieldName = null);
        public delegate void ExchangeDelegate(RWContext c,int fieldNum = -1, string fieldName = null);

        public abstract void Int8(ref byte v, int fieldNum = -1,string fieldName = null);
        public abstract void Int16(ref short v, int fieldNum = -1, string fieldName = null);
        public abstract void Int32(ref int v, int fieldNum = -1, string fieldName = null);
        public abstract void Int64(ref long v, int fieldNum = -1, string fieldName = null);
        public abstract void Double(ref double v, int fieldNum = -1, string fieldName = null);
        public abstract void Float(ref float v, int fieldNum = -1, string fieldName = null);
        public abstract void Bool(ref bool v, int fieldNum = -1, string fieldName = null);
        public abstract void String(ref string v, int fieldNum = -1, string fieldName = null);
        public abstract void Bytes(ref byte[] v, int fieldNum = -1, string fieldName = null);
        public abstract void Vector2(ref Vector2 v, int fieldNum = -1, string fieldName = null);
        public abstract void Vector3(ref Vector3 v, int fieldNum = -1, string fieldName = null);
        public abstract void VInt_2(ref VInt2 v, int fieldNum = -1, string fieldName = null);
        public abstract void VInt_3(ref VInt3 v, int fieldNum = -1, string fieldName = null);
        public abstract void Color(ref Color v, int fieldNum = -1, string fieldName = null);
        public abstract void List<T>(ref List<T> v, ProtoTypeDelegate<T> a, int fieldNum = -1, string fieldName = null);
        public abstract void Object<T>(ref T v, int fieldNum = -1, string fieldName = null) where T : RWBaseObject, new();
        public abstract void Super( ExchangeDelegate a, int fieldNum = -1, string fieldName = null);
        public virtual void SkipField() { }
        public virtual bool SetHead(int fieldNum = -1, string fieldName = null) { return false; }
        public virtual void SetEnd(int fieldNum = -1, string fieldName = null) { }
        public virtual void SetFieldHead(int fieldNum = -1, string fieldName = null) { }

        public virtual void Enum<T>(ref T v, int fieldNum = -1, string fieldName = null)
        { 
            int e = (int)Convert.ToInt32(v);
            Int32(ref e, fieldNum, fieldName);
            v = (T)System.Enum.ToObject(typeof(T),e);
        }
    }
}
