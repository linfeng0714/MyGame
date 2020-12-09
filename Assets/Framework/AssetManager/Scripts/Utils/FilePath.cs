using System.Collections.Generic;
using System.IO;


namespace Framework.AssetManager
{
    public class FilePath
    {
        /// <summary>
        /// 快取路徑是否存在，暫時性解決 5.1.2p1 SD Card IO 卡的問題。
        /// </summary>
        private static Dictionary<int, bool> _fileExistsCache = new Dictionary<int, bool>();

        /// <summary>
        /// 快取路徑是否存在，暫時性解決 5.1.2p1 SD Card IO 卡的問題。
        /// </summary>
        public static bool Exists(string path)
        {
            int pathHash = path.GetHashCode();
            bool isExists;
            if (_fileExistsCache.ContainsKey(pathHash))
            {
                isExists = _fileExistsCache[pathHash];
            }
            else
            {
                isExists = File.Exists(path);
                _fileExistsCache.Add(pathHash, isExists);
            }
            return isExists;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void Clear()
        {
            _fileExistsCache.Clear();
        }
    }
}
