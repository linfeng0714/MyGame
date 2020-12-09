using System.Net.Sockets;

namespace Net
{
    public class Message 
    {
        public TcpClient TcpClient { get; private set; }
        public byte[] Data { get; private set; }
        public Message(byte[] data,TcpClient client)
        {
            Data = data;
            TcpClient = client;
        }

        public void Reply(byte[] data)
        {
            //NetworkStreamUtil
        }
        
    }
}
