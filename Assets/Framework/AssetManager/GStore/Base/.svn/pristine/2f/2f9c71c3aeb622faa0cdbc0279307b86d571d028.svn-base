using System;
using System.Collections.Generic;
using System.Reflection;

namespace GStore
{
    /// <summary>
    /// 反射扩展方法
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// 反射获取私有属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetPrivateProperty<T>(this object obj, string propertyName)
        {
            Type type = obj.GetType();

            var property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            object val = property.GetValue(obj, null);
            return (T)val;
        }

        /// <summary>
        /// 反射获取私有变量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();

            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        /// <summary>
        /// 反射获取静态私有变量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetPrivateStaticField<T>(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            return (T)field.GetValue(null);
        }
    }
}
