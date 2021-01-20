using System;
using System.Xml.Serialization;

namespace Framework.Base
{
    public class PooledClass
    {
        [NonSerialized] public uint usingSeq;
        [NonSerialized] public IObjPoolCtrl holder;
        [NonSerialized] public bool bChkReset = true;

        public virtual void OnUse()
        {
        }

        public virtual void OnRelease()
        {
        }

        public void Release()
        {
            if (IsReleased) return;
            this.OnRelease();
            this.usingSeq = 0u;
            if (this.holder != null)
            {
                this.holder.Release(this);
                this.holder = null;
            }
        }

        /// <summary>
        /// 是否已经释放
        /// </summary>
        public bool IsReleased
        {
            get { return usingSeq == 0; }
        }

        public static implicit operator bool(PooledClass obj)
        {
            bool flag = (object)obj == null;
            if (flag) return false;
            return obj.usingSeq != 0;
        }

        public static bool operator ==(PooledClass lhs, PooledClass rhs)
        {
            bool flag = (object)lhs == null;
            bool flag2 = (object)rhs == null;
            if (flag && flag2) return true;
            if (flag2) return lhs.usingSeq == 0;
            if (flag) return rhs.usingSeq == 0;
            if (lhs.usingSeq == rhs.usingSeq && lhs.GetType() == rhs.GetType()) return true;
            return false;
        }

        public static bool operator !=(PooledClass lhs, PooledClass rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            PooledClass gObject = obj as PooledClass;
            return this == gObject;
        }

        public override int GetHashCode()
        {
            return (int)usingSeq;
        }
    }

}