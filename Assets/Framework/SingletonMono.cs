using UnityEngine;


namespace Framework
{
    public class SingletonMono<T> : MonoComponent where T : MonoComponent
    {
        public static T Instance
        {
            get { return SingletonMonoImpl<T>.Instance; }
        }

        protected override void _OnDestroy()
        {
            base._OnDestroy();
            SingletonMonoImpl<T>.Dispose();
        }
    }
}