using GStore;

public static class GameSettingSetup
{
    public static void Setup()
    {
#if UNITY_EDITOR
        GameSetting.developMode = false;
        //GameSetting.enableAssetBundle = true;
#endif
        GameSetting.hotUpdateMode = true;
        GameSetting.encrypt = false;
        GameSetting.abEncry = false;
        GameSetting.smallPackage = false;
        //GameSetting.serverIndex = 1;
    }
}
