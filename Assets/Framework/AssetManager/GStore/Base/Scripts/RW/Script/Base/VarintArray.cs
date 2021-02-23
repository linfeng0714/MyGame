using System;
using System.Collections;

namespace GStore.RW
{
    public class VarintArray
    {
        public ByteArray byteArray = new ByteArray();

        public byte ReadInt8()
        {
            return byteArray.ReadInt8();
        }

        public void WriteInt8(byte value)
        {
            byteArray.WriteInt8(value);
        }

        public bool ReadBoolean()
        {
            return byteArray.ReadBoolean();
        }

        public void WriteBoolean(bool value)
        {
            byteArray.WriteBoolean(value);
        }

        public Int16 ReadInt16()
        {
            return (Int16)readVarint32Byte();
        }

        public void WriteInt16(Int16 value)
        {
            WriteInt32Byte((UInt16)value);
        }

        public Int32 ReadInt32()
        {
            return (Int32)readVarint32Byte();
        }

        public void WriteInt32(Int32 value)
        {
            WriteInt32Byte((UInt32)value);
        }

        public Int64 ReadInt64()
        {
            return (Int64)readVarint64Byte();
        }
        public void WriteInt64(Int64 value)
        {
            WriteInt64Byte((UInt64)value);
        }

        protected void WriteInt32Byte(UInt32 value)
        {
            if (value < 128)
            {
                byteArray.WriteInt8((byte)value);
                return;
            }

            while (value > 127)
            {
                byteArray.WriteInt8((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            byteArray.WriteInt8((byte)value);
        }

        protected uint readVarint32Byte()
        {
            int tmp = byteArray.ReadInt8();
            if (tmp < 128)
            {
                return (uint)tmp;
            }

            int result = tmp & 0x7f;
            if ((tmp = byteArray.ReadInt8()) < 128)
            {
                result |= tmp << 7;
            }
            else
            {
                result |= (tmp & 0x7f) << 7;
                if ((tmp = byteArray.ReadInt8()) < 128)
                {
                    result |= tmp << 14;
                }
                else
                {
                    result |= (tmp & 0x7f) << 14;
                    if ((tmp = byteArray.ReadInt8()) < 128)
                    {
                        result |= tmp << 21;
                    }
                    else
                    {
                        result |= (tmp & 0x7f) << 21;
                        result |= (tmp = byteArray.ReadInt8()) << 28;
                        if (tmp >= 128)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (byteArray.ReadInt8() < 128)
                                {
                                    return (uint)result;
                                }
                            }
                            throw new InvalidCastException();
                        }
                    }
                }
            }
            return (uint)result;
        }

        protected uint readVarint32ByteToSbyte()
        {
            byte b = byteArray.ReadInt8();
            sbyte sb;
            unchecked
            {
                sb = (sbyte)b;
            }
            int tmp = sb;
            if (tmp < 128)
            {
                return (uint)tmp;
            }

            int result = tmp & 0x7f;
            if ((tmp = byteArray.ReadInt8()) < 128)
            {
                result |= tmp << 7;
            }
            else
            {
                result |= (tmp & 0x7f) << 7;
                if ((tmp = byteArray.ReadInt8()) < 128)
                {
                    result |= tmp << 14;
                }
                else
                {
                    result |= (tmp & 0x7f) << 14;
                    if ((tmp = byteArray.ReadInt8()) < 128)
                    {
                        result |= tmp << 21;
                    }
                    else
                    {
                        result |= (tmp & 0x7f) << 21;
                        result |= (tmp = byteArray.ReadInt8()) << 28;
                        if (tmp >= 128)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (byteArray.ReadInt8() < 128)
                                {
                                    return (uint)result;
                                }
                            }
                            throw new InvalidCastException();
                        }
                    }
                }
            }
            return (uint)result;
        }

        protected void WriteInt64Byte(UInt64 value)
        {
            while (value > 127)
            {
                byteArray.WriteInt8((byte) ((value & 0x7F) | 0x80));
                value >>= 7;
            }
            while (value > 127)
            {
                byteArray.WriteInt8((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            byteArray.WriteInt8((byte)value);
        }

        protected ulong readVarint64Byte()
        {
            int shift = 0;
            ulong result = 0;
            while (shift < 64)
            {
                byte b = byteArray.ReadInt8();
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
                shift += 7;
            }
            throw new InvalidCastException();
        }

        public string ReadString()
        {
            short len = 0;

            len = ReadInt16();

            string s = System.Text.Encoding.UTF8.GetString(byteArray.GetBuffer(),byteArray.ReadOffset(),len);
            byteArray.ConsumeByte(len);

            return s;
        }

        public void WriteString(string s)
        {
            byte[] val = System.Text.Encoding.UTF8.GetBytes(s);
            short len = (short)val.Length;
            WriteInt16(len);
            byteArray.writeBytes(val);
        }
    }
}
