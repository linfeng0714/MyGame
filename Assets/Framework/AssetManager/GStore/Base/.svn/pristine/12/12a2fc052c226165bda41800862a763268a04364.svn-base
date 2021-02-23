using System;

namespace GStore.RW
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RWClassAttribute : Attribute
    {
        public int id = -1;
        public string classDescription = null;

        public RWClassAttribute(int id,string classDescription = "")
        {
            this.classDescription = classDescription;
            this.id = id;
        }

        public RWClassAttribute(string classDescription = "")
        {
            this.classDescription = classDescription;
        }
    }

    public class RWFieldAttribute : Attribute
    {
        public int id = -1;

        public RWFieldAttribute(int id)
        {
            this.id = id;
        }
    }
}