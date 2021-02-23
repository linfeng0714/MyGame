using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GStore
{
    /// <summary>
    /// StreamingAssets目录工具类
    /// </summary>
    public static class StreamingAssets
    {
        private static string streamingAssetsPath;
        private static RuntimePlatform platform;

        /// <summary>
        /// 主线程初始化
        /// </summary>
        public static void Initialize()
        {
            streamingAssetsPath = Application.streamingAssetsPath;
            platform = Application.platform;
        }

        public static string IndexPath
        {
            get { return streamingAssetsPath + "/index"; }
        }

        private static HashSet<string> s_Indices;
        private static HashSet<string> Indices
        {
            get
            {
                if (s_Indices == null)
                {
                    byte[] bytes = LoadBytes(IndexPath);
                    var utf8 = new UTF8Encoding(false);
                    var text = utf8.GetString(bytes);
                    s_Indices = new HashSet<string>(text.Split('\n'), StringComparer.OrdinalIgnoreCase);
                }
                return s_Indices;
            }
        }

        public static bool ShouldPathUseWebRequest(string path)
        {
            return path != null && path.Contains("://");
        }

        public static byte[] LoadBytes(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else if (ShouldPathUseWebRequest(path))
            {
                //Android平台需要用webRequest读取
                UnityWebRequest request = new UnityWebRequest(path, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                var operation = request.SendWebRequest();
                while (operation.isDone == false) { }
                return request.downloadHandler.data;
            }
            return null;
        }

#if UNITY_EDITOR
        public static void Build()
        {
            var files = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories)
                .Where((path) => { return Path.GetExtension(path) != ".meta"; })
                .Select((path) => path.Substring(Application.streamingAssetsPath.Length + 1).Replace(@"\", "/"));

            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);

            foreach (var line in files)
            {
                writer.Write(line);
                writer.Write('\n');
            }
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(IndexPath, builder.ToString(), utf8);
            s_Indices = null;
        }
#endif
        public static bool Exists(string path)
        {
            if (platform == RuntimePlatform.Android)
            {
                path = path.Replace(@"\", "/");
                return Indices.Contains(path);
            }
            else
            {
                return File.Exists(streamingAssetsPath + "/" + path);
            }
        }
    }
}
