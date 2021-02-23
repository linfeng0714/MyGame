using UnityEngine;
using System.Diagnostics;

/// <summary>
/// 继承Unity的MonoBehavior的单例类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : Component
{
    protected static T ms_instance;
    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                //先找是否存在
                ms_instance = FindObjectOfType<T>();
                if (ms_instance == null)
                {
                    CreateInstance();
                }
                DontDestroyOnLoad(ms_instance.gameObject);
            }
            return ms_instance;
        }
    }

    /// <summary>
    /// 创建一个名称为T的GameObject，并且挂载名称为T的Script，单例保证只有一个T的脚本存在
    /// </summary>
    /// <returns></returns>
    private static T CreateInstance()
    {
        if (ms_instance == null)
        {
            GameObject go = new GameObject(typeof(T).Name);            
            T t = go.AddComponent<T>();
            ms_instance = t;
        }
        return ms_instance;
    }
}
