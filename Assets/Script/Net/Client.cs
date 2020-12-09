using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Net
{
    public class Client : IDisposable
    {
        public int ReadLoopIntervalMs { get; set; }
        private TcpClient m_TcpClient;
        public bool QueueStop;
        private string m_Host;
        private int m_Port;
        //private event EventHandler<Message> 
        public void Dispose()
        {
            
        }
    }
}
