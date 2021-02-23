using LitJson;
using System.Text;
using System.Text.RegularExpressions;

namespace GStore
{
    /// <summary>
    /// Json工具
    /// </summary>
    public static class JsonUtil
    {
        /// <summary>
        /// 输出格式好看的json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToPrettyJson(object obj, string newLine = null)
        {
            return ToJson(obj, prettyPrint: true, newLine: newLine);
        }

        public static string ToJson(object obj, bool prettyPrint = false, bool unicodeEscape = true, string newLine = null)
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jsonWriter = new JsonWriter(sb);
            jsonWriter.PrettyPrint = prettyPrint;
            jsonWriter.UnicodeEscape = unicodeEscape;
            if (string.IsNullOrEmpty(newLine) == false)
                jsonWriter.NewLine = newLine;
            JsonMapper.ToJson(obj, jsonWriter);
            return sb.ToString();
        }

        /// <summary>
        /// Unicode转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UnicodeEscape(string input)
        {
            return Regex.Replace(input, @"[^\x00-\x7F]", c =>
         string.Format(@"\u{0:x4}", (int)c.Value[0]));
        }
    }

    /// <summary>
    /// 换行符定义
    /// </summary>
    public static class NewLine
    {
        public const string Windows = "\r\n";
        public const string Unix = "\n";
        public static string DEFAULT = System.Environment.NewLine;
    }
}

