using UnityEngine;
/// <summary>
/// Describe:MonoBehaviour
/// </summary>
public static class MonoBehaviourExtendtions  
{
    public static T GetOrAddComponent<T>(this Component _go) where T : Component
    {
        T result = _go.GetComponent<T>();
        if (result == null)
        {
            result = _go.gameObject.AddComponent<T>();
        }
        return result;
    }

    public static T GetOrAddComponent<T>(this GameObject _go) where T : Component
    {
        T result = _go.GetComponent<T>();
        if (result == null)
        {
            result = _go.AddComponent<T>();
        }
        return result;
    }

}
