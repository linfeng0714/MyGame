using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore.RW
{
    public class RWBaseObject
    {
        public virtual int ClassNameID() { return -1; }
        public virtual string NameSpace() { return ""; }


        public virtual void Order(RWContext c, int fieldNum = -1, string fieldName = null)
        {
        }

        public virtual void SwitchOrder(RWContext c, int fieldNum = -1, string fieldName = null)
        {
            c.rwType.SkipField();
        }

        public virtual RWBaseObject GetCloneObj()
        {
            return null;
        }

        public virtual void AutoCloneData(RWBaseObject clone)
        {
        }
    }
}
