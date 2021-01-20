
using UnityEngine;


namespace Framework
{

    public abstract class SingletonMonoImpl<T> where T : MonoComponent
    {
        private static T _Instance = null;

        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        return null;
                    }
#endif


#if UNITY_EDITOR
                    if (GameObject.FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("More than 1!");

                        //return _Instance;
                    }
#endif
                    string instanceName = typeof(T).Name;

#if UNITY_EDITOR
                    Debug.Log("Instance Name: " + instanceName);
#endif

                    GameObject instanceGo = new GameObject(instanceName);
                    _Instance = instanceGo.AddComponent<T>();

#if UNITY_EDITOR
                    Debug.Log("Add New Singleton " + _Instance.name + " in Game!");
#endif
                }

                return _Instance;

            }
        }

        public static void Dispose()
        {
            _Instance = null;
        }
    }
}
