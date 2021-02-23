using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GStore
{
    public static class CommandLine
    {
#if UNITY_EDITOR_WIN
        public static string RSYNC_COMMAND = ProjectBuilderSettings.kModuleRootFolder + "/Shell/cwRsync/bin/rsync.exe";
#else    
        public static string RSYNC_COMMAND = "rsync";
#endif

        public static int Rsync(string src, string dist, string args)
        {
#if UNITY_EDITOR_WIN
            //Windows路径需要转换成cyg路径才能被cwRsync识别
            src = ConvertCygPath(src);
            dist = ConvertCygPath(dist);
#endif
            return Execute(RSYNC_COMMAND, string.Format("{0} {1} {2}", args, src, dist), throwException: false);
        }

        /// <summary>
        /// 转换成cygPath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string ConvertCygPath(string path)
        {
            path = Path.GetFullPath(path);
            path = path.Replace("\\", "/");
            int index = path.IndexOf(":");
            string drive = path.Substring(0, index);
            string subPath = path.Substring(index + 1);
            return string.Format("/cygdrive/{0}{1}", drive, subPath);
        }

        /// <summary>
        /// 命令行工具
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static int Execute(string command, string[] arguments = null, bool useShellExecute = false, bool throwException = true)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append("\"" + arguments[i] + "\"");
                    sb.Append(" ");
                }
            }
            return Execute(command, sb.ToString(), useShellExecute, throwException);
        }

        /// <summary>
        /// 命令行工具
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static int Execute(string command, string arguments, bool useShellExecute = false, bool throwException = true)
        {
            int exitCode = 0;

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(command);
            startInfo.Arguments = arguments;
            startInfo.CreateNoWindow = true;
            startInfo.ErrorDialog = true;
            startInfo.UseShellExecute = useShellExecute;
            if (startInfo.UseShellExecute)
            {
                startInfo.RedirectStandardOutput = false;
                startInfo.RedirectStandardError = false;
                startInfo.RedirectStandardInput = false;
            }
            else
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);

            if (startInfo.UseShellExecute == false)
            {
                UnityEngine.Debug.LogWarning(process.StandardOutput.ReadToEnd());
            }

            //等待shell脚本执行完毕
            process.WaitForExit();
            exitCode = process.ExitCode;
            process.Close();

            if (throwException && exitCode != 0)
            {
                System.Exception e = new System.Exception("execute " + command + " failed! exitCode = " + exitCode);
                throw e;
            }
            return exitCode;
        }
    }
}
