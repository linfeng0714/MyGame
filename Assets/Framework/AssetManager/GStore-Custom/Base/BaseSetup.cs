
using GStore;
/// <summary>
/// 引擎安装类
/// </summary>
public static class BaseSetup
{
    /// <summary>
    /// 安装各种方法
    /// </summary>
    static public void Setup()
    {
        AssetManagerSetup.Setup();
        AppInfo.Setup(1, 1807230100, 0);
        GameSettingSetup.Setup();
    }
}

