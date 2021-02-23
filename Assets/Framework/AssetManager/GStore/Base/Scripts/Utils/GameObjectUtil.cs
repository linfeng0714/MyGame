using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// 工具类，存放一些通用的静态函数
/// </summary>
public class GameObjectUtil
{
    /// <summary>
    /// 为物件添加组件(此方法不适用于 - 刚销毁,马上又要加的情况)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_obj"></param>
    /// <returns></returns>
    public static T AddComponent<T>(GameObject _obj) where T : Component
    {
        if (_obj == null)
        {
            return null;
        }

        T _t = _obj.GetComponent<T>();
        if (_t == null)
        {
            _t = _obj.AddComponent<T>();
        }
        return _t;
    }
}
