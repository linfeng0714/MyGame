#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

public class CommanLineUtil
{
    static string svn_exe_path = Application.dataPath + "/../../../doc/tool/svn/bin/svn.exe";

    public static bool Execute(string command, int seconds)
    {
        UnityEngine.Debug.LogFormat("command {0}", command);

        bool isSuccess = false;
        string output = ""; //输出字符串  
        if (command != null && !command.Equals(""))
        {
            Process process = new Process(); //创建进程对象
            ProcessStartInfo startInfo = new ProcessStartInfo();

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                startInfo.FileName = @"/bin/bash"; //设定需要执行的命令
                startInfo.Arguments = "-c \" " + command + " \""; //“/C”表示执行完命令后马上退出
            }
            else
            {
                startInfo.FileName = @"C:\Windows\System32\cmd.exe"; //设定需要执行的命令
                startInfo.Arguments = "/C " + command; //“/C”表示执行完命令后马上退出
            }
           
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true; //重定向输入
            startInfo.RedirectStandardOutput = true; //重定向输出,才有StandardOutput
            startInfo.RedirectStandardError = true;//重定向输出错误,才有StandardError
            startInfo.CreateNoWindow = true; //不创建窗口
            process.StartInfo = startInfo;
            try
            {
                if (process.Start())//开始进程  
                {
                    if (seconds == 0)
                    {
                        process.WaitForExit();//这里无限等待进程结束  
                    }
                    else
                    {
                        process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒
                    }
                    string error = process.StandardError.ReadToEnd();
                    output = process.StandardOutput.ReadToEnd() + error;//读取进程的输出

                    if (process.ExitCode == 0)
                    {
                        isSuccess = true;
                    }

                    UnityEngine.Debug.LogFormat("output \n{0}", output);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            finally
            {
                if (process != null)
                    process.Close();
            }
        }
        return isSuccess;
    }

    public static bool SvnCommand(string option, string path, string subOption = "")
    {
        string command = string.Format(@"{0} {1} {2} {3}", "svn", option, path, subOption);
        return Execute(command, 0);
    }

    public static bool SvnCommit(string commitMsg, params object[] path)
    {
        string command = string.Format("{0} commit -m \"{1}\"", "svn", commitMsg);
        //string command = svn_exe_path + " commit -m " + commitMsg;
        for (int i = 0; i < path.Length; i++)
        {
            command += " " + path[i];
        }
        return Execute(command, 0);
    }
}
#endif
