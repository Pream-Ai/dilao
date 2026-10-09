using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>
    /// 一次性把 Assets 下的中文目录名/文件名迁移为 ASCII。
    /// - 使用 AssetDatabase.MoveAsset，GUID / 引用保持不变；
    /// - 先移文件（同目录内改主名），再自底向上移目录（只改父目录下的叶子名），避免目标目录不存在；
    /// - 可干跑（Dry Run）输出映射报表，可重复执行（幂等）。
    /// </summary>
    public static class AsciiAssetMigration
    {
        // 第三方库 / 待删除目录，不参与迁移
        internal static readonly string[] ExcludePrefixes =
        {
            "Assets/Plugins/hotUpdate", // xLua 源码，二期整体移除
        };

        // 直接删除的垃圾文件/目录（不参与迁移，也不在报表里报未映射）
        internal static readonly string[] JunkPaths =
        {
            "Assets/Resource/ui/Freebuttons/__MACOSX",
            "Assets/Resource/文件_元气满满！.ogg.html",
        };

        internal static bool IsJunk(string assetPath)
        {
            foreach (var j in JunkPaths)
                if (assetPath == j || assetPath.StartsWith(j + "/", StringComparison.Ordinal))
                    return true;
            return false;
        }

        const string ReportPath = "Logs/ascii-migration-report.txt";

        [MenuItem("Tools/Assets/ASCII Migration/Dry Run (只输出报表)")]
        public static void DryRunMenu() => Run(apply: false, quit: false);

        [MenuItem("Tools/Assets/ASCII Migration/Apply (执行迁移)")]
        public static void ApplyMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "ASCII 资源命名迁移",
                    "将重命名 Assets 下所有中文目录/文件为 ASCII（GUID 不变），并删除垃圾文件。\n\n确定继续？",
                    "执行", "取消"))
                return;
            Run(apply: true, quit: false);
        }

        // 供 Unity -executeMethod 调用
        public static void DryRunBatch() => Run(apply: false, quit: true);
        public static void ApplyBatch() => Run(apply: true, quit: true);

        static bool IsExcluded(string assetPath)
        {
            foreach (var p in ExcludePrefixes)
                if (assetPath == p || assetPath.StartsWith(p + "/", StringComparison.Ordinal))
                    return true;
            return false;
        }

        static void Run(bool apply, bool quit)
        {
            int exitCode = 0;
            try
            {
                var sb = new StringBuilder();
                var fileMoves = new List<(string oldPath, string newPath)>();
                var dirMoves = new List<(string oldPath, string newPath)>();
                var unmapped = new List<string>();

                // 1) 目录：只改“当前父目录下的叶子名”，自底向上执行
                var dataPath = Application.dataPath.Replace('\\', '/');
                foreach (var abs in Directory.GetDirectories(dataPath, "*", SearchOption.AllDirectories))
                {
                    var dir = "Assets" + abs.Replace('\\', '/').Substring(dataPath.Length);
                    if (IsExcluded(dir) || IsJunk(dir)) continue;
                    var parent = dir.Substring(0, dir.LastIndexOf('/'));
                    var name = dir.Substring(dir.LastIndexOf('/') + 1);
                    var mapped = AsciiNameMap.ToAscii(name, isFile: false, out var changed, out var ok);
                    if (!ok) { unmapped.Add(dir); continue; }
                    if (changed) dirMoves.Add((dir, parent + "/" + mapped));
                }
                dirMoves = dirMoves.OrderByDescending(m => m.oldPath.Count(c => c == '/')).ToList();

                // 2) 文件：同目录内改主名
                foreach (var path in AssetDatabase.GetAllAssetPaths())
                {
                    if (!path.StartsWith("Assets/", StringComparison.Ordinal)) continue;
                    if (IsExcluded(path) || IsJunk(path)) continue;
                    if (AssetDatabase.IsValidFolder(path)) continue; // 目录已在上面处理
                    var parent = path.Substring(0, path.LastIndexOf('/'));
                    var name = path.Substring(path.LastIndexOf('/') + 1);
                    var mapped = AsciiNameMap.ToAscii(name, isFile: true, out var changed, out var ok);
                    if (!ok) { unmapped.Add(path); continue; }
                    if (changed) fileMoves.Add((path, parent + "/" + mapped));
                }

                sb.AppendLine("==== ASCII Asset Migration Report ====");
                sb.AppendLine($"mode      : {(apply ? "APPLY" : "DRY-RUN")}");
                sb.AppendLine($"time      : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"fileMoves : {fileMoves.Count}");
                sb.AppendLine($"dirMoves  : {dirMoves.Count}");
                sb.AppendLine($"unmapped  : {unmapped.Count}");
                sb.AppendLine();

                sb.AppendLine("---- FILES ----");
                foreach (var m in fileMoves)
                    sb.AppendLine($"{m.oldPath}  =>  {m.newPath}");
                sb.AppendLine();
                sb.AppendLine("---- DIRECTORIES ----");
                foreach (var m in dirMoves)
                    sb.AppendLine($"{m.oldPath}  =>  {m.newPath}");
                sb.AppendLine();
                sb.AppendLine("---- UNMAPPED (需人工补映射) ----");
                foreach (var u in unmapped)
                    sb.AppendLine(u);

                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, sb.ToString(), new UTF8Encoding(false));
                Debug.Log($"[AsciiMigration] 报表已写入 {ReportPath}\n文件 {fileMoves.Count} 个，目录 {dirMoves.Count} 个，未映射 {unmapped.Count} 个。");

                if (!apply)
                {
                    Debug.Log("[AsciiMigration] DRY-RUN，未做任何修改。");
                }
                else
                {
                    // 3) 删除垃圾
                    foreach (var junk in JunkPaths)
                    {
                        if (AssetDatabase.IsValidFolder(junk) || File.Exists(junk) || Directory.Exists(junk))
                        {
                            AssetDatabase.DeleteAsset(junk);
                            Debug.Log($"[AsciiMigration] 删除垃圾: {junk}");
                        }
                    }

                    int moved = 0, failed = 0;

                    // 文件先移
                    foreach (var m in fileMoves)
                    {
                        if (!File.Exists(m.oldPath)) continue; // 目录后缀规则可能已改变? 不存在则跳过
                        var err = AssetDatabase.MoveAsset(m.oldPath, m.newPath);
                        if (string.IsNullOrEmpty(err)) moved++;
                        else { failed++; Debug.LogError($"[AsciiMigration] 移动文件失败 {m.oldPath} => {m.newPath}: {err}"); }
                    }

                    // 目录自底向上移
                    foreach (var m in dirMoves)
                    {
                        if (!AssetDatabase.IsValidFolder(m.oldPath)) continue;
                        var err = AssetDatabase.MoveAsset(m.oldPath, m.newPath);
                        if (string.IsNullOrEmpty(err)) moved++;
                        else { failed++; Debug.LogError($"[AsciiMigration] 移动目录失败 {m.oldPath} => {m.newPath}: {err}"); }
                    }

                    // 清理残留 .DS_Store
                    int ds = 0;
                    foreach (var f in Directory.GetFiles(dataPath, ".DS_Store", SearchOption.AllDirectories))
                    {
                        File.Delete(f);
                        ds++;
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log($"[AsciiMigration] 完成：成功 {moved}，失败 {failed}，清理 .DS_Store {ds} 个，未映射 {unmapped.Count} 个。");
                    if (failed > 0 || unmapped.Count > 0) exitCode = 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AsciiMigration] 异常: {e}");
                exitCode = 1;
            }
            finally
            {
                if (quit) EditorApplication.Exit(exitCode);
            }
        }
    }
}
