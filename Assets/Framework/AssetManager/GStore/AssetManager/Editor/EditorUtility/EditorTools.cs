using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class EditorTools
{
    /// <summary>
    /// 判断当前是否运行在命令行模式
    /// </summary>
    public static bool isBatchMode
    {
        get
        {
            string commandLineOptions = System.Environment.CommandLine.ToLower();
            return commandLineOptions.Contains("-batchmode");
        }
    }

    /// <summary>
    /// 刷新间隔
    /// </summary>
    public static float displayProgressBarInterval = 0.5f;
    private static double lastDisplayProgressTime = 0;

    /// <summary>
    /// 间隔一定时间才刷新显示的progressBar
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="process"></param>
    public static void DisplayProgressBar(string title, string info, float process)
    {
        //开始和结束一定会显示
        if (process >= 1 || process <= 0)
        {
            EditorUtility.DisplayProgressBar(title, info, process);
            return;
        }
        if (EditorApplication.timeSinceStartup - lastDisplayProgressTime > displayProgressBarInterval)
        {
            lastDisplayProgressTime = EditorApplication.timeSinceStartup;
            EditorUtility.DisplayProgressBar(title, info, process);
        }
    }

    /// <summary>
    /// 间隔一定时间才刷新显示的progressBar
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="process"></param>
    public static bool DisplayCancelableProgressBar(string title, string info, float process)
    {
        //开始和结束一定会显示
        if (process >= 1 || process <= 0)
        {
            return EditorUtility.DisplayCancelableProgressBar(title, info, process);
        }
        if (EditorApplication.timeSinceStartup - lastDisplayProgressTime > displayProgressBarInterval)
        {
            lastDisplayProgressTime = EditorApplication.timeSinceStartup;
            return EditorUtility.DisplayCancelableProgressBar(title, info, process);
        }
        return false;
    }
    public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 递归获取文件
    /// </summary>
    /// <param name="dirPath">文件夹路径</param>
    /// <param name="filterEx">过滤的后缀名</param>
    /// <param name="subPaths">返回的路径数组</param>
    public static void GetFiles(string dirPath, string[] filterEx, List<string> subPaths)
    {
        string[] fileNames = Directory.GetFiles(dirPath);
        string[] directories = Directory.GetDirectories(dirPath);
        foreach (string file in fileNames)
        {
            string filePath = file.Replace("\\", "/");
            if (filterEx.Count(p => p.ToLower().Equals(System.IO.Path.GetExtension(file).ToLower())) <= 0)
            {
                subPaths.Add(filePath);
            }
        }
        foreach (string dir in directories)
        {
            //过滤svn文件夹
            if (dir.Contains(".svn"))
            {
                continue;
            }
            GetFiles(dir, filterEx, subPaths);
        }
    }

    /// <summary>
    /// 递归获取文件夹
    /// </summary>
    /// <param name="dirPath">文件夹路径</param>
    /// <param name="dirs">返回的文件夹数组</param>
    public static void GetDirs(string dirPath, List<string> dirs)
    {
        string[] directories = Directory.GetDirectories(dirPath);
        dirs.AddRange(directories);
        foreach (string dir in directories)
        {
            GetDirs(dir, dirs);
        }
    }

    /// <summary>
    /// 读取整个文件夹的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static T[] LoadAllFromDir<T>(string dir) where T : UnityEngine.Object
    {
        if (Directory.Exists(dir) == false)
        {
            return null;
        }

        string[] directoryEntries;
        List<T> objList = new List<T>();
        directoryEntries = System.IO.Directory.GetFileSystemEntries(dir);

        for (int i = 0; i < directoryEntries.Length; i++)
        {
            string path = directoryEntries[i].Replace("\\", "/");
            if (path.EndsWith(".meta"))
            {
                continue;
            }
;
            T tempTex = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
            if (tempTex != null)
                objList.Add(tempTex);
        }
        if (objList.Count > 0)
            return objList.ToArray();
        return null;
    }

    public class ProgressBar
    {
        private string m_Title;
        private string m_Info;
        private float m_CurProcess;

        #region Internal

        private void _ShowDialog()
        {
            EditorUtility.DisplayProgressBar(m_Title, m_Info, m_CurProcess);
        }

        #endregion // Internal

        #region Show Or Hide

        public void Show(string title)
        {
            Show(title, string.Empty, 0.0f);
        }
        public void Show(string title, string info)
        {
            Show(title, info, 0.0f);
        }
        public void Show(string title, string info, float process)
        {
            m_Title = title;
            m_Info = info;
            m_CurProcess = process;

            _ShowDialog();
        }

        public void Hide()
        {
            EditorUtility.ClearProgressBar();
        }

        #endregion // Show Or Hide

        #region For Update

        public void SetInfo(string info)
        {
            m_Info = info;
            _ShowDialog();
        }
        public void SetProcess(float process)
        {
            m_CurProcess = process;
            _ShowDialog();
        }
        public void UpdateUI()
        {
            _ShowDialog();
        }

        #endregion // For Update



    }


    public class Object
    {
        public static bool IsFolder(UnityEngine.Object obj)
        {
            if (obj.GetType() == typeof(UnityEngine.Object))
            {
                string assertPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
                assertPath = Path.ConvertAssetPathToFullPath(assertPath);
                assertPath = Path.ConvertToSystemPath(assertPath);
                if (System.IO.Directory.Exists(assertPath))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class Path
    {
        //private static string RESOURCES_PATH = "Assets/Resources/";

        private static string m_ProjectAssetPrefixPath;

        // 构造
        static Path()
        {
            m_ProjectAssetPrefixPath = Application.dataPath;
            m_ProjectAssetPrefixPath = m_ProjectAssetPrefixPath.Replace("/Assets", "/");
        }

        public static string GetProjectAssetPrefixPath()
        {
            return m_ProjectAssetPrefixPath;
        }


        //  把"C:/XXXX/"路径转为"C:\XXXX\"路径(windows下)
        public static string ConvertToSystemPath(string path)
        {
            return path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        }

        // 把"Asset/XXX" 项目路径转换成 "C:/Asset/XXX"全路径(Unity路径格式)
        public static string ConvertAssetPathToFullPath(string assetPath)
        {
            return m_ProjectAssetPrefixPath + assetPath;
        }

        // 确保目录已经存在(path不可以是文件名,只能是目录名)(必需是系统路径格式)
        public static void MakeSureDirExist(string path)
        {
            try
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
                if (dirInfo.Exists)
                {
                    return;
                }

                dirInfo.Create();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

}
