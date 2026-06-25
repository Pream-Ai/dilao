using UnityEngine;
using System.IO;
using UnityEditor;
public class BundleBuildTool : MonoBehaviour
{
    [MenuItem("Tools/Build Mobel AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory
            , BuildAssetBundleOptions.ChunkBasedCompression
            , BuildTarget.StandaloneWindows64
            );
        Debug.Log("物理打包成功,资源已锁进streamingAsset文件");
    }
}
