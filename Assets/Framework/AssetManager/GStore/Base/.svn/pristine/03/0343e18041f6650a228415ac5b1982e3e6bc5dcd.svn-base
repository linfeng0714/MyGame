using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore
{
    public class CustomEncrypt : IEncrypt
    {
        string keyString = "wer2346534sdfdfd";

        int B_SIZE = 1;

        byte[] box = null;

        const int partLength = 256;

        byte[] cache = null;

        public void create_box()
        {
            if (box == null)
            {
                box = Encoding.UTF8.GetBytes(keyString);
            }
            if (cache == null)
            {
                cache = new byte[partLength];
            }
        }

        public byte[] Encrypt(byte[] byteText, string filePath = "")
        {
            create_box();

            int size = byteText.Length;

            int num = 0;
            int part = 0;
            int allPart = size / partLength;
            int remain = size % partLength;
            while (num < size)
            {
                if (num >= part * partLength)
                {
                    if (part != 0)
                    {
                        for (int i = 0; i < partLength; i++)
                        {
                            byteText[(part - 1) * partLength + i] = cache[i];
                        }
                    }
                    part++;
                }

                int cacheIndex = num - ((part - 1) * partLength);
                if (allPart < part)
                {
                    cache[cacheIndex] = byteText[2 * part * partLength - 2 * partLength + remain - 1 - num];
                    cache[cacheIndex] = (byte)(cache[cacheIndex] ^ box[0]);
                }
                else
                {
                    cache[cacheIndex] = byteText[2 * part * partLength - partLength - 1 - num];
                    cache[cacheIndex] = (byte)(cache[cacheIndex] ^ box[0]);
                }
                num++;
            }

            for (int i = 0; i < remain; i++)
            {
                byteText[(part - 1) * partLength + i] = cache[i];
            }

            return byteText;
        }

        public byte[] Decrypt(byte[] showText, string filePath = "")
        {
            return Encrypt(showText, filePath);
        }
    }
}
