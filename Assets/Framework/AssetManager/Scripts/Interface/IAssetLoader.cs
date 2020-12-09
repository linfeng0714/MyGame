using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public interface IAssetLoader
    {
        T LoadAsset<T>(int assetId) where T : Object;

        T[] LoadAllAsset<T>(int assetId) where T : Object;

        byte[] LoadFileData(string filePath); 
    }
}

