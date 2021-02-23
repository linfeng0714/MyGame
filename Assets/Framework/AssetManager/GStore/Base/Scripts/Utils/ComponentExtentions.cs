using UnityEngine;

namespace GStore
{
    /// <summary>
    /// Component 扩展类
    /// </summary>
    public static class ComponentExtentions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            T result = component.GetComponent<T>();
            if (result == null)
            {
                result = component.gameObject.AddComponent<T>();
            }
            return result;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T result = gameObject.GetComponent<T>();
            if (result == null)
            {
                result = gameObject.AddComponent<T>();
            }
            return result;
        }
    }
}
