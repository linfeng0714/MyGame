using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GStore
{
    public static class StaticUtility
    {
        /// <summary>
        /// 连接文件路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string Combine(this string path, string path2)
        {
            return Path.Combine(path, path2);
        }

        public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute
        {
#if NETFX_CORE
			return (T)type.GetTypeInfo().GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#else
            return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#endif
        }

        public static T RTGetAttribute<T>(this FieldInfo type, bool inherited) where T : Attribute
        {
#if NETFX_CORE
			return (T)type.GetTypeInfo().GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#else
            return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
#endif
        }
    }

   
}
