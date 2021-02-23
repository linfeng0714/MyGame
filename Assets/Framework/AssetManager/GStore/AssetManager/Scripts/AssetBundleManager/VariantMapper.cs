using System.Collections.Generic;
using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 变体映射
    /// </summary>
    public class VariantMapper
    {
        /// <summary>
        /// 已打包的变体
        /// </summary>
        private Dictionary<string, int> m_PackedVariantStateDict = null;

        /// <summary>
        /// 已打包的变体名列表
        /// </summary>
        private List<string> m_PackedVariantList = null;

        /// <summary>
        /// 已激活的变体名
        /// </summary>
        private string[] m_ActivedVariants = null;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="manifest"></param>
        public VariantMapper(AssetBundleManifest manifest)
        {
            string[] variantNames = manifest.GetAllAssetBundlesWithVariant();
            m_PackedVariantList = new List<string>();
            m_PackedVariantStateDict = new Dictionary<string, int>();

            for (int i = 0; i < variantNames.Length; i++)
            {
                string nameWithVariant = variantNames[i];
                int index = nameWithVariant.LastIndexOf('.');
                string bundleName = nameWithVariant.Substring(0, index);
                string variant = nameWithVariant.Substring(index);

                int variantState = 0;
                m_PackedVariantStateDict.TryGetValue(bundleName, out variantState);
                variantState |= 1 << GetVariantIndex(variant);
                m_PackedVariantStateDict[bundleName] = variantState;
            }
        }

        /// <summary>
        /// 激活变体
        /// </summary>
        /// <param name="variants"></param>
        public void ActivateVariants(string[] variants)
        {
            if (variants == null || variants.Length <= 0)
            {
                return;
            }
            if (variants.Length == 1 && string.IsNullOrEmpty(variants[0]))
            {
                return;
            }

            m_ActivedVariants = new string[variants.Length];
            for (int i = 0; i < m_ActivedVariants.Length; i++)
            {
                m_ActivedVariants[i] = "." + variants[i];
            }
        }

        /// <summary>
        /// 重新映射变体名 - 变体激活顺序匹配，匹配失败则返回无变体
        /// </summary>
        /// <returns></returns>
        public string RemapVariantName(string bundleName)
        {
            bundleName = GetBundleNameWithoutVariant(bundleName);

            int variantState = 0;
            m_PackedVariantStateDict.TryGetValue(bundleName, out variantState);
            //该bundle没有变体被打包
            if (variantState == 0)
            {
                return bundleName;
            }

            //精确匹配
            if (m_ActivedVariants != null && m_ActivedVariants.Length > 0)
            {
                for (int i = 0; i < m_ActivedVariants.Length; i++)
                {
                    //激活变体与打包变体匹配
                    if (CheckVariantPacked(variantState, m_ActivedVariants[i]))
                    {
                        string remapVariantName = bundleName + m_ActivedVariants[i];
                        return remapVariantName;
                    }
                }
            }

            //模糊匹配
            string fuzzyVariant = GetFuzzyVariant(variantState);
            if (string.IsNullOrEmpty(fuzzyVariant) == false)
            {
                string remapVariantName = bundleName + fuzzyVariant;
                Debug.LogWarningFormat("已激活的变体匹配失败，模糊匹配成功，请正确设置激活列表以消除此警告。remapVariantName={0}, activedVariant=[{1}]", remapVariantName, m_ActivedVariants == null ? string.Empty : string.Join(",", m_ActivedVariants));
                return remapVariantName;
            }

            return bundleName;
        }

        /// <summary>
        /// 为变体名编号
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        private int GetVariantIndex(string variant)
        {
            int index = m_PackedVariantList.IndexOf(variant);
            if (index < 0)
            {
                m_PackedVariantList.Add(variant);
                return m_PackedVariantList.Count - 1;
            }
            else
            {
                return index;
            }
        }

        /// <summary>
        /// 检查是否打包了目标变体
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        private bool CheckVariantPacked(string bundleName, string variant)
        {
            int variantState = 0;
            m_PackedVariantStateDict.TryGetValue(bundleName, out variantState);

            return CheckVariantPacked(variantState, variant);
        }

        /// <summary>
        /// 检查是否打包了目标变体
        /// </summary>
        /// <param name="variantState"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        private bool CheckVariantPacked(int variantState, string variant)
        {
            if (variantState == 0)
            {
                return false;
            }

            int index = GetVariantIndex(variant);
            return (variantState & 1 << index) != 0;
        }

        /// <summary>
        /// 获取模糊匹配的变体名
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private string GetFuzzyVariant(string bundleName)
        {
            int variantState = 0;
            m_PackedVariantStateDict.TryGetValue(bundleName, out variantState);

            return GetFuzzyVariant(variantState);
        }

        /// <summary>
        /// 获取模糊匹配的变体名
        /// </summary>
        /// <param name="variantState"></param>
        /// <returns></returns>
        private string GetFuzzyVariant(int variantState)
        {
            if (variantState == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < m_PackedVariantList.Count; i++)
            {
                if ((variantState & 1 << i) != 0)
                {
                    //模糊匹配，只要存在就返回
                    return m_PackedVariantList[i];
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取所有已打包的变体BundleName
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public List<string> GetAllPackedAssetBundleNamesWithVariant(string bundleName)
        {
            bundleName = GetBundleNameWithoutVariant(bundleName);
            List<string> result = new List<string>();
            foreach (var variant in m_PackedVariantList)
            {
                if (CheckVariantPacked(bundleName, variant))
                {
                    result.Add(bundleName + variant);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取不包含变体名的Bundle名
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static string GetBundleNameWithoutVariant(string bundleName)
        {
            int index = bundleName.IndexOf(AssetPathDefine.assetBundleExtension);

            if (index < 0)
            {
                return bundleName;
            }

            int variantIndex = index + AssetPathDefine.assetBundleExtension.Length;
            if (variantIndex < bundleName.Length)
            {
                //去除变体名
                bundleName = bundleName.Substring(0, variantIndex);
            }
            return bundleName;
        }

        /// <summary>
        /// 判断是否有变体
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public bool HasVariant(string bundleName)
        {
            return m_PackedVariantStateDict.ContainsKey(GetBundleNameWithoutVariant(bundleName));
        }
    }
}
