using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GStore
{
    public static class FileUtil
    {
        public static string ReadAllText(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("ReadAllText Exception {0}", ex);
                return null;
            }
        }

        public static bool WriteAllText(string path, string text)
        {
            bool isSucess = false;
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            try
            {
                File.WriteAllText(path, text, Encoding.UTF8);
                isSucess = true;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("WriteAllText Exception {0}", ex);
                isSucess = false;
            }
            return isSucess;
        }

        public static bool WriteTxtLine(string filePath, string data)
        {
            bool isSucess = false;
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            FileStream stream = null;
            StreamWriter writer = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    stream = File.Create(filePath);
                    stream.Close();
                }
                stream = File.Open(filePath, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(stream, Encoding.UTF8);
                //Debug.LogErrorFormat("data {0}", data);
                writer.WriteLine(data);
                isSucess = true;
            }
            catch (Exception ex)
            {
                isSucess = false;
                Debug.LogErrorFormat("WriteTxtLine Exception {0}", ex);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return isSucess;
        }

        /// <summary>
        /// 拷贝文件到指定目录
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="desDir"></param>
        public static void CopyDirFile(string sourceDir, string desDir, string title, bool isEncry = false, string[] exceptsFloder = null, string[] exceptsSuffix = null)
        {
            List<string> excepts = null;
            if (exceptsFloder != null)
            {
                excepts = exceptsFloder.ToList();
            }
            List<string> files = new List<string>();
            Recursive(sourceDir, ref files, excepts);

            foreach (string f in files)
            {
                if (f.EndsWith(".meta"))
                    continue;
                if (exceptsSuffix != null)
                {
                    bool isContinue = false;
                    foreach (string suffix in exceptsSuffix)
                    {
                        if (f.EndsWith(suffix))
                        {
                            isContinue = true;
                        }
                    }
                    if (isContinue)
                    {
                        continue;
                    }
                }

                string newfile = f.Replace(sourceDir, "");
                string newpath = desDir + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                byte[] src = File.ReadAllBytes(f);
                if (isEncry)
                {
                    src = GStore.EncryptTool.Encrypt(src);
                }
                File.WriteAllBytes(newpath, src);
            }
        }

        /// <summary>
        /// 递归获取目录下多所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private static void Recursive(string path, ref List<string> files, List<string> exceptFloder = null)
        {
            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string filename in names)
            {
                string ext = Path.GetExtension(filename);
                if (ext.Equals(".meta"))
                    continue;
                files.Add(filename.Replace('\\', '/'));
            }
            foreach (string dir in dirs)
            {
                if (exceptFloder != null)
                {
                    string relativePath = dir.Replace("\\", "/").Replace(path + "/", "");
                    int index = exceptFloder.FindIndex((name) =>
                    {
                        return name == relativePath;
                    });
                    if (index >= 0)
                    {
                        continue;
                    }
                }
                if (dir.EndsWith(".svn"))
                    continue;
                Recursive(dir, ref files);
            }
        }
    }
}
