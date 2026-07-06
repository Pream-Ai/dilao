using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
//场景1：furnires,npcres
public class ResourceManager: Singleton<ResourceManager>
{
    protected override void Awake()
    {
        base.Awake();
    }
    public T LoadFromFile<T>(string abName ,string assetName) where T : Object
    {
        string abPath = Path.Combine(Application.streamingAssetsPath,abName);
        AssetBundle localBundle = AssetBundle.LoadFromFile(abPath);
        if (localBundle == null)
        {
            Debug.LogError($"路径{abPath}中没找到该bundle包");
            return null;
        }
        T targetAsset=localBundle.LoadAsset<T>(assetName);
        localBundle.Unload(false);
        return targetAsset;
    }

    public UnityEngine.Object GetAssetCache(string name,string type_name)
    {
#if UNITY_EDITOR

#endif
        return null;
    }
}
