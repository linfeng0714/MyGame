using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Security;
using GStore;

/// <summary>
/// 启动类型
/// </summary>
public enum ELaunch
{
    Local = 1,
    Network = 2,
    Editor = 3,
}

/// <summary>
/// 默认系统语言
/// </summary>
public enum ELanguageDefualtType
{
    None = 0,       //跟随系统
    ZH_Hans = 1,    //简体
    ZH_Hant = 2,    //繁体
    English = 3,    //英语
    Japan = 4,      //日语
}

public class SettingManager : Singleton<SettingManager>
{
    #region setup.xml
    /// <summary>
    /// 登陆服务器IP和端口
    /// </summary>
    public static string LoginServerIp { get; set; }
    public static int LoginServerPort { get; private set; }

    /// <summary>
    /// 游戏服务器IP和端口 - 临时等从服务器列表获取
    /// </summary>
    public static string GameServerIp { get; set; }
    public static int GameServerPort { get { return 7002; } }

    /// <summary>
    /// 预置服务器列表
    /// </summary>
    public static List<ServerInfo> ServerList { get; private set; }

    //基本配置
    public static int game_version { get; private set; }   // 游戏版本
    public static long res_version { get; private set; }       //资源版本
    public static int hot_update_config_id { get; private set; }    //对应的热更配置id

    public static bool isDevelop { get; private set; }//是否是开发模式
    public static bool isHotUpdate { get; private set; }//是否开启热更
    public static bool isEncrypt { get; private set; }//是否加密
    public static bool isSmallPackage { get; private set; }//是否是小包

    public static bool mDownloadingWhilePlaying = false;// 是否开启边玩边下
    public static bool mDwpBuildPackage = false;//是否打边玩边下的包
    public static bool mDwpCopyResToSvn = false;//是否将边玩边下资源拷贝到热更SVN
    public static int mDwpVersion = 0;//边玩边下的版本
    public static bool mDwpSupplementUpdate = false;//是否开启边玩边下的自动补充下载

    public static int network_type;

    /// <summary>
    /// 是否使用AB
    /// </summary>
    private bool use_ab = false;
    public bool UseAB { get { return use_ab; } private set { use_ab = value; } }

#endregion
    /// <summary>
    /// 加载数据
    /// </summary>
    public bool Init()
    {
        if (LoadSetup() == false)
        {
            return false;
        }
        //设置永不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //DevicePrefsManager.Instance.Load();
        return true;
    }

    /// <summary>
    /// 从StreamingAssets下读取setup.xml
    /// </summary>
    /// <returns></returns>
    private byte[] GetSetupBytes()
    {
        //IOS平台:Application.streamingAssetsPath = Application/xxxxx/xxx.app/Data/Raw
        //android平台:Application.streamingAssetsPath = jar:file:///data/app/xxx.xxx.xxx.apk/!/assets

        //为了加密setup.xml,做了一下逻辑
        //编辑器器下，加载Art下的setup.xml
        //真机下，setup.xml会加密并拷贝到StreamingAssets下（game_setup.xml）,真机读取StreamingAssets下的game_setup.xml
#if UNITY_EDITOR
        string file_path = System.IO.Path.Combine(Application.dataPath + "/Res", "setup.xml");
        byte[] setup_bytes = null;
        try
        {
            setup_bytes = System.IO.File.ReadAllBytes(file_path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("ReadAllText(setup.xml)出错：" + ex.Message);
            return null;
        }
        return setup_bytes;
#else
        string file_path = System.IO.Path.Combine(Application.streamingAssetsPath, "game_setup.xml");
        byte[] setup_bytes = null;

        if (file_path.Contains("://"))
        {
            WWW www = new WWW(file_path);
            long stime = TimeUtil.CurrentTimeMillis;
            while (!www.isDone)
            {
                long etime = TimeUtil.CurrentTimeMillis;
                if ((etime - stime) >= 5000.0f)
                {
#if UNITY_EDITOR
                    Debug.LogError("读取StreamingAssets/setup.xml超时");
#endif
                    return null;
                }
                System.Threading.Thread.Sleep(1);
            }
            if (string.IsNullOrEmpty(www.error) != true)
            {
#if UNITY_EDITOR
                Debug.LogError("读取StreamingAssets/setup.xml出错：" + www.error);
#endif
                return null;
            }
            setup_bytes = www.bytes;
#if UNITY_EDITOR
            Debug.Log("setup.xml:" + setup_bytes);
#endif
        }
        else
        {
            try
            {
                setup_bytes = System.IO.File.ReadAllBytes(file_path);
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError("ReadAllText(setup.xml)出错：" + ex.Message);
#endif
                return null;
            }
        }
        if (setup_bytes != null)
        {
            setup_bytes = GStore.EncryptTool.Decrypt(setup_bytes);
        }
        return setup_bytes;
#endif
    }
    /// <summary>
    /// 加载Setup.xml数据
    /// </summary>
    /// <returns></returns>
    public bool LoadSetup()
    {
        byte[] setup_bytes = GetSetupBytes();

        if (setup_bytes == null)
        {
            Debug.LogError("请检查setup.xml是否存在");
            return false;
        }
        XMLParser xml = new XMLParser();
        xml.Parse(XMLTool.ToString(setup_bytes));
        SecurityElement node = xml.ToXml().SearchForChildByTag("Publish");
        game_version = int.Parse(XMLTool.Attribute(node, "game_version"));
        isDevelop = bool.Parse(XMLTool.Attribute(node, "developMode"));
        use_ab = bool.Parse(XMLTool.Attribute(node, "use_ab"));
        isHotUpdate = bool.Parse(XMLTool.Attribute(node, "updateMode"));
        isEncrypt = bool.Parse(XMLTool.Attribute(node, "encry"));
        isSmallPackage = bool.Parse(XMLTool.Attribute(node, "smallPackage"));
        network_type = XMLTool.GetIntAttribute(node, "network_type");

        bool.TryParse(XMLTool.Attribute(node, "downloadingWhilePlaying"), out mDownloadingWhilePlaying);
        bool.TryParse(XMLTool.Attribute(node, "dwpBuildPackage"), out mDwpBuildPackage);
        bool.TryParse(XMLTool.Attribute(node, "dwpCopyResToSvn"), out mDwpCopyResToSvn);
        int.TryParse(XMLTool.Attribute(node, "dwpVersion"), out mDwpVersion);
        bool.TryParse(XMLTool.Attribute(node, "dwpSupplementUpdate"), out mDwpSupplementUpdate);

        //网络配置
        node = GetNetworkNode(xml.ToXml(), network_type);
        LoadServerInfoList(node);

        LoginServerIp = ServerList[0].ip;
        LoginServerPort = ServerList[0].port;
        //资源标识
        res_version = ServerList[0].res_version;
        hot_update_config_id = ServerList[0].hot_update_config_id;

        GameServerIp = LoginServerIp;

        return true;
    }

    private SecurityElement GetNetworkNode(SecurityElement _node, int _network_type)
    {
        XMLNodeList _list = _node.GetNodeList("Network");
        foreach (SecurityElement _result in _list)
        {
            if (XMLTool.GetIntAttribute(_result, "type") == _network_type)
            {
                return _result;
            }
        }
        return null;
    }

    private void LoadServerInfoList(SecurityElement _node)
    {
        XMLNodeList _list = _node.GetNodeList("Server");
        ServerList = new List<ServerInfo>();
        foreach (SecurityElement _server_node in _list)
        {
            ServerInfo _info = new ServerInfo();
            ServerList.Add(_info);

            _info.name = XMLTool.GetStringAttribute(_server_node, "name");
            _info.ip = XMLTool.GetStringAttribute(_server_node, "ip");
            _info.port = XMLTool.GetIntAttribute(_server_node, "port");
            _info.res_version = XMLTool.GetLongAttribute(_server_node, "res_version");
            _info.hot_update_config_id = XMLTool.GetIntAttribute(_server_node, "hot_update_config_id");
        }
    }
}

public class ServerInfo
{
    public string ip;
    public int port;
    public string name;
    public long res_version;//资源版本
    public int hot_update_config_id;//全包删除资源标识  0为默认值不删除， 新安装的版本如果大于上个版本的这个值，则代表要删除资源（只限于全包的情况下）

    public override string ToString()
    {
        return name + " " + ip;
    }
}
