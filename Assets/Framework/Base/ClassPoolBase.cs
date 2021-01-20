using System;
using System.Collections.Generic;

namespace Framework.Base
{
    public abstract class ClassPoolBase : IObjPoolCtrl
    {
        private static List<IObjPoolCtrl> _CreatedPoolList = new List<IObjPoolCtrl>();
        protected List<object> pool = new List<object>(128);
        protected uint reqSeq;

        public int capacity
        {
            get
            {
                return this.pool.Capacity;
            }
            set
            {
                this.pool.Capacity = value;
            }
        }

        public ClassPoolBase()
        {
            _CreatedPoolList.Add(this);
        }

        public static void ClearCreatedPool()
        {
            for (int i = 0; i < _CreatedPoolList.Count; i++)
            {
                _CreatedPoolList[i].Clear();
            }
        }

        public object GetObject()
        {
            return _Get();
        }

        protected virtual object _Get()
        {
            return null;
        }

        public void Release(PooledClass obj)
        {
            this.pool.Add(obj);
        }

        public void Clear()
        {
            reqSeq = 0;
            pool.Clear();
        }
    }
}
