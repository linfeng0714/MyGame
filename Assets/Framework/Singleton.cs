using System;

namespace Framework
{
    /// <summary>
    /// 游戏单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        private static T ms_instance;

        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (ms_instance == null)
                {
                    ms_instance = Activator.CreateInstance<T>();
                }
                return ms_instance;
            }
        }

        public static void ReleaseInstance()
        {
            ms_instance = default(T);
        }
    }
}
