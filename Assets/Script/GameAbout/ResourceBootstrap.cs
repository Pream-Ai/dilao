using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 启动期资源准备流程：初始化 Addressables → 检查/应用 catalog 更新 → 下载远端依赖。
/// 进度通过 OnStatus(progress 0..1, statusText) 暴露，供加载界面订阅。
/// </summary>
public static class ResourceBootstrap
{
    /// <summary>(progress 0..1, statusText)</summary>
    public static event Action<float, string> OnStatus;

    public static async Task<bool> PrepareAsync()
    {
        Report(0f, "初始化资源系统");
        if (!await AssetService.Instance.InitializeAsync())
            return false;

        Report(0.1f, "检查资源更新");
        await AssetService.Instance.UpdateCatalogsAsync();

        Report(0.2f, "计算下载大小");
        long size = await AssetService.Instance.GetRemoteDownloadSizeAsync();

        if (size > 0)
        {
            string text = $"下载资源 ({FormatSize(size)})";
            Report(0.2f, text);
            bool ok = await AssetService.Instance.DownloadRemoteDependenciesAsync(
                p => Report(0.2f + 0.8f * p, text));
            if (!ok) return false;
        }

        Report(1f, "资源准备完成");
        return true;
    }

    private static void Report(float progress, string text)
    {
        OnStatus?.Invoke(progress, text);
        Debug.Log($"[ResourceBootstrap] {progress:P0} {text}");
    }

    private static string FormatSize(long bytes)
    {
        if (bytes >= 1024 * 1024) return $"{bytes / (1024f * 1024f):F1} MB";
        if (bytes >= 1024) return $"{bytes / 1024f:F1} KB";
        return $"{bytes} B";
    }
}
