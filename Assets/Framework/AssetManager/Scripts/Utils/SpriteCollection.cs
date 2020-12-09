using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class SpriteCollection 
    {
        private Dictionary<string, Sprite> _collectionDict;

        private Sprite[] _allSprite;

        public SpriteCollection(Sprite[] sprites)
        {
            _allSprite = sprites;
            _collectionDict = new Dictionary<string, Sprite>(_allSprite.Length);
            for (int i = 0; i < _allSprite.Length; i++)
            {
                Sprite sprite = _allSprite[i];
                if (_collectionDict.ContainsKey(sprite.name))
                {
                    throw new System.ArgumentException(string.Format("图集中存在重复的spriteName:{0}", sprite.name));
                }
                _collectionDict[sprite.name] = sprite;
            }
        }

        public Sprite GetSprite(string spriteName)
        {
            return _collectionDict[spriteName];
        }

        public bool TryGetSprite(string spriteName, out Sprite sprite)
        {
            return _collectionDict.TryGetValue(spriteName, out sprite);
        }
        /// <summary>
        /// 获取全部Sprite
        /// </summary>
        /// <returns></returns>
        public Sprite[] GetAllSprite()
        {
            return _allSprite;
        }

        public Sprite GetSingleSprite()
        {
            if (_allSprite == null || _allSprite.Length <= 0)
            {
                Debug.LogError("集合中没有Sprite！");
                return null;
            }
            return _allSprite[0];
        }
    }
}

