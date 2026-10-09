using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 统一资源入口（基于 Addressables）。
/// - 全部异步加载；按地址维护引用计数，Release 归零才真正释放句柄；
/// - 业务代码只认 Address（如 AAConst 常量）；
/// - 负责启动期的 catalog 检查/更新与远端依赖下载；
/// - 编辑器下走 Addressables 的 Use Asset Database 模式，迭代无需打包。
/// </summary>
public class AssetService : Singleton<AssetService>
{
    /// <summary>远端内容统一标签，必须与构建端 AddressableUtil.RemoteLabel 保持一致。</summary>
    public const string RemoteLabel = "Remote";

    private class Entry
    {
        public AsyncOperationHandle Handle;
        public int RefCount;
    }

    private readonly Dictionary<string, Entry> _assets = new Dictionary<string, Entry>();
    private readonly List<AsyncOperationHandle> _instances = new List<AsyncOperationHandle>();

    public bool IsReady { get; private set; }

    /// <summary>初始化 Addressables。返回是否成功。</summary>
    public async Task<bool> InitializeAsync()
    {
        if (IsReady) return true;
        var init = Addressables.InitializeAsync();
        await init.Task;
        IsReady = init.Status == AsyncOperationStatus.Succeeded;
        if (!IsReady) Debug.LogError("[AssetService] Addressables 初始化失败");
        return IsReady;
    }

    /// <summary>检查并应用远端 catalog 更新。返回是否有更新被应用。</summary>
    public async Task<bool> UpdateCatalogsAsync()
    {
        var check = Addressables.CheckForCatalogUpdates(false);
        await check.Task;
        bool succeeded = check.Status == AsyncOperationStatus.Succeeded;
        var catalogs = succeeded ? check.Result : null;
        if (check.IsValid()) Addressables.Release(check);

        if (!succeeded)
        {
            Debug.LogWarning("[AssetService] 检查 catalog 更新失败，使用本地 catalog");
            return false;
        }
        if (catalogs == null || catalogs.Count == 0) return false;

        var update = Addressables.UpdateCatalogs(catalogs, false);
        await update.Task;
        bool ok = update.Status == AsyncOperationStatus.Succeeded;
        if (!ok) Debug.LogWarning("[AssetService] 应用 catalog 更新失败");
        if (update.IsValid()) Addressables.Release(update);
        if (ok) Debug.Log("[AssetService] catalog 已更新");
        return ok;
    }

    /// <summary>计算远端内容仍需下载的字节数。</summary>
    public async Task<long> GetRemoteDownloadSizeAsync()
    {
        var handle = Addressables.GetDownloadSizeAsync(RemoteLabel);
        await handle.Task;
        long size = 0;
        if (handle.Status == AsyncOperationStatus.Succeeded) size = handle.Result;
        else Debug.LogWarning("[AssetService] 计算远端下载大小失败");
        if (handle.IsValid()) Addressables.Release(handle);
        return size;
    }

    /// <summary>下载远端内容依赖，onProgress 为 0..1。返回是否成功。</summary>
    public async Task<bool> DownloadRemoteDependenciesAsync(Action<float> onProgress = null)
    {
        var handle = Addressables.DownloadDependenciesAsync(RemoteLabel, false);
        try
        {
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.GetDownloadStatus().Percent);
                await Task.Yield();
            }
            onProgress?.Invoke(1f);
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[AssetService] 下载远端资源失败");
                return false;
            }
            return true;
        }
        finally
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
    }

    /// <summary>加载资源并递增其引用计数。</summary>
    public async Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        if (_assets.TryGetValue(key, out var entry))
        {
            if (entry.Handle.IsValid())
            {
                entry.RefCount++;
                await entry.Handle.Task;
                return entry.Handle.Result as T;
            }
            _assets.Remove(key);
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetService] 加载失败: {key}");
            Addressables.Release(handle);
            return null;
        }
        _assets[key] = new Entry { Handle = handle, RefCount = 1 };
        return handle.Result;
    }

    public async Task<GameObject> InstantiateAsync(string key, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(key, parent);
        await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AssetService] 实例化失败: {key}");
            Addressables.Release(handle);
            return null;
        }
        _instances.Add(handle);
        return handle.Result;
    }

    /// <summary>释放一个实例（配合 InstantiateAsync 使用）。</summary>
    public void ReleaseInstance(GameObject go)
    {
        if (go == null) return;
        for (int i = _instances.Count - 1; i >= 0; i--)
        {
            var h = _instances[i];
            if (h.IsValid() && ReferenceEquals(h.Result, go))
            {
                _instances.RemoveAt(i);
                Addressables.ReleaseInstance(go);
                return;
            }
        }
        Addressables.ReleaseInstance(go);
    }

    /// <summary>递减引用计数；归零时释放句柄。</summary>
    public void Release(string key)
    {
        if (!_assets.TryGetValue(key, out var entry)) return;
        entry.RefCount--;
        if (entry.RefCount > 0) return;
        _assets.Remove(key);
        if (entry.Handle.IsValid()) Addressables.Release(entry.Handle);
    }

    public void ReleaseAll()
    {
        foreach (var entry in _assets.Values)
            if (entry.Handle.IsValid()) Addressables.Release(entry.Handle);
        _assets.Clear();

        foreach (var h in _instances)
            if (h.IsValid()) Addressables.ReleaseInstance(h);
        _instances.Clear();
    }
}
