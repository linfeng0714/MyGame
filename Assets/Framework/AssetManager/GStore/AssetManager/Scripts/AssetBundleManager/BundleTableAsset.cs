using System;
using System.Collections;
using System.Collections.Generic;
using GStore;
using UnityEngine;

namespace GStore
{
    [Serializable]
    public class BundleTableAsset : ScriptableObject
    {
        [SerializeField]
        public List<AssetBundleTable.BundleTableInfo> listBundleTable = new List<AssetBundleTable.BundleTableInfo>();
    }    
}

