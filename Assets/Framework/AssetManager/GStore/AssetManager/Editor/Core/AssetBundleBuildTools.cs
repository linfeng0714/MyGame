using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using GStore;
using System;
using Object = UnityEngine.Object;

/// <summary>
/// AssetBundle打包工具类，实现整个打包策略的各种方法
/// </summary>
public class AssetBundleBuildTools
{
    /// <summary>
    /// 资源信息列表
    /// </summary>
    private static List<AssetInfo> m_AssetInfoList = new List<AssetInfo>();

    /// <summary>
    /// 资源集
    /// </summary>
    private static Dictionary<string, int> m_AssetNameDict = new Dictionary<string, int>();

    /// <summary>
    /// 资源引用计数器
    /// </summary>
    private static Dictionary<string, HashSet<string>> m_AssetReferenceCounter = new Dictionary<string, HashSet<string>>();

    /// <summary>
    /// 已记录引用的资源
    /// </summary>
    private static HashSet<string> m_AssetReferenceRecordedSet = new HashSet<string>();

    /// <summary>
    /// 资源bundle字典 - assetPath -> abName
    /// </summary>
    private static Dictionary<string, string> m_AssetPathDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// ABName集 - 记录本次打包设置的ABName
    /// </summary>
    private static HashSet<string> m_ABNameSet = new HashSet<string>();

    /// <summary>
    /// 丢失的资源
    /// </summary>
    private static Dictionary<string, string> m_MissingAssetList = new Dictionary<string, string>();

    /// <summary>
    /// 合并文件列表
    /// </summary>
    private static List<string> m_CombineFileList = new List<string>();

    /// <summary>
    /// 变体字典
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>> m_VariantDict = new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// 变体名集合
    /// </summary>
    private static HashSet<string> m_VariantNames = new HashSet<string>();

    /// <summary>
    /// 共享包索引
    /// </summary>
    private static int m_SharedAssetsIndex = 0;

    /// <summary>
    /// 文件类型打包后大小计算权重 - 估值
    /// </summary>
    private static Dictionary<string, float> m_AssetBuildSizeWeightDict = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
    {
        { ".anim", 0.08f},
        { ".fbx", 0.5f},
        { ".tga", 0.1f},
        { ".png", 0.5f},
    };

    /// <summary>
    /// bundle表构造器
    /// </summary>
    private static BundleTableCreateMgr m_BundleTableCreator = new BundleTableCreateMgr();

    /// <summary>
    /// 文件提交器
    /// </summary>
    private static FileCommitor m_FileCommitor = new FileCommitor();

    /// <summary>
    /// 设置ABName完整流程
    /// </summary>
    /// <returns></returns>
    [ProjcetBuildMethod(10, "标记资源")]
    public static bool MarkAssets()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        //检查资源文件夹
        if (Directory.Exists(AssetPathDefine.resFolder) == false)
        {
            Debug.LogErrorFormat("资源文件夹不存在，请检查:{0}", AssetPathDefine.resFolder);
            return false;
        }

        Clear();

        //读取变体表
        if (ReadVariants() == false)
        {
            EditorUtility.DisplayDialog("错误", "ReadVariants出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //读取资源表
        if (!ReadAssetTable())
        {
            EditorUtility.DisplayDialog("错误", "ReadAssetTable出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //分析依赖计数
        if (!InitReferenceCounter())
        {
            EditorUtility.DisplayDialog("错误", "InitReferenceCounter出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //更新缓存
        ReferenceCache.Save();

        //设置固定包ABName
        if (!SetFixedAssetGroup())
        {
            EditorUtility.DisplayDialog("错误", "SetFixedAssetBundleName出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //设置资源表ABName
        if (!SetAssetAddress())
        {
            EditorUtility.DisplayDialog("错误", "SetAssetBundleName出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //设置依赖ABName
        if (!SetDependenceAddress())
        {
            EditorUtility.DisplayDialog("错误", "SetDependenceABName出现了一些错误，请查看日志。", "确定");
            return false;
        }

        //合并依赖包
        if (ProjectBuilderSettings.Instance.combineDependenceAssetBundle)
        {
            CombineDependenceAB();
        }

        //移除无用的ABName
        ClearUnusedBundleName();

        //保存BundleTable
        m_BundleTableCreator.SaveFile();

        //保存引用表，用于分析
        SaveReferenceTable();

        //Bundle记录
        RecordModifyFile(ProjectBuilderSettings.Instance.bundleTableFile, false);
        RecordModifyFile(ProjectBuilderSettings.Instance.depBundleTableFile, false);

        //测试功能:目前只实现了OSX下提交meta
        if (IsCommitModify())
        {
            m_FileCommitor.Commit("Commit AssetBundleName.(by AssetBundleBuildTools)");
        }

        sw.Stop();
        Debug.LogWarning(string.Format("设置AssetBundleName总共耗时:{0}", sw.Elapsed.ToString()));
        return true;
    }

    /// <summary>
    /// 是否要提交更改
    /// </summary>
    /// <returns></returns>
    private static bool IsCommitModify()
    {
#if UNITY_EDITOR_OSX && !CLOUDBUILD_BAN_SVN_COMMIT
        return ProjectBuilderSettings.Instance.commitAssetBundleName || UnityEditorInternal.InternalEditorUtility.inBatchMode;
#else
        return false;
#endif
    }

    /// <summary>
    /// 移除已经无用的ABName
    /// </summary>
    /// <returns></returns>
    private static bool ClearUnusedBundleName()
    {
        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();

        string bundleTableName = Path.GetFileNameWithoutExtension(m_BundleTableCreator.BundleTableScriptablePath).ToLower();
        m_ABNameSet.Add(bundleTableName);
        m_AssetPathDict.Add(m_BundleTableCreator.BundleTableScriptablePath, bundleTableName);

        int i = 0;
        foreach (string abName in bundleNames)
        {
            i++;
            EditorTools.DisplayProgressBar(string.Format("ClearUnusedBundleName...({0}/{1})", i, bundleNames.Length), abName, (float)i / bundleNames.Length);
            if (m_ABNameSet.Contains(abName) == false)
            {
                if (IsCommitModify())
                {
                    string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
                    foreach (string assetPath in assetPaths)
                    {
                        RecordModifyFile(assetPath);
                    }
                }

                AssetDatabase.RemoveAssetBundleName(abName, true);
                UnityEngine.Debug.LogWarning("RemoveUnusedABName: " + abName);
            }
            else
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
                foreach (string assetPath in assetPaths)
                {
                    //不是本次打包设置的
                    if (m_AssetPathDict.ContainsKey(assetPath) == false)
                    {
                        if (IsCommitModify())
                        {
                            RecordModifyFile(assetPath);
                        }

                        var importer = AssetImporter.GetAtPath(assetPath);
                        importer.assetBundleName = string.Empty;

                        UnityEngine.Debug.LogWarningFormat("ClearBundleName: assetPath={0}, bundleName={1}", assetPath, abName);
                    }
                }
            }
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
        EditorTools.ClearProgressBar();
        return true;
    }

    /// <summary>
    /// 读入AssetInfo
    /// </summary>
    private static bool ReadAssetTable()
    {
        try
        {
            //删除持久化目录的数据，确保读表的信息最新
            if (Directory.Exists(AssetPathDefine.externalFilePath))
            {
                Directory.Delete(AssetPathDefine.externalFilePath, true);
            }

            var enumerator = AssetTable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetInfo data = enumerator.Current.Value;
                if (File.Exists(data.assetPath) == false)
                {
                    if (Directory.Exists(data.assetPath))
                    {
                        //跳过文件夹
                        continue;
                    }

                    m_MissingAssetList.Add(data.id.ToString(), data.assetPath);
                    continue;
                }
                m_AssetInfoList.Add(data);

                string key = data.assetPath;
                int id = 0;
                if (m_AssetNameDict.TryGetValue(key, out id))
                {
                    UnityEngine.Debug.LogWarningFormat("{0} already exists, last_id={1}, now_id={2}", key, id, data.id);
                    continue;
                }

                m_AssetNameDict.Add(data.assetPath, data.id);
            }
            enumerator.Dispose();

            if (m_MissingAssetList.Count > 0)
            {
                string json = JsonUtil.ToPrettyJson(m_MissingAssetList, NewLine.Unix);
                string recordFilePath = ProjectBuilderSettings.Instance.missingAssetRecordFile;
                File.WriteAllText(recordFilePath, json, new System.Text.UTF8Encoding(false));
                RecordModifyFile(recordFilePath, false);
                UnityEngine.Debug.LogWarningFormat("asset表有{0}个资源丢失！已生成记录文件：{1}", m_MissingAssetList.Count, recordFilePath);
            }

            return true;
        }
        catch (System.Exception e)
        {
            EditorTools.ClearProgressBar();
            UnityEngine.Debug.LogError("Step:ReadAssetTable error! " + e.ToString());
            UnityEngine.Debug.LogException(e);
            return false;
        }
    }

    /// <summary>
    /// 分析固定包的引用记数
    /// </summary>
    /// <returns></returns>
    private static bool InitFixedReferenceCounter()
    {
        foreach (var kvp in FixedGroupSettings.Instance.GetAllGroups())
        {
            string dir = kvp.Key;
            Object[] objs = EditorTools.LoadAllFromDir<Object>(dir);
            if (objs == null)
            {
                continue;
            }

            foreach (Object obj in objs)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                RecordAssetReference(assetPath);
            }
        }
        return true;
    }

    /// <summary>
    /// 读出所有资源的第一依赖作为待选资源，初始化引用计数器，记录引用数。
    /// </summary>
    private static bool InitReferenceCounter()
    {
        try
        {
            for (int i = 0; i < m_AssetInfoList.Count; i++)
            {
                AssetInfo info = m_AssetInfoList[i];
                string assetPath = info.assetPath;

                EditorTools.DisplayProgressBar(string.Format("InitReferenceCounter ({0}/{1})", i + 1, m_AssetInfoList.Count), assetPath, (float)(i + 1) / m_AssetInfoList.Count);

                if (File.Exists(assetPath) == false)
                {
                    continue;
                }
                RecordAssetReference(assetPath);
            }

            //分析固定包的引用
            InitFixedReferenceCounter();

            EditorTools.ClearProgressBar();
            return true;
        }
        catch (System.Exception e)
        {
            EditorTools.ClearProgressBar();
            UnityEngine.Debug.LogError("Step:InitReferenceCounter error!");
            UnityEngine.Debug.LogError(e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 设置固定的包名
    /// </summary>
    private static bool SetFixedAssetGroup()
    {
        try
        {
            Dictionary<string, FixedGroup> groups = FixedGroupSettings.Instance.GetAllGroups();

            int length = groups.Count;
            int i = 0;
            foreach (var kvp in groups)
            {
                i++;
                FixedGroup fixedGroup = kvp.Value;
                EditorTools.DisplayProgressBar(string.Format("设置固定包:({0}/{1})", i.ToString(), length.ToString()), fixedGroup.folder, (float)(i) / length);

                string abName = fixedGroup.folder;
                string folderRoot = AssetPathDefine.resFolder;
                //移除路径前缀
                if (abName.StartsWith(folderRoot))
                {
                    abName = abName.Substring(folderRoot.Length);
                }
                //移除特殊符号
                abName = abName.ToLower().Replace('\\', '/').Replace("/", "_") + AssetPathDefine.assetBundleExtension;

                List<string> paths = fixedGroup.Entries;
                for (int j = 0; j < paths.Count; j++)
                {
                    string path = paths[j];
                    AssetImporter importer = AssetImporter.GetAtPath(path);

                    if (importer == null)
                    {
                        Debug.LogError("can not import : " + path);
                        return false;
                    }

                    string assetPath = importer.assetPath;

                    //处理Dummy Material, 用于控制shader编译的变体
                    string fileName = Path.GetFileName(assetPath);
                    if (fileName.StartsWith("dummy"))
                    {
                        //添加支持shadervariants
                        if (fileName.EndsWith(".mat") || fileName.EndsWith(".shadervariants"))
                        {
                            abName = ProjectBuilderSettings.Instance.shaderAssetBundleName;
                            RecordAssetBundleName(importer, abName);
                            continue;
                        }
                    }

                    //分割打包需要独立的abName
                    if (fixedGroup.separately)
                    {
                        abName = GenerateUniqueABName(assetPath);
                    }

                    RecordAssetBundleName(importer, assetPath, abName, string.Empty, false);
                }
                //支持LoadAll
                m_BundleTableCreator.RecordBundleName(fixedGroup.folder, string.Empty, abName);
            }
            EditorTools.ClearProgressBar();
            return true;
        }
        catch (System.Exception e)
        {
            EditorTools.ClearProgressBar();
            UnityEngine.Debug.LogError("Step:SetFixedAssetBundleName error!");
            UnityEngine.Debug.LogError(e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 记录引用
    /// </summary>
    /// <param name="assetPath"></param>
    private static void RecordAssetReference(string assetPath)
    {
        if (m_AssetReferenceRecordedSet.Add(assetPath) == false)
        {
            //已处理过，跳出递归，防止循环引用。
            return;
        }

        string[] deps = ReferenceCache.GetDependencies(assetPath, false);

        foreach (string d in deps)
        {
            if (Path.GetExtension(d).Equals(".cs")) //跳过c#脚本
            {
                continue;
            }
            if (File.Exists(d) == false)
            {
                continue;
            }

            HashSet<string> refSet = null;
            if (m_AssetReferenceCounter.TryGetValue(d, out refSet) == false)
            {
                refSet = new HashSet<string>();
                m_AssetReferenceCounter[d] = refSet;
            }

            refSet.Add(assetPath);

            RecordAssetReference(d);
        }
    }

    /// <summary>
    /// 记录修改文件
    /// </summary>
    /// <param name="assetPath"></param>
    private static void RecordModifyFile(string assetPath, bool isMeta = true)
    {
        string filePath = assetPath;

        if (assetPath.StartsWith("Assets/") == false)
        {
            UnityEngine.Debug.LogWarning("修改了Assets以外的目录?");
            return;
        }

        if (isMeta)
        {
            filePath = filePath + ".meta";
        }

        m_FileCommitor.Add(filePath);
    }

    /// <summary>
    /// 记录ABName - 依赖包
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="bundleName"></param>
    private static void RecordAssetBundleName(AssetImporter importer, string bundleName)
    {
        RecordAssetBundleName(importer, string.Empty, bundleName, string.Empty, true);
    }

    /// <summary>
    /// 记录ABName
    /// </summary>
    /// <param name="assetKey"></param>
    /// <param name="assetPath"></param>
    /// <param name="bundleName"></param>
    /// <param name="isDep"></param>
    private static void RecordAssetBundleName(AssetImporter importer, string assetKey, string bundleName, string variant, bool isDep)
    {
        string lastBundleName = string.Empty;
        //已经设置过ABName
        if (m_AssetPathDict.TryGetValue(importer.assetPath, out lastBundleName))
        {
            if (lastBundleName != bundleName)
            {
                throw new System.InvalidOperationException(string.Format("{0} 重复设置abName, last={1}, now={2}", importer.assetPath, lastBundleName, bundleName));
            }
            else
            {
                //可能是不同assetKey
                m_BundleTableCreator.RecordBundleName(assetKey, importer.assetPath, bundleName, isDep);
                return;
            }
        }
        else
        {
            m_AssetPathDict[importer.assetPath] = bundleName;
        }

        //检查ABName
        if (IsContainNotASCII(bundleName))
        {
            throw new System.ArgumentException(string.Format("abName格式错误!只允许ASCII字符. assetPath={0}, abName={1}", importer.assetPath, bundleName));
        }
        //检查variant
        if (IsContainNotASCII(variant))
        {
            throw new System.ArgumentException(string.Format("variant格式错误!只允许ASCII字符. assetPath={0}, abName={1}", importer.assetPath, bundleName));
        }

        if (string.IsNullOrEmpty(variant))
        {
            m_ABNameSet.Add(bundleName);
        }
        else
        {
            m_ABNameSet.Add(string.Format("{0}.{1}", bundleName, variant));
        }

        //设置ABName和变体
        if (importer.assetBundleName != bundleName || importer.assetBundleVariant != variant)
        {
            RecordModifyFile(importer.assetPath);
            importer.SetAssetBundleNameAndVariant(bundleName, variant);
        }

        //记录到索引文件
        m_BundleTableCreator.RecordBundleName(assetKey, importer.assetPath, bundleName, isDep);

        //支持直接通过场景名加载场景
        if (isDep == false && importer.assetPath.EndsWith(".unity"))
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(importer.assetPath);
            if (sceneAsset != null)
            {
                //Debug.LogFormat("记录打包场景文件：{0}", assetPath);
                m_BundleTableCreator.RecordBundleName(sceneAsset.name, importer.assetPath, bundleName, isDep);
            }
        }
    }

    /// <summary>
    /// 生成共享ABName
    /// </summary>
    /// <returns></returns>
    private static string GenerateSharedABName()
    {
        var setting = ProjectBuilderSettings.Instance;
        return string.Format("{0}{1}{2}", setting.sharedAssetBundleName, m_SharedAssetsIndex++, AssetPathDefine.assetBundleExtension);
    }

    /// <summary>
    /// 生成唯一ABName
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static string GenerateUniqueABName(string assetPath)
    {
        return AssetDatabase.AssetPathToGUID(assetPath) + AssetPathDefine.assetBundleExtension;
    }

    /// <summary>
    /// 生成Sprite ABName
    /// </summary>
    /// <returns></returns>
    private static string GenerateSpriteAtlasABName(TextureImporter importer)
    {
        string abName = string.Empty;
        if (importer == null)
        {
            return abName;
        }

        if (string.IsNullOrEmpty(importer.spritePackingTag) == false)
        {
            //Debug.LogFormat("{0}:{1}", importer.assetPath, importer.spritePackingTag);
            abName = string.Format("spriteatlas_{0}{1}", importer.spritePackingTag.ToLower(), AssetPathDefine.assetBundleExtension);
        }

        return abName;
    }

    /// <summary>
    /// 是否是SpriteAtlas
    /// </summary>
    /// <param name="importer"></param>
    /// <returns></returns>
    private static bool IsSpriteAtlas(AssetImporter importer)
    {
        TextureImporter textureImporter = importer as TextureImporter;
        return textureImporter != null && string.IsNullOrEmpty(textureImporter.spritePackingTag) == false;
    }

    /// <summary>
    /// 设置AssetBundleName，
    /// </summary>
    private static bool SetAssetAddress()
    {
        try
        {
            int count = m_AssetInfoList.Count;
            int value = 0;
            foreach (var info in m_AssetInfoList)
            {
                string assetPath = info.assetPath;
                EditorTools.DisplayProgressBar(string.Format("SetAssetBundle:({0}/{1})", ++value, count), assetPath, (float)(value) / count);
                string abName = string.Empty;
                if (m_AssetPathDict.TryGetValue(assetPath, out abName))//该资源已经设置了，这里可能是固定包设置了，也可能是有不同id指向同一个资源，也可能是依赖包那边设置了
                {
                    //使用之前设置的abName，添加一条记录
                    m_BundleTableCreator.RecordBundleName(assetPath, assetPath, abName);
                    continue;
                }

                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                {
                    Debug.LogErrorFormat("can not import: {0}, try fix...", assetPath);
                    //不知道为什么会出现没导入的情况，尝试强行导入
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    importer = AssetImporter.GetAtPath(assetPath);
                    if (importer == null)
                    {
                        Debug.LogErrorFormat("can not import: {0}", assetPath);
                        return false;
                    }
                }

                if (assetPath != importer.assetPath)
                {
                    Debug.LogWarningFormat("资源表命名大小写不一致！表格={0}, 实际={1}, id={2}", assetPath, importer.assetPath, info.id);
                }

                if (Path.GetExtension(assetPath).Equals(".shader"))
                {
                    abName = ProjectBuilderSettings.Instance.shaderAssetBundleName;
                }
                else
                {
                    if (IsSpriteAtlas(importer))
                    {
                        abName = GenerateSpriteAtlasABName(importer as TextureImporter);
                    }
                    else
                    {
                        abName = string.Concat(info.id, AssetPathDefine.assetBundleExtension);
                    }
                }
                //RecordAssetBundleName(importer, info.id.ToString(), abName, string.Empty);
                SetBundleNameWithVariant(importer, importer.assetPath, abName);
            }

            return true;
        }
        catch (System.Exception e)
        {
            EditorTools.ClearProgressBar();
            Debug.LogError("Step:SetAssetBundleName error!");
            Debug.LogError(e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 设置依赖AB名
    /// </summary>
    private static bool SetDependenceAddress()
    {
        int count = m_AssetReferenceCounter.Count;
        int value = 0;

        Dictionary<string, HashSet<string>>.Enumerator enumerator = m_AssetReferenceCounter.GetEnumerator();

        while (enumerator.MoveNext() != false)
        {
            KeyValuePair<string, HashSet<string>> pair = enumerator.Current;

            EditorTools.DisplayProgressBar(string.Format("SetDepAssetBundle:({0}/{1})", ++value, count), pair.Key, (float)(value) / count);

            string assetPath = pair.Key;

            if (m_AssetPathDict.ContainsKey(assetPath))
            {
                //该资源已设置过abName，所以跳过
                continue;
            }

            string abName = string.Empty;
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                Debug.LogError("can not import : " + assetPath);
                //这一步可能是Reimport触发一些资源脚本删除掉一些资源
                continue;
                //return false;
            }

            if (Path.GetExtension(assetPath).Equals(".shader")) //shader集中打包
            {
                abName = ProjectBuilderSettings.Instance.shaderAssetBundleName;
            }
            else
            {
                if (IsSpriteAtlas(importer))
                {
                    abName = GenerateSpriteAtlasABName(importer as TextureImporter);
                    //RecordAssetBundleName(importer, abName);
                    SetBundleNameWithVariant(importer, abName);
                    continue;
                }

                if (pair.Value.Count > 1)  //5.被引用大于1的资源
                {
                    if (ProjectBuilderSettings.Instance.combineDependenceAssetBundle && CheckHasVariants(assetPath) == false)
                    {
                        string[] deps = ReferenceCache.GetDependencies(assetPath, false);
                        //只合并没有下一层依赖的资源
                        if (deps.Length == 0)
                        {
                            //记录到下一步合并
                            m_CombineFileList.Add(importer.assetPath);
                            continue;
                        }
                    }

                    abName = GenerateUniqueABName(assetPath);
                }
                else
                {
                    if (CheckHasVariants(assetPath) == false)
                    {
                        //6.被引用小于等于1的资源不用打包，这一步可以去除由于依赖关系改变而不需要打ab的资源
                        if (string.IsNullOrEmpty(importer.assetBundleName) == false)
                        {
                            RecordModifyFile(assetPath);
                        }
                        importer.assetBundleName = "";
                        continue;
                    }
                }
            }

            //记录依赖资源
            //RecordAssetBundleName(importer, abName);
            SetBundleNameWithVariant(importer, abName);
        }
        return true;
    }

    /// <summary>
    /// 合并依赖AB
    /// </summary>
    /// <returns></returns>
    private static bool CombineDependenceAB()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        int count = m_CombineFileList.Count;
        int value = 0;

        long sizeLimit = ProjectBuilderSettings.Instance.finenessLimit * 1024;
        long combineSize = 0;
        string abName = string.Empty;
        int combineABCount = 0;
        foreach (string assetPath in m_CombineFileList)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                continue;
            }

            EditorTools.DisplayProgressBar(string.Format("CombineDependenceAssetBundle:({0}/{1})", ++value, count), importer.assetPath, (float)(value) / count);

            //读取文件大小
            long size = GetAssetSize(importer);
            if (size >= sizeLimit)
            {
                //单文件超出限制，单独打包
                RecordAssetBundleName(importer, GenerateUniqueABName(importer.assetPath));
                combineABCount++;
                continue;
            }

            //生成新包名
            if (string.IsNullOrEmpty(abName))
            {
                abName = GenerateSharedABName();
                combineABCount++;
            }

            RecordAssetBundleName(importer, abName);

            //达到大小限制，合下一个包
            combineSize += size;
            if (combineSize >= sizeLimit)
            {
                combineSize = 0;
                abName = string.Empty;
            }
        }

        EditorTools.ClearProgressBar();

        sw.Stop();
        UnityEngine.Debug.LogWarningFormat("合并依赖包：{0}->{1}, Cost Time:{2}", count, combineABCount, sw.Elapsed.ToString());

        return true;
    }

    /// <summary>
    /// 输出AB
    /// </summary>
    [ProjcetBuildMethod(20, "打包资源")]
    public static bool BuildAssets()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        BuildAssetBundleOptions op = BuildAssetBundleOptions.ChunkBasedCompression;

        string outputFolder = AssetPathDefine.editorBundlePath;
        if (Directory.Exists(outputFolder) == false)
        {
            Directory.CreateDirectory(outputFolder);
        }

        //调用Unity API
        BuildPipeline.BuildAssetBundles(outputFolder, op, EditorUserBuildSettings.activeBuildTarget);

        //命令行模式减少日志堆栈
        if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        sw.Stop();
        UnityEngine.Debug.LogWarningFormat("打包AB耗时：{0}", sw.Elapsed.ToString());

        //打包后清理无用的资源
        ClearOutpath();

        return true;
    }

    /// <summary>
    /// 删除输出目录中的shader
    /// </summary>
    /// <param name="outputFolder"></param>
    public static void ClearShaderAB()
    {
        string outputFolder = AssetPathDefine.editorBundlePath;

        //打包前删除shader AB，强制重打shader，避免因为Unity没有处理好cginc文件的依赖关系而造成shader没有更新的问题
        string shaderABPath = outputFolder + ProjectBuilderSettings.Instance.shaderAssetBundleName;
        string shaderABManifestPath = shaderABPath + ".manifest";
        if (File.Exists(shaderABPath))
        {
            File.Delete(shaderABPath);
        }
        if (File.Exists(shaderABManifestPath))
        {
            File.Delete(shaderABManifestPath);
        }
    }

    /// <summary>
    /// 导入小包
    /// </summary>
    /// <returns></returns>
    public static bool ImportSmallPackAB()
    {
        string targetFolder = Application.streamingAssetsPath + "/" + AssetPathDefine.assetBundleFolder + "/";
        //清空Assetbundle文件夹
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        Directory.CreateDirectory(targetFolder);

        string outPath = AssetPathDefine.editorBundlePath;

        //BundleTable
        string bundleTableName = Path.GetFileNameWithoutExtension(AssetPathDefine.bundleTableFileName).ToLower();
        File.Copy(outPath + bundleTableName, targetFolder + bundleTableName);

        //manifest
        File.Copy(outPath + AssetPathDefine.assetBundleFolder, targetFolder + AssetPathDefine.assetBundleFolder);

        //小包配置文件
        var packedBundleList = AlwaysPackedSettings.Instance.bundles;
        if (packedBundleList == null || packedBundleList.Count == 0)
        {
            return true;
        }

        //读取AssetBundleManifest
        string manifestPath = outPath + AssetPathDefine.assetBundleFolder;
        AssetBundle ab = AssetBundle.LoadFromFile(manifestPath);
        if (ab == null)
        {
            Debug.LogErrorFormat("读取AssetBundleManifest失败! path={0}", manifestPath);
            return false;
        }
        AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        var variantMapper = new VariantMapper(manifest);

        //收集依赖的AB
        HashSet<string> packedBundleSet = new HashSet<string>();
        foreach (string bundleName in packedBundleList)
        {
            CollectBundle(manifest, variantMapper, bundleName, packedBundleSet);
        }

        ab.Unload(true);

        //复制Assetbundle
        foreach (string bundleName in packedBundleSet)
        {
            string sourcePath = outPath + bundleName;
            string destPath = targetFolder + bundleName;
            File.Copy(sourcePath, destPath, true);
        }

        return true;
    }

    /// <summary>
    /// 收集依赖Bundle，包含变体
    /// </summary>
    /// <param name="manifest"></param>
    /// <param name="variantMapper"></param>
    /// <param name="bundleName"></param>
    /// <param name="collection"></param>
    private static void CollectBundle(AssetBundleManifest manifest, VariantMapper variantMapper, string bundleName, HashSet<string> collection)
    {
        if (variantMapper.HasVariant(bundleName))
        {
            var list = variantMapper.GetAllPackedAssetBundleNamesWithVariant(bundleName);
            foreach (var variantBundle in list)
            {
                foreach (string depBundleName in manifest.GetAllDependencies(variantBundle))
                {
                    CollectBundle(manifest, variantMapper, depBundleName, collection);
                }
                collection.Add(variantBundle);
            }
        }
        else
        {
            foreach (string depBundleName in manifest.GetAllDependencies(bundleName))
            {
                CollectBundle(manifest, variantMapper, depBundleName, collection);
            }
            collection.Add(bundleName);
        }
    }

    public static bool ImportAllAB()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        string outPath = AssetPathDefine.editorBundlePath + "/";
        string targetFolder = Application.streamingAssetsPath + "/" + AssetPathDefine.assetBundleFolder + "/";

        List<string> fileList = new List<string>();
        EditorTools.GetFiles(outPath, new string[] { ".manifest", ".xml", ".json" }, fileList);

        GStore.CommandLine.Rsync(outPath, targetFolder, "-av --delete --exclude *.manifest --exclude *.meta --exclude .DS_Store");

        sw.Stop();
        Debug.LogWarningFormat("copy {0} assetbundles, 耗时:{1}", fileList.Count, sw.Elapsed.ToString());
        return true;
    }

    /// <summary>
    /// 导入AB到StreamingAsset文件夹
    /// </summary>
    [ProjcetBuildMethod(30, "导入资源")]
    public static bool ImportAssets()
    {
        if (GameSetting.hotUpdateMode && GameSetting.smallPackage)
        {
            return ImportSmallPackAB();
        }
        else
        {
            return ImportAllAB();
        }
    }

    /// <summary>
    /// 删除输出路径
    /// </summary>
    public static void DeleteOutPath()
    {
        string outpath = AssetPathDefine.editorBundlePath;
        if (Directory.Exists(outpath))
        {
            Directory.Delete(outpath, true);
        }
    }

    /// <summary>
    /// 清理无用资源
    /// </summary>
    static void ClearOutpath()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        int count = 0;
        int value = 0;

        //清理无用包
        string[] allAssetBundleName = AssetDatabase.GetAllAssetBundleNames();

        string outpath = AssetPathDefine.editorBundlePath;
        List<string> allBundleFile = new List<string>();
        EditorTools.GetFiles(outpath, new string[] { "", ".manifest", ".meta" }, allBundleFile);
        count = allBundleFile.Count;
        value = 0;

        for (int i = 0; i < allBundleFile.Count; i++)
        {
            string bundlePath = allBundleFile[i];
            string bundleName = Path.GetFileName(bundlePath);
            EditorTools.DisplayProgressBar("Clear", bundleName, 0.5f * (++value) / count + 0.5f);
            if (!allAssetBundleName.Contains(bundleName))
            {
                Debug.LogWarning(string.Format("Delete: {0}", bundlePath));
                File.Delete(bundlePath);
                string str1 = string.Format("{0}.manifest", bundlePath);
                if (File.Exists(str1))
                    File.Delete(str1);
            }
        }

        //清理无用文件夹
        List<string> allDir = new List<string>();
        EditorTools.GetDirs(outpath, allDir);
        allDir.Sort();
        for (int i = 0; i < allDir.Count; i++)
        {
            string path = allDir[i];
            if (!Directory.Exists(path))
                continue;
            if (Directory.GetDirectories(path).Length != 0 || Directory.GetFiles(path).Length != 0)
                continue;
            Directory.Delete(path);
        }

        EditorTools.ClearProgressBar();
        sw.Stop();
        Debug.LogWarningFormat("清理输出文件夹, 耗时:{0}", sw.Elapsed.ToString());
    }

    /// <summary>
    /// 清理数据
    /// </summary>
    public static void Clear()
    {
        m_AssetInfoList.Clear();
        m_AssetNameDict.Clear();
        m_AssetReferenceCounter.Clear();
        m_AssetReferenceRecordedSet.Clear();
        m_AssetPathDict.Clear();
        m_ABNameSet.Clear();
        m_MissingAssetList.Clear();
        m_FileCommitor.Clear();
        m_VariantDict.Clear();
        m_VariantNames.Clear();
        m_CombineFileList.Clear();
        m_SharedAssetsIndex = 0;
        m_BundleTableCreator.Clear();
    }

    /// <summary>
    /// 完结操作
    /// </summary>
    public static void Complete()
    {
        EditorUtility.DisplayDialog("输出完毕", string.Concat("输出路径", AssetPathDefine.editorBundlePath), "OK");
    }

    /// <summary>
    /// 保存引用表
    /// </summary>
    /// <returns></returns>
    private static bool SaveReferenceTable()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        Dictionary<string, List<string>> referenceTable = new Dictionary<string, List<string>>(m_AssetReferenceCounter.Count);
        foreach (var kvp in m_AssetReferenceCounter)
        {
            referenceTable[kvp.Key] = kvp.Value.ToList();
        }

        string json = JsonUtil.ToPrettyJson(referenceTable, NewLine.Unix);
        string filePath = ProjectBuilderSettings.Instance.referenceTableFile;
        File.WriteAllText(filePath, json, new System.Text.UTF8Encoding(false));
        Debug.LogWarningFormat("一共处理了{0}个依赖文件！已生成引用表：{1}", m_AssetReferenceCounter.Count, filePath);

        RecordModifyFile(filePath, false);

        sw.Stop();
        Debug.LogWarningFormat("保存引用表, 耗时:{0}", sw.Elapsed.ToString());
        return true;
    }

    /// <summary>
    /// 获取资源大小
    /// </summary>
    /// <param name="importer"></param>
    /// <returns></returns>
    private static long GetAssetSize(AssetImporter importer)
    {
        //源文件大小
        long size = new FileInfo(importer.assetPath).Length;

        //根据后缀估算打包后的大小
        string extension = Path.GetExtension(importer.assetPath);
        float weight = 0;
        if (m_AssetBuildSizeWeightDict.TryGetValue(extension, out weight))
        {
            size = (long)(size * weight);
        };

        return size;
    }

    #region 变体相关
    /// <summary>
    /// 读取变体信息
    /// </summary>
    /// <returns></returns>
    private static bool ReadVariants()
    {
        string searchPattern = "Variant-*";
        string[] variantDirectories = Directory.GetDirectories("Assets/", searchPattern, SearchOption.AllDirectories);
        Debug.LogWarningFormat("variantDirectories={0}", variantDirectories.Length);

        int value = 0;
        int count = variantDirectories.Length;

        foreach (var directory in variantDirectories)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            string variantName = directoryInfo.Name.Substring(searchPattern.Length - 1).ToLower();

            ++value;

            List<string> files = new List<string>();
            EditorTools.GetFiles(directory, new string[] { ".meta", ".cs", ".DS_Store" }, files);

            int index = 0;
            int fileCount = files.Count;
            foreach (string file in files)
            {
                float progress = (float)(value) / count + 1f / count * index / fileCount;
                EditorTools.DisplayProgressBar(string.Format("ReadVariants:({0}/{1})", value, count), file, progress);

                string sourceFile = file.Replace("\\", "/").Replace("/" + directoryInfo.Name, "");

                AssetImporter sourceImporter = AssetImporter.GetAtPath(sourceFile);
                AssetImporter variantImporter = AssetImporter.GetAtPath(file);
                bool isMatch = sourceImporter != null;
                if (isMatch)
                {
                    RecordVariants(sourceImporter.assetPath, variantImporter.assetPath, variantName);
                }
                else
                {
                    string assetPath = file;
                    if (variantImporter != null)
                    {
                        assetPath = variantImporter.assetPath;
                    }
                    Debug.LogWarningFormat("variant not match! assetPath={0}", assetPath);
                }
            }
        }
        EditorTools.ClearProgressBar();

        if (m_VariantNames.Count > 0)
        {
            Debug.LogWarningFormat("matched variants=[{0}]", string.Join(",", m_VariantNames.ToArray()));
        }

        return true;
    }

    /// <summary>
    /// 记录变体信息
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="variantFile"></param>
    /// <param name="variantName"></param>
    private static void RecordVariants(string sourceFile, string variantFile, string variantName)
    {
        Dictionary<string, string> variants = null;
        if (m_VariantDict.TryGetValue(sourceFile, out variants) == false)
        {
            variants = new Dictionary<string, string>();
            m_VariantDict[sourceFile] = variants;
        }
        variants[variantFile] = variantName;
        m_VariantNames.Add(variantName);
    }

    /// <summary>
    /// 检查资源是否拥有变体
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static bool CheckHasVariants(string assetPath)
    {
        return m_VariantDict.ContainsKey(assetPath);
    }

    /// <summary>
    /// 设置BundleName和Variant
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private static bool SetBundleNameWithVariant(AssetImporter importer, string bundleName)
    {
        return SetBundleNameWithVariant(importer, string.Empty, bundleName, true);
    }

    /// <summary>
    /// 设置BundleName和Variant
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private static bool SetBundleNameWithVariant(AssetImporter importer, string key, string bundleName, bool isDep = false)
    {
        Dictionary<string, string> variants = null;
        if (m_VariantDict.TryGetValue(importer.assetPath, out variants) == false)
        {
            RecordAssetBundleName(importer, key, bundleName, string.Empty, isDep);
        }
        else
        {
            string defaultVariant = ProjectBuilderSettings.Instance.defaultVariantName.Substring(1);
            RecordAssetBundleName(importer, key, bundleName, defaultVariant, isDep);
            foreach (var kvp in variants)
            {
                AssetImporter variantImporter = AssetImporter.GetAtPath(kvp.Key);
                RecordAssetBundleName(variantImporter, key, bundleName, kvp.Value, true);
            }
        }

        return true;
    }

    #endregion
    private static readonly System.Text.RegularExpressions.Regex mRegNotASCII = new System.Text.RegularExpressions.Regex(@"[^\x00-\xff]");

    /// <summary>
    /// 是否含有非ASCII编码的字符
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsContainNotASCII(string content)
    {
        return mRegNotASCII.IsMatch(content);
    }
}