using UnityEngine;


namespace Framework
{
    public abstract class MonoComponent : UnityEngine.MonoBehaviour
    {
        #region field
        private Transform _CacheTransform = null;
        private GameObject _GameGo = null;
        private string _Name = string.Empty;
        //private string _Tag = string.Empty;
        private bool _isAwake = false;
        #endregion


        #region property
        public new string name
        {
            get
            {
                if (string.IsNullOrEmpty(this._Name))
                {
                    this._Name = base.name;
                }
                return this._Name;
            }
            set
            {
                this._Name = value;
                base.name = value;
            }
        }
        //         public new string tag
        //         {
        //             get { return this._Tag;  }
        //             set
        //             {
        //                 this._Tag = value;
        //                 base.tag = value;
        //             }
        //         }

        public new Transform transform
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return base.transform;
                }
#endif
                return this._CacheTransform;
            }
        }
        public new GameObject gameObject
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return base.gameObject;
                }
#endif
                return this._GameGo;
            }
        }
        #endregion


        protected void Awake()
        {
            this._GameGo = base.gameObject;
            this._CacheTransform = base.transform;
            //this._Tag = base.tag;
            //this._Name = base.name;
            FastOnAwake();
        }

        protected void OnDestroy()
        {
            FastOnDestroy();
            this._GameGo = null;
            this._CacheTransform = null;
        }


        public void FastOnAwake()
        {
            if (!_isAwake)
            {
                _isAwake = true;
                _OnAwake();
            }
        }

        public void FastOnDestroy()
        {
            if (_isAwake)
            {
                _isAwake = false;
                _OnDestroy();
            }
        }

        public void SetAcitve(bool b)
        {
            gameObject.SetActive(b);
            if (b) _OnActive();
            else _OnDeActive();
        }

        protected virtual void _OnAwake()
        {
            // empty
        }
        protected virtual void _OnActive()
        {
            // empty
        }
        protected virtual void _OnDeActive()
        {
            // empty
        }
        protected virtual void _OnDestroy()
        {
            // empty
        }
    }
}