using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace GStore
{
    public class FileCommitor
    {
        /// <summary>
        /// 有修改的文件列表
        /// </summary>
        private List<string> m_FileList = new List<string>();

        /// <summary>
        /// 添加要提交的文件
        /// </summary>
        /// <param name="file"></param>
        public void Add(string file)
        {
            m_FileList.Add(file);
        }

        /// <summary>
        /// 清除列表
        /// </summary>
        public void Clear()
        {
            m_FileList.Clear();
        }

        public static void Commit(string[] files, string message)
        {
            if (files.Length == 0)
            {
                return;
            }

            //目前仅实现了OSX下提交的功能
#if UNITY_EDITOR_OSX
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string command = Path.GetFullPath(ProjectBuilderSettings.kModuleRootFolder + "/Shell/commit.sh");

            string[] args = new string[files.Length + 1];
            args[0] = message;

            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];

                //修复svn命令识别@符号
                if (path.Contains("@"))
                {
                    path += "@";
                }
                args[i + 1] = path;
            }
            CommandLine.Execute(command, args, true);

            sw.Stop();
            Debug.LogWarning(string.Format("提交SVN总共耗时:{0}", sw.Elapsed.ToString()));
#endif
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="message"></param>
        public void Commit(string message)
        {
            Commit(m_FileList.ToArray(), message);
        }
    }
}
