using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore
{
    public interface IEncrypt
    {
        byte[] Encrypt(byte[] byteText, string filePath = "");
        byte[] Decrypt(byte[] showText, string filePath = "");

    }
}

