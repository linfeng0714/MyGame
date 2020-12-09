
namespace Framework.Base
{
    /// <summary>
    /// 工具类，存放一些通用的静态函数
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// 获取去除BOM的字符串
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string GetStringWithoutBOM(byte[] array)
        {
            if (array.Length > 3)
            {
                //去除bom
                if (array[0] == 0xef && array[1] == 0xbb && array[2] == 0xbf)
                {
                    return System.Text.Encoding.UTF8.GetString(array, 3, array.Length - 3);
                }
            }

            return System.Text.Encoding.UTF8.GetString(array, 0, array.Length);
        }

        /// <summary>
        /// 获取文件前缀，例如 table/hero.csv -> table/hero
        /// </summary>
        /// <param name="_filename"></param>
        /// <returns></returns>
        public static string GetPrefix(string _filename, string _char)
        {
            int index = _filename.LastIndexOf(_char);
            if (index > 0)
            {
                _filename = _filename.Substring(0, index);
            }
            return _filename;
        }

        /// <summary>
        /// 获取文件后缀，例如hero.csv -> .csv
        /// </summary>
        /// <param name="_filename"></param>
        /// <param name="_char"></param>
        /// <returns></returns>
        public static string GetSuffix(string _filename, string _char)
        {
            int index = _filename.LastIndexOf(_char);
            if (index > 0)
            {
                _filename = _filename.Substring(index);
            }
            return _filename;
        }

        /// <summary>
        /// 获取文件名ID，例如ai_9999 = 9999
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetFileID(string name)
        {
            int index = name.IndexOf('_');
            if (index > 0)
            {
                return int.Parse(name.Substring(index + 1, name.Length - index - 1));
            }
            return int.Parse(name);
        }

    }
}
