using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>
    /// 一键接入 Addressables：
    ///  1) 生成 AddressableAssetsData 设置资产
    ///  2) 配置 Profile 变量（Local/Remote Build&Load Path）+ 远端 Catalog
    ///  3) 按目录规则自动建组、设 Address/Label
    /// 可通过菜单执行，也可用 Unity -executeMethod 批量执行。
    /// </summary>
    public static class AddressablesBootstrap
    {
        // nginx 远端地址（内网自建，后续改这里即可）
        const string RemoteLoadRoot = "http://127.0.0.1:8080/dilao/[BuildTarget]";
        const string RemoteBuildRoot = "ServerData/Remote/[BuildTarget]";
        // 本地组必须 BuildPath/LoadPath 同为 dynamic-lookup（Addressables 校验）
        const string LocalBuildRoot = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";

        const string ReportPath = "Logs/addressables-setup-report.txt";

        // ------------------------------------------------------------------ 菜单
        [MenuItem("Tools/Assets/Addressables/Setup (Settings + Groups)")]
        public static void SetupMenu() => SetupAll();

        [MenuItem("Tools/Assets/Addressables/Report")]
        public static void ReportMenu() => WriteReport();

        // ------------------------------------------------------------- 批处理入口
        public static void SetupBatch()
        {
            int code = 0;
            try { SetupAll(); }
            catch (Exception e) { Debug.LogError($"[AddressablesBootstrap] {e}"); code = 1; }
            finally { if (Application.isBatchMode) EditorApplication.Exit(code); }
        }

        public static void BuildRemoteBatch()
        {
            int code = 0;
            try
            {
                var settings = GetSettings();
                AddressableAssetSettings.BuildPlayerContent(out var result);
                WriteReport();
                Debug.Log($"[AddressablesBootstrap] 构建完成: {result}。输出目录 ServerData/Remote/[BuildTarget]。");
                if (settings == null) code = 1;
            }
            catch (Exception e) { Debug.LogError($"[AddressablesBootstrap] Build 失败: {e}"); code = 1; }
            finally { if (Application.isBatchMode) EditorApplication.Exit(code); }
        }

        // ------------------------------------------------------------------ 主流程
        public static void SetupAll()
        {
            var settings = GetSettings();

            ConfigureProfile(settings);
            ConfigureRemoteCatalog(settings);
            ConfigureGlobal(settings);
            ApplyGroups(settings);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteReport();
            Debug.Log("[AddressablesBootstrap] Addressables 设置与分组完成。");
        }

        static AddressableAssetSettings GetSettings()
        {
            // create=true：不存在则生成 Assets/AddressableAssetsData/AddressableAssetSettings.asset
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null) throw new Exception("无法创建 AddressableAssetSettings");
            return settings;
        }

        static void ConfigureProfile(AddressableAssetSettings settings)
        {
            var pid = settings.activeProfileId;
            settings.profileSettings.SetValue(pid, AddressableAssetSettings.kLocalBuildPath, LocalBuildRoot);
            settings.profileSettings.SetValue(pid, AddressableAssetSettings.kLocalLoadPath,
                "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]");
            settings.profileSettings.SetValue(pid, AddressableAssetSettings.kRemoteBuildPath, RemoteBuildRoot);
            settings.profileSettings.SetValue(pid, AddressableAssetSettings.kRemoteLoadPath, RemoteLoadRoot);
        }

        static void ConfigureRemoteCatalog(AddressableAssetSettings settings)
        {
            settings.BuildRemoteCatalog = true;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        }

        static void ConfigureGlobal(AddressableAssetSettings settings)
        {
            settings.DisableCatalogUpdateOnStartup = false; // 启动时检查 catalog 更新
            settings.MaxConcurrentWebRequests = 8;
        }

        // ------------------------------------------------------------------ 分组
        static Dictionary<string, AddressableAssetGroup> _cache;

        static AddressableAssetGroup Group(AddressableAssetSettings settings, string name, bool remote)
        {
            if (_cache == null) _cache = new Dictionary<string, AddressableAssetGroup>();
            if (_cache.TryGetValue(name, out var cached)) return cached;

            var group = settings.groups.Find(g => g != null && g.Name == name);
            if (group == null)
            {
                group = settings.CreateGroup(name, false, false, false, null,
                    typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema != null)
            {
                schema.BuildPath.SetVariableByName(settings,
                    remote ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath);
                schema.LoadPath.SetVariableByName(settings,
                    remote ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath);
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
                schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
                schema.IncludeInBuild = true;
            }

            var cu = group.GetSchema<ContentUpdateGroupSchema>();
            if (cu != null) cu.StaticContent = false;

            _cache[name] = group;
            return group;
        }

        /// <summary>清空由本工具管理的组，保证规则幂等、无历史残留。</summary>
        static void ClearManaged(AddressableAssetSettings settings)
        {
            foreach (var g in settings.groups
                         .Where(x => x != null &&
                                     (x.Name.StartsWith("Local_") || x.Name.StartsWith("Remote_") ||
                                      x.Name.StartsWith("Furni_") || x.Name.StartsWith("Npc_")))
                         .ToList())
            {
                foreach (var e in g.entries.ToList())
                    settings.RemoveAssetEntry(e.guid, false);
            }
        }

        static void Add(AddressableAssetSettings settings, AddressableAssetGroup group, string path,
            string address = null, string label = null)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            // 不可导入的文件（如 PDF/文档）不能进 Addressables，否则内容构建失败
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == null) return;
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = string.IsNullOrEmpty(address) ? path : address;
            if (!string.IsNullOrEmpty(label))
            {
                settings.AddLabel(label, false);
                entry.SetLabel(label, true, true);
            }
        }

        static void AddDir(AddressableAssetSettings settings, AddressableAssetGroup group, string dir,
            string label)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
            {
                var p = f.Replace('\\', '/');
                if (p.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                Add(settings, group, p, null, label);
            }
        }

        /// <summary>从路径里找“最近的、属于某个 NPC 的祖先目录名”。</summary>
        static string OwnerNpc(string assetPath, HashSet<string> npcNames)
        {
            var parts = assetPath.Split('/');
            for (int i = parts.Length - 2; i >= 0; i--)
                if (npcNames.Contains(parts[i])) return parts[i];
            return null;
        }

        static void ApplyGroups(AddressableAssetSettings settings)
        {
            ClearManaged(settings);
            _cache = null;

            // ---- 本地基础包 ----
            var local = Group(settings, "Local_Base", remote: false);
            AddDir(settings, local, "Assets/Resource/fonts", "base");
            AddDir(settings, local, "Assets/Resource/ui", "ui");
            // 不主动加 Assets/TextMesh Pro：其 Resources 已由 Built In Data 的 Resources 收集，
            // 且 Documentation 下的 PDF 无法导入，会导致内容构建失败。

            // ---- 远端分类 ----
            var audio = Group(settings, "Remote_Audio", remote: true);
            AddDir(settings, audio, "Assets/Resource/audio", "audio");

            var misc = Group(settings, "Remote_Misc", remote: true);
            AddDir(settings, misc, "Assets/Resource/materials", "misc");
            AddDir(settings, misc, "Assets/Resource/pets", "misc");
            AddDir(settings, misc, "Assets/Resource/livingroom", "misc");
            AddDir(settings, misc, "Assets/Resource/furniture", "misc"); // 散图，稍后按家具名移动

            // ---- 家具：每件一个组 ----
            foreach (var prefab in Directory.GetFiles("Assets/Data/furni", "*.prefab", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(prefab);
                var g = Group(settings, "Furni_" + name, remote: true);
                Add(settings, g, prefab, "Furni/" + name, "furni");
                Add(settings, g, "Assets/Data/furni/" + name + ".asset", "Furni/" + name + "/Data", "furni");
                Add(settings, g, "Assets/Resource/furniture/" + name + ".png", "Furni/" + name + "/Icon", "furni");
            }

            // ---- NPC：每个角色一个组（含其立绘/动画/控制器） ----
            var npcNames = new HashSet<string>(
                Directory.GetFiles("Assets/Data/npc", "*.prefab", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension));

            foreach (var name in npcNames)
            {
                var g = Group(settings, "Npc_" + name, remote: true);
                Add(settings, g, "Assets/Data/npc/" + name + ".prefab", "Npc/" + name, "npc");
                Add(settings, g, "Assets/Data/npc/" + name + ".asset", "Npc/" + name + "/Data", "npc");
            }

            // 角色资源：按“最近的祖先目录名”归属，避免 youxia 把 jianshang 递归吞进去
            if (Directory.Exists("Assets/Resource/charactor"))
            {
                foreach (var f in Directory.GetFiles("Assets/Resource/charactor", "*", SearchOption.AllDirectories))
                {
                    if (f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                    var p = f.Replace('\\', '/');
                    var owner = OwnerNpc(p, npcNames);
                    var g = owner != null ? Group(settings, "Npc_" + owner, remote: true) : misc;
                    Add(settings, g, p, null, owner != null ? "npc" : "misc");
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }

        // ------------------------------------------------------------------ 报表
        static void WriteReport()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("==== Addressables Setup Report ====");
            sb.AppendLine($"time   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            if (settings == null)
            {
                sb.AppendLine("settings: (null)");
            }
            else
            {
                sb.AppendLine($"profile: {settings.profileSettings.GetProfileName(settings.activeProfileId)}");
                sb.AppendLine($"remoteCatalog: {settings.BuildRemoteCatalog}");
                sb.AppendLine();
                foreach (var g in settings.groups.Where(x => x != null).OrderBy(x => x.Name))
                {
                    var schema = g.GetSchema<BundledAssetGroupSchema>();
                    var load = schema != null ? schema.LoadPath.GetValue(settings) : "?";
                    var build = schema != null ? schema.BuildPath.GetValue(settings) : "?";
                    sb.AppendLine($"[{g.Name}]  entries={g.entries.Count}");
                    sb.AppendLine($"    build={build}");
                    sb.AppendLine($"    load ={load}");
                    foreach (var e in g.entries.OrderBy(e => e.address).Take(2000))
                        sb.AppendLine($"      - {e.address}   <= {e.AssetPath}");
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, sb.ToString(), new System.Text.UTF8Encoding(false));
            Debug.Log($"[AddressablesBootstrap] 报表: {ReportPath}");
        }
    }
}
