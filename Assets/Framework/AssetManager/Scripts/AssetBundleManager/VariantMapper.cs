using Framework.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.AssetManager
{
    public class VariantMapper
    {
        /// <summary>
        /// 已打包的变体
        /// </summary>
        private Dictionary<string, int> _packedVariantStateDict = null;
        /// <summary>
        /// 已打包的变体名列表
        /// </summary>
        private List<string> _packedVariantList = null;
        /// <summary>
        /// 已激活的变体名
        /// </summary>
        private string[] _activedVariants = null;

        public VariantMapper(AssetBundleManifest manifest)
        {
            string[] variantNames = manifest.GetAllAssetBundlesWithVariant();
            _packedVariantList = new List<string>();
            _packedVariantStateDict = new Dictionary<string, int>();

            for(int i=0;i<variantNames.Length;i++)
            {
                using (gstring.Block())
                {
                    gstring nameWithVariant = variantNames[i];
                    int index = nameWithVariant.LastIndexOf('.');
                    gstring bundleName = nameWithVariant.Substring(0, index);
                    gstring variant = nameWithVariant.Substring(index);
                    int variantState = 0;
                    _packedVariantStateDict.TryGetValue(bundleName, out variantState);
                    variantState |= 1 << GetVariantIndex(variant);
                    _packedVariantStateDict[bundleName] = variantState;
                }
            }
        }
        /// <summary>
        /// 激活变体
        /// </summary>
        /// <param name="variants"></param>
        public void ActivateVariants(string[] variants)
        {
            if (variants == null || variants.Length <= 0)
                return;
            if (variants.Length == 1 && string.IsNullOrEmpty(variants[0]))
                return;
            _activedVariants = new string[variants.Length];
            for(int i=0;i<_activedVariants.Length;i++)
            {
                using (gstring.Block())
                {
                    _activedVariants[i] = gstring.Format(".", variants[i]);
                }    
            }
        }
        /// <summary>
        /// 重新映射变体名 - 变体激活顺序匹配，匹配失败则返回变体
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public string RemapVariantName(string bundleName)
        {
            bundleName = GetBundleNameWithoutVariant(bundleName);
            int variantState = 0;
            _packedVariantStateDict.TryGetValue(bundleName, out variantState);
            if (variantState == 0)
                return bundleName;
            if (_activedVariants != null && _activedVariants.Length > 0)
            {
                for (int i = 0; i < _activedVariants.Length; i++)
                {
                    //激活变体与打包变体匹配
                    if (CheckVariantPacked(variantState, _activedVariants[i]))
                    {
                        using (gstring.Block())
                        {
                            gstring remapVariantName = gstring.Format(bundleName, _activedVariants[i]);
                            return remapVariantName;
                        }

                    }
                }
            }
                //模糊匹配
                string fuzzyVariant = GetFuzzyVariant(variantState);
                if (string.IsNullOrEmpty(fuzzyVariant) == false)
                {
                    string remapVariantName = bundleName + fuzzyVariant;
                    Debug.LogWarningFormat("已激活的变体匹配失败，模糊匹配成功，请正确设置激活列表以消除此警告。remapVariantName={0}, activedVariant=[{1}]", remapVariantName, _activedVariants == null ? string.Empty : string.Join(",", _activedVariants));
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
            int index = _packedVariantList.IndexOf(variant);
            if(index < 0)
            {
                _packedVariantList.Add(variant);
                return _packedVariantList.Count - 1;
            }
            return index;
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
            _packedVariantStateDict.TryGetValue(bundleName, out variantState);

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
            _packedVariantStateDict.TryGetValue(bundleName, out variantState);

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

            for (int i = 0; i < _packedVariantStateDict.Count; i++)
            {
                if ((variantState & 1 << i) != 0)
                {
                    //模糊匹配，只要存在就返回
                    return _packedVariantList[i];
                }
            }
            return string.Empty;
        }

        public static string GetBundleNameWithoutVariant(string bundleName)
        {
            using (gstring.Block())
            {
                gstring name = bundleName;
                int index = name.IndexOf(AssetPathDefine.assetBundleExtension);
                if(index < 0)
                {
                    return name;
                }
                int variantIndex = index + AssetPathDefine.assetBundleExtension.Length;
                if(variantIndex < name.Length)
                {
                    name = name.Substring(0, variantIndex);//去除变体名
                }
                return name;
            }
                
        }

        public bool HasVariant(string bundleName)
        {
            return _packedVariantStateDict.ContainsKey(GetBundleNameWithoutVariant(bundleName));
        }
    }
}

