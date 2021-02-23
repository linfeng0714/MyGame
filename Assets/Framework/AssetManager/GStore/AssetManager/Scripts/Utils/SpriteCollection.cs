using System;
using System.Collections.Generic;
using UnityEngine;

namespace GStore
{
    /// <summary>
    /// Sprite集合 - 支持single和multiply模式
    /// </summary>
    public class SpriteCollection
    {
        /// <summary>
        /// 集合字典
        /// </summary>
        private Dictionary<string, Sprite> m_CollectionDict;

        /// <summary>
        /// 所有Sprite
        /// </summary>
        private Sprite[] m_AllSprite;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="allSprite"></param>
        public SpriteCollection(Sprite[] allSprite)
        {
            m_AllSprite = allSprite;
            m_CollectionDict = new Dictionary<string, Sprite>(allSprite.Length);
            for (int i = 0; i < allSprite.Length; i++)
            {
                Sprite sprite = allSprite[i];
                if (m_CollectionDict.ContainsKey(sprite.name))
                {
                    throw new System.ArgumentException(string.Format("图集中存在重复的spriteName:{0}", sprite.name));
                }
                m_CollectionDict[sprite.name] = sprite;
            }
        }

        /// <summary>
        /// 获取Sprite
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string spriteName)
        {
            return m_CollectionDict[spriteName];
        }

        /// <summary>
        /// 获取Sprite
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public bool TryGetSprite(string spriteName, out Sprite sprite)
        {
            return m_CollectionDict.TryGetValue(spriteName, out sprite);
        }

        /// <summary>
        /// 获取所有Sprite
        /// </summary>
        /// <returns></returns>
        public Sprite[] GetAllSprite()
        {
            return m_AllSprite;
        }

        /// <summary>
        /// 获取Sprite
        /// </summary>
        /// <returns></returns>
        public Sprite GetSingleSprite()
        {
            if (m_AllSprite == null || m_AllSprite.Length <= 0)
            {
                Debug.LogError("集合中没有Sprite！");
                return null;
            }
            return m_AllSprite[0];
        }
    }
}
