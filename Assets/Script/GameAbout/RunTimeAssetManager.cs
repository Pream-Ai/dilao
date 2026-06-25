using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//场景1：furnires,npcres
public class RunTimeAssetManager : MonoBehaviour
{
    public static RunTimeAssetManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
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
}
