using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore
{
    
    public class ABEncrypt : IEncrypt
    {
        public byte[] Encrypt(byte[] byteText, string filePath = "")
        {
            byte[] bytes = new byte[3] { 0, 9, 8 };
            List<byte> byteTemps = new List<byte>();
            byteTemps.AddRange(bytes);
            byteTemps.AddRange(byteText);
            return byteTemps.ToArray();
        }

        public byte[] Decrypt(byte[] showText, string filePath = "")
        {
            throw new NotImplementedException();
        }
    }
}

