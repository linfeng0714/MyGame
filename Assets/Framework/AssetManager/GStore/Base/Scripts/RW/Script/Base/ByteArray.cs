using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace GStore.RW
{
    public class ByteArray : Stream
    {
        public bool consume = true;

        protected byte[] byteBuffer = null;
        //protected int currentOffsetRange.readOffset = 0;
        //protected int currentOffsetRange.writeOffset = 0;

        protected Dictionary<int, OffsetRange> offsetDict = null;

        protected Stack<Dictionary<int, OffsetRange>> offsetDictStack = null;

        protected OffsetRange defaultOffsetRange = new OffsetRange(0,0);

        protected OffsetRange currentOffsetRange = null;

        static Queue<OffsetRange> offsetRangeCache = null;//new List<OffsetRange>();

        static protected OffsetRange GetOffsetRange()
        {
            if (offsetRangeCache == null) {
                offsetRangeCache = new Queue<OffsetRange>();
            }

            if (offsetRangeCache.Count > 0)
            {
                return offsetRangeCache.Dequeue();
            }
            else {
                return new OffsetRange(0,0);
            }
        }

        static protected void CacheOffsetRange(OffsetRange offsetRange)
        {
            if (offsetRangeCache == null)
            {
                offsetRangeCache = new Queue<OffsetRange>();
            }

            offsetRangeCache.Enqueue(offsetRange);
        }

        public ByteArray() {
            currentOffsetRange = defaultOffsetRange;
        }

        public class OffsetRange {
            public OffsetRange(int r,int w) {
                readOffset = r;
                writeOffset = w;
            }

            public int readOffset = 0;
            public int writeOffset = 0;
        }

        public void PushRangeStack(Dictionary<int, OffsetRange> dict)
        {
            if (offsetDictStack == null)
            {
                offsetDictStack = new Stack<Dictionary<int, OffsetRange>>();
            }

            offsetDictStack.Push(dict);
            offsetDict = offsetDictStack.Peek();
        }

        public void PopRangeStack()
        {
            if (offsetDictStack == null || offsetDictStack.Count < 1)
            {
                throw new Exception("pop a empty stack.");
            }

            offsetDictStack.Pop();
        }

        public void AddOffsetRange(int index,int readOffset,int writeOffset)
        {
            if (offsetDict == null)
            {
                offsetDict = new Dictionary<int, OffsetRange>();
            }

            OffsetRange offsetRange = null;

            if (offsetDict.TryGetValue(index, out offsetRange))
            {
                offsetRange.readOffset = readOffset;
                offsetRange.writeOffset = writeOffset;
            }
            else
            {
                offsetRange = GetOffsetRange();
                offsetRange.readOffset = readOffset;
                offsetRange.writeOffset = writeOffset;
                offsetDict.Add(index,offsetRange);
            }
        }

        public void CleanAllRange()
        {
            foreach (var range in offsetDict)
            {
                CacheOffsetRange(range.Value);
            }

            offsetDict.Clear();
        }

        public void RemoveRange(int index)
        {
            OffsetRange offsetRange = null;

            if (offsetDict.TryGetValue(index, out offsetRange))
            {
                if (offsetRange == currentOffsetRange || offsetRange == defaultOffsetRange)
                    throw new Exception("try to cache a offsetRange that is currently in used!");

                CacheOffsetRange(offsetRange);
                offsetDict.Remove(index);
            }
        }

        public void ApplayDefaultRange()
        {
            currentOffsetRange = defaultOffsetRange;
        }

        public bool ApplyRange(int index)
        {
            if (offsetDict != null)
            {
                OffsetRange offsetRange = null;

                if (offsetDict.TryGetValue(index, out offsetRange))
                {
                    currentOffsetRange = offsetRange;

                    return true;
                }
            }

            //throw new Exception("offsetRange index is not exist!");

            return false;
        }

        public override bool CanRead
        {
            get
            {
                return DataSize() > 0;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return DataSize() > 0;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return DataSize();
            }
        }

        private long _position = 0;

        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
            }
        }

        public void ReUse()
        {
            currentOffsetRange.readOffset = 0;
            currentOffsetRange.writeOffset = 0;
        }

        public int GetDataLength()
        {
            return currentOffsetRange.writeOffset - currentOffsetRange.readOffset;
        }

        public int GetHeadOffset()
        {
            return currentOffsetRange.readOffset;
        }

        public void SetHeadOffset(int offset)
        {
            currentOffsetRange.readOffset = offset;
        }

        public int readBytes(byte[] buffer,int offset,int nLen)
        {
            if (DataSize() < nLen)
                nLen = DataSize();

            if (currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界  readBytes ");
                 throw new IndexOutOfRangeException(" Failed: 长度越界  readBytes ");
            }

            Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, buffer, offset, nLen);

            if(consume)
                this.currentOffsetRange.readOffset += nLen;

            return nLen;
        }

        public int INCREAT_SIZE = 100;

        public void CleanReadOffset()
        {
            Buffer.BlockCopy(byteBuffer,currentOffsetRange.readOffset,byteBuffer,0,currentOffsetRange.writeOffset - currentOffsetRange.readOffset);
            currentOffsetRange.writeOffset -= currentOffsetRange.readOffset;
            currentOffsetRange.readOffset = 0;
        }

        public void RequestBufferSize(int expectLength)
        {
            if (byteBuffer == null)
            {
                byteBuffer = new byte[0];
                currentOffsetRange.readOffset = 0;
                currentOffsetRange.writeOffset = 0;
            }

            int remainSize = byteBuffer.Length - currentOffsetRange.writeOffset;

            if (remainSize < expectLength)
            {
                if(remainSize + currentOffsetRange.readOffset >= expectLength)
                {
                    CleanReadOffset();
                    return;
                }

                int newSize = Mathf.CeilToInt((expectLength + GetDataLength()) / (float)INCREAT_SIZE) * INCREAT_SIZE;

                byte[] buf = new byte[newSize];

                Buffer.BlockCopy(byteBuffer,currentOffsetRange.readOffset, buf, 0,currentOffsetRange.writeOffset - currentOffsetRange.readOffset);

                byteBuffer = buf;
                currentOffsetRange.readOffset = 0;
                currentOffsetRange.writeOffset -= currentOffsetRange.readOffset;
            }
        }

        public void OverrideBytes(byte[] buffer, int offset = 0, int size = -1, int position = 0)
        {
            if (buffer.Length < 1)
            {
                return;
            }

            int len = size < 0 ? buffer.Length : size;

            RequestBufferSize(len);

            Buffer.BlockCopy(buffer, offset, byteBuffer, currentOffsetRange.readOffset + position, len);

            int inc = (currentOffsetRange.readOffset + position) - currentOffsetRange.writeOffset;

            if (inc < 0)
                inc = 0;

            currentOffsetRange.writeOffset += inc;
        }

        public void writeBytes(byte[] buffer,int offset = 0,int size = -1)
        {
            if (buffer.Length < 1)
            {
                return;
            }

            int len = size < 0 ? buffer.Length : size;
            
            RequestBufferSize(len);

            Buffer.BlockCopy(buffer,offset, byteBuffer, currentOffsetRange.writeOffset, len);

            currentOffsetRange.writeOffset += len;
        }

        public int DataSize()
        {
            if (byteBuffer == null)
            {
                return 0;
            }
            return currentOffsetRange.writeOffset - currentOffsetRange.readOffset;
        }

        public void SetReadOffset(int offset)
        {
            currentOffsetRange.readOffset = offset;
        }

        public int ReadOffset()
        {
            return currentOffsetRange.readOffset;
        }

        public int WriteOffset()
        {
            return currentOffsetRange.writeOffset;
        }

        public void SetWriteOffset(int offset)
        {
             currentOffsetRange.writeOffset = offset;
        }

        public string dataString()
        {
            return "";
        }

        public void ConsumeByte(int len)
        {
            System.Diagnostics.Debug.Assert(byteBuffer.Length >= currentOffsetRange.readOffset + len);
            currentOffsetRange.readOffset += len;
        }

        public void SetBuffer(byte[] bytes)
        {
            byteBuffer = bytes;
            currentOffsetRange.readOffset = 0;
            currentOffsetRange.writeOffset = bytes.Length;
    }

        public void ClearBuffer()
        {
            currentOffsetRange.writeOffset = 0;
            currentOffsetRange.readOffset = 0;
        }

        sbyte ByteAtIndex(int index)
        {
            if (index > byteBuffer.Length)
            {
                return 0;
            }

            return Convert.ToSByte(byteBuffer[index]);
        }

        public sbyte ReadSbyte()
        {
            int nLen = sizeof(byte);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界  ReadSbyte ");
                throw new IndexOutOfRangeException(" Failed: 长度越界  ReadSbyte ");
            }
            byte bt = byteBuffer[currentOffsetRange.readOffset];

            if (consume)
                this.currentOffsetRange.readOffset += nLen;

            sbyte sb;
            unchecked
            {
                sb = (sbyte)bt;
            }
            return sb;
        }

        byte[] tempBytes = new byte[8];

        public byte ReadInt8()
        {
            int nLen = sizeof(byte);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界  ReadInt8 ");
                throw new IndexOutOfRangeException(" Failed: 长度越界  ReadInt8 ");
            }
            byte bt = byteBuffer[currentOffsetRange.readOffset];

            if (consume)
                this.currentOffsetRange.readOffset += nLen;

            return bt;
        }
        public short ReadInt16(bool bReverse = false)
        {
            int nLen = sizeof(short);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界   ReadInt16 ");
                throw new IndexOutOfRangeException(" Failed: 长度越界   ReadInt16 ");
            }

            short val = 0;

            if (bReverse)
            {
                Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, tempBytes, 0, 2);
                Array.Reverse(tempBytes, 0, 2);
                val = BitConverter.ToInt16(tempBytes, 0);
            }
            else {
                val = BitConverter.ToInt16(byteBuffer, currentOffsetRange.readOffset);
            }
                
            if (consume)
                currentOffsetRange.readOffset += nLen;

            return val;

        }
        public int ReadInt32(bool bReverse = false)
        {
            int nLen = sizeof(int);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界   ReadInt32 ");
                throw new IndexOutOfRangeException(" Failed: 长度越界   ReadInt32 ");
            }

            int val = 0;

            if (bReverse)
            {
                Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, tempBytes, 0, 4);
                Array.Reverse(tempBytes, 0, 4);
                val = BitConverter.ToInt32(tempBytes,0);
            }
            else
            {
                val = BitConverter.ToInt32(byteBuffer, currentOffsetRange.readOffset);
            }

            if (consume)
                this.currentOffsetRange.readOffset += nLen;

            return val;
        }

        public long ReadInt64(bool bReverse = false)
        {
            int nLen = sizeof(Int64);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界   ReadInt64 ");
                throw new IndexOutOfRangeException(" Failed: 长度越界   ReadInt64 ");
            }

            long val = 0;

            if (bReverse)
            {
                Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, tempBytes, 0,8);
                Array.Reverse(tempBytes, 0, 8);
                val = BitConverter.ToInt64(tempBytes, 0);
            }
            else
            {
                val = BitConverter.ToInt64(byteBuffer, currentOffsetRange.readOffset);
            }

            if (consume)
                this.currentOffsetRange.readOffset += nLen;

            return val;
        }

        public double ReadDouble(bool bReverse = false)
        {
            int nLen = sizeof(int);
            if (this.currentOffsetRange.readOffset + nLen > this.byteBuffer.Length)
            {
                Debug.Log(" Failed: 长度越界   ReadInt32 ");
                throw new IndexOutOfRangeException(" Failed: 长度越界   ReadInt32 ");
            }

            double val = 0;

            if (bReverse)
            {
                Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, tempBytes, 0, 8);
                Array.Reverse(tempBytes, 0, 8);
                val = BitConverter.ToDouble(tempBytes, 0);
            }
            else
            {
                val = BitConverter.ToDouble(byteBuffer, currentOffsetRange.readOffset);
            }

            if (consume)
                this.currentOffsetRange.readOffset += 8;

            return val;
        }

        public float ReadFloat(bool bReverse = false)
        {
            return (float)ReadDouble(bReverse);
        }

        public string ReadString()
        {
            int len = 0;

            len = ReadInt16();

            System.Diagnostics.Debug.Assert(DataSize() >= len);

            string s = System.Text.Encoding.UTF8.GetString(GetBuffer(),ReadOffset(), len);

            ConsumeByte(len);

            return s;
        }

//         public override int ReadByte()
//         {
//             return (int)ReadInt8();
//         }

        public bool ReadBoolean()
        {
           bool b = ReadInt8() > 0;
           return b;
        }

        public void ReadBytes(byte[] value, int index = 0)
        {
            if (value != null && index > value.Length)
            {
                throw new Exception("invalid index.");
            }

            int len = ReadInt32();

            int left = value.Length - index;

            if (value == null)
            {
                index = 0;
                value = new byte[len];
            } else if (left < len)
            {
                byte[] t = new byte[value.Length + (len - left)];
                Buffer.BlockCopy(value,0,t,0,value.Length);
                value = t;
            }

            readBytes(value, index, len);
        }


        public void WriteBoolean(bool value)
        {
            byte t = 0;
            if (value) t = 1; 
            WriteInt8(t);
        }

        public void WriteInt8(byte value)
        {
            WriteByte(value);
        }

        public void WriteInt16(short value,bool bReverse = false)
        {
            byte[] val = BitConverter.GetBytes(value);
            
            if(bReverse)
                Array.Reverse(val);

            writeBytes(val);
        }

        public void WriteInt32(int value, bool bReverse = false)
        {
            byte[] val = BitConverter.GetBytes(value);

            if (bReverse)
                Array.Reverse(val);

            writeBytes(val);
        }

        public void WriteInt64(long value, bool bReverse = false)
        {
            byte[] val = BitConverter.GetBytes(value);

            if (bReverse)
                Array.Reverse(val);

            writeBytes(val);
        }

        public void WriteDouble(double value, bool bReverse = false)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (bReverse)
                Array.Reverse(bytes);

            writeBytes(bytes);
        }

        public void WriteFloat(float value,bool reverse = false)
        {
            WriteDouble(value, reverse);
        }

        public void WriteString(string value)
        {
            var datas = System.Text.Encoding.UTF8.GetBytes(value);
            short len = (short)datas.Length;

            WriteInt16(len);

            writeBytes(datas);
        }

        public void WriteBytes(byte[] value,int index = 0,int count = -1)
        {
            if (count == -1) count = value.Length;

            short len = (short)count;

            WriteInt32(len);

            writeBytes(value,index,count);
        }

        public void writeByteBuffer(ByteArray buffer)
        {
            writeBytes(buffer.byteBuffer);
        }

        public ByteArray copyBufferPart(int startIndex, int nCount)
        {
            ByteArray buffer = new ByteArray();

            byte[] byteArray = new byte[nCount];

            Buffer.BlockCopy(byteBuffer, startIndex, byteArray, 0, nCount);

            buffer.writeBytes(byteArray);

            return buffer;

        }

        public ByteArray copyBufferAll()
        {
            ByteArray buffer = new ByteArray();

            int byteLen = DataSize() - ReadOffset();

            byte[] ByteArray = new byte[byteLen];

            Buffer.BlockCopy(byteBuffer, currentOffsetRange.readOffset, ByteArray, 0, byteLen);

            buffer.writeBytes(ByteArray);

            return buffer;
        }

        //void cleanHeadOffset();
        public byte[] GetBuffer()
        {
            if (byteBuffer !=null)
            {
                return byteBuffer;
            }
            return new byte[0];
        }

        public override void Flush()
        {
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return readBytes(buffer,offset,count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writeBytes(buffer, offset, count);
        }

        public byte[] ReadData()
        {
            byte[] bytes = new byte[GetDataLength()];
            consume = false;
            Read(bytes, ReadOffset(), bytes.Length);
            consume = true;
            return bytes;
        }
    }
}


