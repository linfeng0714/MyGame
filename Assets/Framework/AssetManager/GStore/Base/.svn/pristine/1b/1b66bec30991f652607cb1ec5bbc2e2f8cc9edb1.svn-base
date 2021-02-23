using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GStore
{
    public class EncryptTool
    {
        //加密算法
        public static IEncrypt encryptAlg = new CustomEncrypt();
        public static IEncrypt abEncrypt = new ABEncrypt();
        private static string log;
        public static string logFileName = "GlobalGameManager.lua";
        public static bool isShowAll = false;


        public static byte[] Encrypt(byte[] byteText, string filePath = "")
        {
#if Encrypt
            Stopwatch watch = null;
            bool isLog = filePath.Contains(logFileName) || isShowAll;
            if (isLog)
            {
                watch = new Stopwatch();
                watch.Start();
            }
#endif
            byte[] bytes = encryptAlg.Encrypt(byteText, filePath);
#if Encrypt
            if (isLog)
            {
                watch.Stop();
                string str = string.Format("加密文件：{0} 加密时间：{1}\n加密前数据：{2}\n加密后数据：{3}\n", filePath, watch.Elapsed, GetStrByByteList(byteText), GetStrByByteList(bytes));
                Log(str);
            }
#endif
            return bytes;
        }

        public static byte[] Decrypt(byte[] showText, string filePath = "")
        {
#if Encrypt
            Stopwatch watch = null;
            bool isLog = filePath.Contains(logFileName) || isShowAll;
            if (isLog)
            {
                watch = new Stopwatch();
                watch.Start();
            }
#endif
            byte[] bytes = encryptAlg.Decrypt(showText, filePath);
#if Encrypt
            if (isLog)
            {
                watch.Stop();
                string str = string.Format("解密文件：{0} 解密时间：{1}\n解密前数据：{2}\n解密后数据：{3}\n", filePath, watch.Elapsed, GetStrByByteList(showText), GetStrByByteList(bytes));
                Log(str);
            }
#endif
            return bytes;
        }

        public static byte[] ABEncrypt(byte[] byteText, string filePath = "")
        {
#if Encrypt
            Stopwatch watch = null;
            bool isLog = filePath.Contains(logFileName) || isShowAll;
            if (isLog)
            {
                watch = new Stopwatch();
                watch.Start();
            }
#endif
            byte[] bytes = abEncrypt.Encrypt(byteText, filePath);
#if Encrypt
            if (isLog)
            {
                watch.Stop();
                string str = string.Format("加密文件：{0} 加密时间：{1}\n加密前数据：{2}\n加密后数据：{3}\n", filePath, watch.Elapsed, GetStrByByteList(byteText), GetStrByByteList(bytes));
                Log(str);
            }
#endif
            return bytes;
        }

        public static void Log(string logTemp)
        {
            if (logTemp != null)
            {
                string str = logTemp + "\n";
                log += str;
                UnityEngine.Debug.LogErrorFormat(str);
            }
        }

#if UNITY_EDITOR
        private static Vector2 scrollPos;
        public static void OnGUI()
        {
            GUI.skin.verticalScrollbar.fixedWidth = 50;
            GUI.skin.verticalScrollbarThumb.fixedWidth = 50;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(Screen.width * 0.15f);
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.5f));
                {
                    GUILayout.BeginVertical(GUILayout.Height(Screen.height * 0.5f));
                    {
                        scrollPos = GUILayout.BeginScrollView(scrollPos);
                        {
                            GUILayout.Label(log);
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }
#endif

        public static string GetStrByByteList(byte[] byteList, bool showAll = false, bool isByts = false)
        {
            string str = "";
            str += string.Format("长度：{0}\n", byteList.Length);
            if (byteList != null)
            {
                if (byteList.Length >= 50 && !showAll)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        string s = byteList[i].ToString();
                        if (isByts)
                        {
                            s = Convert.ToString(byteList[i], 2);
                        }
                        str += string.Format("{0}|", s);
                    }
                    for (int i = byteList.Length - 25; i < byteList.Length; i++)
                    {
                        string s = byteList[i].ToString();
                        if (isByts)
                        {
                            s = Convert.ToString(byteList[i], 2);
                        }
                        str += string.Format("{0}|", s);
                    }
                }
                else
                {
                    for (int i = 0; i < byteList.Length; i++)
                    {
                        string s = byteList[i].ToString();
                        if (isByts)
                        {
                            s = Convert.ToString(byteList[i], 2);
                        }
                        str += string.Format("{0}|", s);
                    }
                }

                //return byteList[0].ToString();
            }
            return str;
        }
    }
}

