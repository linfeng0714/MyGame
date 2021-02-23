using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore.RW
{
    public class RWContext
    {
        int classId = -1;
        public IReadOrWrite rwType;
        public bool isEditor = true;
        public bool isReadContext = false;//是否是读Context

        public delegate object GetRWObject (string nameSpace, string className, int classId);
        public static GetRWObject getRWObject;

        public virtual object ObjectFactory (string nameSpace, string className, int classId)
        {
            if (getRWObject != null)
            {
                return getRWObject (nameSpace, className, classId);
            }

            else
            {
                return null;
            }
        }

    }
}