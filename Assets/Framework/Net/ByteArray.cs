using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Net
{
    public class ByteArray : Stream
    {
        public bool consume = true;

        protected byte[] byteBuffer = null;
        protected Dictionary<int, OffsetRange> offsetDict = null;

        protected Stack<Dictionary<int, OffsetRange>> offsetDictStack = null;
        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public class OffsetRange
        {
            public int ReadOffset = 0;
            public int WriteOffset = 0;
            public OffsetRange(int r ,int w)
            {
                ReadOffset = r;
                WriteOffset = w;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
