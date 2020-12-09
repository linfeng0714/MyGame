using System;
using System.Net.Sockets;
using System.Threading;

namespace Net
{
    public class NetworkStreamUtil
    {
        class AsyncWriteStateObject
        {
            public ManualResetEvent eventDone;
            public NetworkStream stream;
            public Exception exception;
        }
        public static void Write(NetworkStream steam, byte[] data)
        {
            AsyncWriteStateObject state = new AsyncWriteStateObject();
            state.eventDone = new ManualResetEvent(false);
            state.stream = steam;
            state.exception = null;
            //using(ByteBuffer)

         }
    }
}

