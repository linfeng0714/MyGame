using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore
{
    /// <summary>
    /// 资源异常
    /// </summary>
    public class AssetException : UnityEngine.UnityException
    {
        public AssetException(string msg) : base(msg)
        {

        }

        public AssetException(string msg, Exception innerException) : base(msg, innerException)
        {

        }
    }
}
