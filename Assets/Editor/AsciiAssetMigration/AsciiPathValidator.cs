using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>
    /// 强制约束：Assets 下（第三方库除外）不允许出现非 ASCII 的目录/文件名。
    /// 目的：保证 Addressables bundle 名、catalog、CDN URL、macOS(NFD)/Windows(NFC) 跨平台构建可复现。
    /// </summary>
    public static class AsciiPathValidator
    {
        static readonly string[] ExcludePrefixes = AsciiAssetMigration.ExcludePrefixes;

        [MenuItem("Tools/Assets/ASCII Migration/Validate ASCII Paths")]
        public static void ValidateMenu()
        {
            var bad = Collect();
            if (bad.Count == 0)
            {
                Debug.Log("[AsciiValidator] 通过：未发现非 ASCII 路径。");
            }
            else
            {
                foreach (var b in bad) Debug.LogError($"[AsciiValidator] 非 ASCII 路径: {b}");
                Debug.LogError($"[AsciiValidator] 共 {bad.Count} 处违规。");
            }
        }

        // 供 CI 调用：有违规则退出码 1
        public static void ValidateBatch()
        {
            var bad = Collect();
            foreach (var b in bad) Debug.LogError($"[AsciiValidator] 非 ASCII 路径: {b}");
            Debug.Log($"[AsciiValidator] 违规 {bad.Count} 处。");
            EditorApplication.Exit(bad.Count == 0 ? 0 : 1);
        }

        public static List<string> Collect()
        {
            var result = new List<string>();
            var dataPath = Application.dataPath.Replace('\\', '/');

            foreach (var abs in Directory.GetDirectories(dataPath, "*", SearchOption.AllDirectories))
            {
                var dir = "Assets" + abs.Replace('\\', '/').Substring(dataPath.Length);
                if (IsExcluded(dir)) continue;
                if (AsciiAssetMigration.IsJunk(dir)) continue;
                if (!AsciiNameMap.IsAscii(dir)) result.Add(dir);
            }

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal)) continue;
                if (IsExcluded(path)) continue;
                if (AsciiAssetMigration.IsJunk(path)) continue;
                if (!AsciiNameMap.IsAscii(path)) result.Add(path);
            }

            return result.Distinct().OrderBy(x => x).ToList();
        }

        static bool IsExcluded(string assetPath)
        {
            foreach (var p in ExcludePrefixes)
                if (assetPath == p || assetPath.StartsWith(p + "/", StringComparison.Ordinal))
                    return true;
            return false;
        }
    }

    /// <summary>构建前自动校验，违规直接中断构建。</summary>
    public class AsciiPathBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var bad = AsciiPathValidator.Collect();
            if (bad.Count > 0)
            {
                foreach (var b in bad) Debug.LogError($"[AsciiValidator] 非 ASCII 路径: {b}");
                throw new BuildFailedException($"存在 {bad.Count} 处非 ASCII 资源路径，请先执行 Tools/Assets/ASCII Migration/Apply。");
            }
        }
    }
}
