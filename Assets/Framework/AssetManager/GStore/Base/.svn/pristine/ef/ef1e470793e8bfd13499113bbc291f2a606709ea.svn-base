using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GStore.RW
{
    public abstract class IReaderAndWriter
    {
        public abstract RWContext ResetWrite();

        public abstract RWContext ResetRead(byte[] bytes);

        public abstract RWContext ResetRead(Stream stream);

        public abstract byte[] WriteToBytes();

        public abstract void WriteToStream(Stream stream);

        public abstract void ReadObjectByStream<T>(Stream stream, ref T o) where T : RWBaseObject, new();

        public abstract void ReadObjectByBytes<T>(byte[] bytes, ref T o) where T : RWBaseObject, new();

        public abstract void WriteObjectToStream(RWBaseObject o, Stream stream);

        public abstract byte[] WriteObjectToBytes(RWBaseObject o);

        public abstract void WriteToFile(string path);
    }
}

