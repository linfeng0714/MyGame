using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GStore;

/// <summary>
/// 资源管理器使用示例
/// </summary>
public class Example : MonoBehaviour
{
    /// <summary>
    /// 资源id
    /// </summary>
    private int m_AssetId = 201020;

    /// <summary>
    /// Sprite资源id
    /// </summary>
    private int m_SpriteAssetId = 30025;

    /// <summary>
    /// SpriteName
    /// </summary>
    private string m_SpriteName = "Alma_01";

    /// <summary>
    /// SpriteRenderer
    /// </summary>
    public SpriteRenderer spriteRenderer;

    /// <summary>
    /// 位置X值
    /// </summary>
    private int m_PositionX = -10;

    /// <summary>
    /// 启用AssetBundle
    /// </summary>
    private bool m_EnableAssetBundle = false;

    /// <summary>
    /// 激活的变体
    /// </summary>
    private string m_ActiveVariants = "low,normal";

    void Start()
    {
    }

    void OnDestroy()
    {
    }

    private void OnApplicationQuit()
    {
        //强制清理
        AssetBundle.UnloadAllAssetBundles(true);
    }

    void OnGUI()
    {
        GUIConfig();

        GUILayout.BeginHorizontal();
        m_EnableAssetBundle = GUILayout.Toggle(m_EnableAssetBundle, "启用AssetBundle");

        GUILayout.Label("variants");
        m_ActiveVariants = GUILayout.TextField(m_ActiveVariants);

        GUILayout.EndHorizontal();

        if (GUILayout.Button("初始化资源管理器"))
        {
            //资源管理器全局只应该初始化一次，此处为了测试开关AssetBundle而重复初始化。
#if UNITY_EDITOR
            //GameSetting.enableAssetBundle = m_EnableAssetBundle;
#endif
            //强制清理
            AssetBundle.UnloadAllAssetBundles(true);
            AssetManager.Instance.ActivateVariants(m_ActiveVariants.Split(','));
        }

        if (GUILayout.Button("UnloadUnusedAssets"))
        {
            AssetManager.Instance.UnloadUnusedAssets();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("资源ID");
        m_AssetId = int.Parse(GUILayout.TextField(m_AssetId.ToString()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("测试异步加载"))
        {
            LoadAssetAsync();
        }
        if (GUILayout.Button("测试缓存"))
        {
            CacheGameObject();
        }
        if (GUILayout.Button("测试实例化和回收"))
        {
            StartCoroutine(InstantiateAndRecycleCoroutine());
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("测试实例化、修改材质shader和回收重置"))
        {
            StartCoroutine(LoadChangeRecycle());
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("资源ID");
        m_SpriteAssetId = int.Parse(GUILayout.TextField(m_SpriteAssetId.ToString()));

        GUILayout.Label("SpriteName");
        m_SpriteName = GUILayout.TextField(m_SpriteName);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("加载Sprite"))
        {
            LoadSprite();
        }

        if (GUILayout.Button("加载多个ab"))
        {
            LoadMultipleObjs();
        }
    }

    private void GUIConfig()
    {
        int fontSize = Screen.width / 40;

        GUI.skin.label.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.textField.fontSize = fontSize;
    }

    /// <summary>
    /// 缓存资源到对象池
    /// </summary>
    private void CacheGameObject()
    {
        AssetManager.Instance.CacheObject(m_AssetId, 3);
    }

    void LoadMultipleObjs()
    {
        float time = Time.realtimeSinceStartup;
        string[] AssetNames = new[] { "201020", "201021", "201022", "201030", "201040", "202010", "202011", "202012", "202020",
            "202021","202022","202030","202040","202050","202051","202060","202070","202080","202090","202100","202110", };
        for (int i = 0; i < AssetNames.Length; i++)
        {
            GameObject obj = AssetManager.Instance.LoadAsset<UnityEngine.Object>(int.Parse(AssetNames[i])) as GameObject;
        }
        Debug.Log("Loading Multi Assets Spend Time--- " + ((Time.realtimeSinceStartup - time) * 1000).ToString("f3") + " ms" );
    }

    /// <summary>
    /// 实例化协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator InstantiateAndRecycleCoroutine()
    {
        GameObject go = AssetManager.Instance.LoadAssetAndInstantiate(m_AssetId);
        go.transform.position = new Vector3(GetPositionX(), 0, 0);

        //3秒后回收
        yield return RecycleCoroutine(go, 3);
    }

    /// <summary>
    /// 获取X坐标
    /// </summary>
    /// <returns></returns>
    private int GetPositionX()
    {
        m_PositionX++;
        if (m_PositionX > 10)
        {
            m_PositionX = -10;
        }
        return m_PositionX;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    private void LoadAssetAsync()
    {
        AssetManager.Instance.LoadAssetAsyncAndInstantiate(m_AssetId,
        (go, isOld) =>
        {
            go.transform.position = new Vector3(0, 2, 0);
            Debug.Log("异步加载完成！");

            //3秒后回收
            StartCoroutine(RecycleCoroutine(go, 3));
        },
        null);
    }

    /// <summary>
    /// 异步加载资源，进行材质修改，
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadChangeRecycle()
    {
        GameObject go = AssetManager.Instance.LoadAssetAndInstantiate(m_AssetId);
        if(go.GetComponent<MaterialRestore>() == null)
        {
            go.AddComponent<MaterialRestore>();
        }
        go.transform.position = new Vector3(GetPositionX(), 0, 0);
        go.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Actor/Animal");
        go.GetComponentInChildren<Renderer>().material.color = Color.green;

        //3秒后回收
        yield return RecycleCoroutine(go, 3);
    }

    /// <summary>
    /// 定时回收
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerator RecycleCoroutine(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        AssetManager.Instance.RecycleGameObject(go);
    }

    /// <summary>
    /// 加载Sprite
    /// </summary>
    private void LoadSprite()
    {
        Sprite sprite = AssetManager.Instance.LoadSprite(m_SpriteAssetId, m_SpriteName);
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
