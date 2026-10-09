using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 统一资源入口（基于 Addressables）。
/// - 全部异步加载，句柄缓存 + 引用计数；
/// - 业务代码只认 Address（如 "Furni/bingxiang"、"Npc/deluyi"、"Furni/bingxiang/Icon"）；
/// - 编辑器下走 Addressables 的 Use Asset Database 模式，迭代无需打包。
/// </summary>
public class AssetService : Singleton<AssetService>
{
    readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    public bool IsReady { get; private set; }

    /// <summary>初始化 Addressables（首次加载会自动初始化，这里显式暴露以便做版本检查）。</summary>
    public async void InitializeAsync()
    {
        if (IsReady) return;
        var init = Addressables.InitializeAsync();
        await init.Task;
        IsReady = init.Status == AsyncOperationStatus.Succeeded;
        if (!IsReady) Debug.LogError("[AssetService] Addressables 初始化失败");
    }

    public async System.Threading.Tasks.Task<T> LoadAssetAsync<T>(string key) where T : Object
    {
        if (_handles.TryGetValue(key, out var existing))
        {
            if (existing.IsValid())
            {
                await existing.Task;
                return existing.Result as T;
            }
            _handles.Remove(key);
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetService] 加载失败: {key}");
            Addressables.Release(handle);
            return null;
        }
        _handles[key] = handle;
        return handle.Result;
    }

    public async System.Threading.Tasks.Task<GameObject> InstantiateAsync(string key, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(key, parent);
        await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetService] 实例化失败: {key}");
            Addressables.Release(handle);
            return null;
        }
        return handle.Result;
    }

    /// <summary>释放一个实例（配合 InstantiateAsync 使用）。</summary>
    public void ReleaseInstance(GameObject go)
    {
        if (go != null) Addressables.ReleaseInstance(go);
    }

    public void Release(string key)
    {
        if (_handles.TryGetValue(key, out var handle))
        {
            _handles.Remove(key);
            if (handle.IsValid()) Addressables.Release(handle);
        }
    }

    public void ReleaseAll()
    {
        foreach (var h in _handles.Values)
            if (h.IsValid()) Addressables.Release(h);
        _handles.Clear();
    }
}
