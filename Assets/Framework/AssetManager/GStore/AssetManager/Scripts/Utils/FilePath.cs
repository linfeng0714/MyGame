using System.Collections.Generic;
using System.IO;


namespace GStore
{
    public class FilePath
    {
        /// <summary>
        /// 快取路徑是否存在，暫時性解決 5.1.2p1 SD Card IO 卡的問題。
        /// </summary>
        private static Dictionary<int, bool> m_FileExistsCache = new Dictionary<int, bool>();

        /// <summary>
        /// 快取路徑是否存在，暫時性解決 5.1.2p1 SD Card IO 卡的問題。
        /// </summary>
        public static bool Exists(string path)
        {
            int pathHash = path.GetHashCode();
            bool isExists;
            if (m_FileExistsCache.ContainsKey(pathHash))
            {
                isExists = m_FileExistsCache[pathHash];
            }
            else
            {
                isExists = File.Exists(path);
                m_FileExistsCache.Add(pathHash, isExists);
            }
            return isExists;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void Clear()
        {
            m_FileExistsCache.Clear();
        }
    }
}
