using System;

namespace Framework.Base
{
    public class ClassPool<T> : ClassPoolBase where T : PooledClass
    {
        private static ClassPool<T> _instance;
        private static ClassPool<T> instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClassPool<T>();
                }
                return _instance;
            }
        }

        public static uint NewSeq()
        {
            instance.reqSeq += 1u;
            return instance.reqSeq;
        }

        public static T Get()
        {
            return instance.GetObject() as T;
        }

        protected override object _Get()
        {
            if (instance.pool.Count > 0)
            {
                T t = (T)(instance.pool[instance.pool.Count - 1]);
                instance.pool.RemoveAt(instance.pool.Count - 1);
                t.usingSeq = NewSeq();
                t.holder = instance;
                t.OnUse();
                return t;
            }
            T t2 = Activator.CreateInstance(typeof(T)) as T;
            t2.usingSeq = NewSeq();
            t2.holder = instance;
            t2.OnUse();
            return t2;
        }
    }

}