using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;
using System.Net;

enum NetErrorType
{
    eSendError = 0,	 /*发送数据错误	-(重激活)*/
    eRecvError = 1,	 /*接受数据错误	-(重激活)*/
};

enum NetStatue
{
    /** 0 - 未连接 */
    eDISCONNECT = 0,
    /** 1 - 正常工作中 */
    eWORKING,
    /** 2 - 事务行错误 */
    eTRANSERROR,
};
namespace Framework.Net
{
    public class Connection
    {
        public delegate void EventHandle(System.Object param);
        public delegate int Handle(byte[] data, int size, int reciv_time);
        public enum State
        {
            None,
            Initialized,

            Connecting,
            Connected,
            ConnectFailed,
            DisconTimeout,

            DisconRecvErr1, //服务器异常掉线
            DisconRecvErr2, //服务器主动掉线
            DisconSendErr1,
            DisconSendErr2,
            Close,
        }

        private EventHandle _disconnect = null;
        private Handle _handle = null;

        private Action<int> _fErrorNotify;

        /*发送队列*/
        private ByteArrayQueue _sendQueue;
        /*接受队列*/
        private ByteArrayQueue _receiveQueue;

        /*缓存队列*/
        private LinkedList<ByteArray> _cacheQueue;

        public Connection(Handle handle , Action<int> errorFunc = null)
        {
            _handle = handle;
            setErrorNotify(errorFunc);
        }

        private void setErrorNotify(Action<int> errorFunc)
        {
            _fErrorNotify = errorFunc;
        }
    }
    public class NetworkManager : Singleton<NetworkManager>
    {
    }
}
