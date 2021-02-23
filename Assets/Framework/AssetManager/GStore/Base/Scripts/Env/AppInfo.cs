using LitJson;
using System.IO;
using System.Collections.Generic;

namespace GStore
{
    static public class AppInfo
    {
        /// <summary>
        /// 游戏版本号
        /// </summary>
        public static int appVersion { get; private set; }
        /// <summary>
        /// 资源版本号
        /// </summary>
        public static long resVersion { get; private set; }

        /// <summary>
        /// 包体游戏版本号
        /// </summary>
        public static int game_version { get; private set; }
        /// <summary>
        /// 包体资源版本号
        /// </summary>
        public static long res_version { get; private set; }
        /// <summary>
        /// 游戏渠道号
        /// </summary>
        public static int channel;

        public static List<long> hadDownSmallPakcageList = new List<long>();

        private static string filePath
        {
            get { return System.IO.Path.Combine(AssetPathDefine.webBasePath, "record.txt"); }
        }

        public const string AllRemoveRes = "all_remove_res";
        public const string AppVersion = "app_version";
        public const string ResVersion = "res_version";
        public const string HadDownSmallPakcageList = "had_down_smallPackage_list";

        public static void Init()
        {
            hadDownSmallPakcageList.Clear();
            string _text_str = FileUtil.ReadAllText(filePath);
            if (!string.IsNullOrEmpty(_text_str))
            {
                JsonData jdata = JsonMapper.ToObject(_text_str);
                appVersion = GetIntDataByJson(AppVersion, jdata);
                resVersion = GetLongDataByJson(ResVersion, jdata);
                string list = GetStringDataByJson(HadDownSmallPakcageList,jdata);
                if(list != null)
                {
                    string[] sp = list.Split('~');
                    if(sp != null)
                    {
                        foreach(string item in sp)
                        {
                            int value = 0;
                            int.TryParse(item,out value);
                            hadDownSmallPakcageList.Add(value);
                        }   
                    }
                }
            }
            else
            {
                appVersion = 0;
                resVersion = 0;
            }
        }

        public static void SaveAll(int gameVersion, long res_Version,long hadDownSmallPakcageVersion = 0)
        {
            appVersion = gameVersion;
            resVersion = res_Version;
            if(hadDownSmallPakcageVersion != 0 && !hadDownSmallPakcageList.Contains(hadDownSmallPakcageVersion))
            {
                hadDownSmallPakcageList.Add(hadDownSmallPakcageVersion);
            }
            JsonData jdata = new JsonData();
            jdata[AppVersion] = appVersion.ToString();
            jdata[ResVersion] = resVersion.ToString();
            string str = "";
            for(int i = 0; i < hadDownSmallPakcageList.Count; i ++)
            {
                str += hadDownSmallPakcageList[i].ToString();
                if(i != hadDownSmallPakcageList.Count - 1)
                {
                    str += "~";
                }
            }
            jdata[HadDownSmallPakcageList] = str;
            FileUtil.WriteAllText(filePath, jdata.ToJson());
        }

        /// <summary>
        /// 配置包体游戏版本号，资源版本号，移除号（覆盖安装时移除数据），渠道号
        /// </summary>

        public static void Setup(int gameVersion, long resVersion, int tempChannel)
        {
            game_version = gameVersion;
            res_version = resVersion;
            channel = tempChannel;
        }

        public static bool IsDownSmallPackage(long resVersion)
        {
            if(hadDownSmallPakcageList.Contains(resVersion))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsRecord()
        {
            return File.Exists(filePath);
        }

        public static void DelRecord()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static int GetIntDataByJson(string _property, JsonData jdata)
        {
            int data = 0;
            if (jdata != null)
            {
                if (jdata.Keys.Contains(_property))
                {
                    string str = jdata[_property].ToString();
                    int.TryParse(str, out data);
                }
            }
            return data;
        }

        public static long GetLongDataByJson(string _property, JsonData jdata)
        {
            long data = 0;
            if (jdata != null)
            {
                if (jdata.Keys.Contains(_property))
                {
                    string str = jdata[_property].ToString();
                    long.TryParse(str, out data);
                }
            }
            return data;
        }

        public static string GetStringDataByJson(string _property, JsonData jdata)
        {
            string str = null;
            if (jdata != null)
            {
                if (jdata.Keys.Contains(_property))
                {
                    str = jdata[_property].ToString();
                }
            }
            return str;
        }
        
    }
}

