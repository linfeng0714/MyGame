using System;

namespace Framework.Base
{
    public interface IObjPoolCtrl
    {
        void Release(PooledClass obj);

        void Clear();
    }

}
