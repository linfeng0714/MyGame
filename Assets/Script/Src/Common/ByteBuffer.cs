﻿using System;
using System.Text;

public class ByteBuffer : IDisposable
{
    private byte[] bytes = new byte[0];
    private int position = 0;

    public byte[] source
    {
        get { return bytes; }
        set
        {
            bytes = value;
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        position = 0;
    }

    public int GetPosition() { return position; }

    public ByteBuffer(byte[] value)
    {
        source = value;
    }

    public ByteBuffer WriteInt16(short value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public short ReadInt16()
    {
        return BitConverter.ToInt16(get(2), 0);
    }
    public ByteBuffer WriteInt32(int value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public int ReadInt32()
    {
        return BitConverter.ToInt32(get(4), 0);
    }
    public ByteBuffer WriteUInt32(uint value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public uint ReadUInt32()
    {
        return BitConverter.ToUInt32(get(4), 0);
    }
    public ByteBuffer WriteUInt16(ushort value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public ushort ReadUInt16()
    {
        return BitConverter.ToUInt16(get(2), 0);
    }
    public ByteBuffer WriteComplexCollection<T>(System.Collections.Generic.List<T> collection, Func<T, byte[]> forEach)
    {
        int count = collection.Count;
        WriteInt32(count);
        for (int i = 0; i < count; ++i)
        {
            WriteBytes(forEach(collection[i]));
        }
        return this;
    }
    public ByteBuffer WriteSimpleCollection<T>(System.Collections.Generic.List<T> collection, Action<T> forEach)
    {
        int count = collection.Count;
        WriteInt32(count);
        for (int i = 0; i < count; ++i)
        {
            forEach(collection[i]);
        }
        return this;
    }
    public System.Collections.Generic.List<T> ReadSimpleCollection<T>(Func<T> forEach)
    {
        System.Collections.Generic.List<T> collection = new System.Collections.Generic.List<T>();
        int count = ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            collection.Add(forEach());
        }
        return collection;
    }
    public System.Collections.Generic.List<T> ReadComplexCollection<T>(Func<byte[], T> forEach)
    {
        System.Collections.Generic.List<T> collection = new System.Collections.Generic.List<T>();
        int count = ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            collection.Add(forEach(ReadBytes()));
        }

        return collection;
    }
    public ByteBuffer WriteString(string value)
    {
        byte[] data = Encoding.UTF8.GetBytes(value);
        WriteInt32(data.Length);
        copy(data);
        return this;
    }
    public string ReadString()
    {
        return Encoding.UTF8.GetString(get(ReadInt32()));
    }
    public ByteBuffer WriteBytes(byte[] value)
    {
        WriteInt32(value.Length);
        copy(value);
        return this;
    }
    public byte[] ReadBytes()
    {
        return get(ReadInt32());
    }
    public ByteBuffer WriteByte(byte value)
    {
        copy(new byte[] { value });
        return this;
    }
    public byte ReadByte()
    {
        return get(1)[0];
    }
    public ByteBuffer WriteBool(bool value)
    {
        WriteByte(Convert.ToByte(value));
        return this;
    }
    public bool ReadBool()
    {
        return Convert.ToBoolean(ReadByte());
    }
    public ByteBuffer WriteInt64(long value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public long ReadInt64()
    {
        return BitConverter.ToInt32(get(8), 0);
    }
    public ByteBuffer WriteUInt64(ulong value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public ulong ReadUInt64()
    {
        return BitConverter.ToUInt64(get(8), 0);
    }
    public ByteBuffer WriteFloat(float value)
    {
        copy(BitConverter.GetBytes(value));
        return this;
    }
    public float ReadFloat()
    {
        return BitConverter.ToSingle(get(4), 0);
    }

    private void copy(byte[] value)
    {
        byte[] temps = new byte[bytes.Length + value.Length];
        Buffer.BlockCopy(bytes, 0, temps, 0, bytes.Length);
        Buffer.BlockCopy(value, 0, temps, position, value.Length);


        position += value.Length;
        bytes = temps;
        temps = null;
    }

    private byte[] get(int length)
    {
        byte[] data = new byte[length];
        Buffer.BlockCopy(bytes, position, data, 0, length);
        position += length;
        return data;
    }

    public void Dispose()
    {
        position = 0;
        bytes = null;
    }

    
}
