using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class BundleTableAsset : ScriptableObject
    {
        public List<AssetBundleTable.BundleTableInfo> listBundleTable = new List<AssetBundleTable.BundleTableInfo>();
    }
}

