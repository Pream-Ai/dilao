using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>
    /// 目录即配置的 Addressables 管线（移植自 merge_vertex 的 AddressableUtil，去掉 HybridCLR/Newtonsoft）。
    ///
    /// 约定：
    ///   Assets/Project/AddressableRes/Bundles/&lt;分类&gt;/m_xxx[_dl][_r]/...
    ///     - m_ = PackTogether（多包合一）/ s_ = PackSeparately（单包）
    ///     - _dl = 动态加载（生成 AAConst 常量）
    ///     - _r  = 远端
    ///     - 目录名 = 组名；address = 相对组父目录的路径
    /// 构建前会清空并按目录重建所有组，并生成 AAConst.cs。
    /// 远端路径/目录由环境变量注入：BUILD_VERSION / BUILD_BRANCH / RELEASE_BUILD。
    /// </summary>
    public static class AddressableUtil
    {
        public const string RootGroupPath = "Assets/Project/AddressableRes";
        public const string KeyAddressScriptPath = "Assets/Project/AddressableRes/Common/Scripts/Definition/AAConst.cs";
        public const string DefaultLocalGroupName = "DefaultLocalGroup";
        public const string BuiltInDataGroupName = "Built In Data";
        public const string RemoteLabel = "Remote";

        public const string SinglePackPrefix = "s_";
        public const string MultiPackPrefix = "m_";
        public const string IgnoreBuildTag = "__i";
        public static readonly Regex DynamicLoadSuffix = new(@"_dl(?:_|\b)");
        public static readonly Regex RemoteLoadSuffix = new(@"_r(?:_|\b)");

        private static readonly List<string> RepeatAddressErrors = new();
        private static readonly HashSet<string> IgnoreExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            "", ".meta", ".cs", ".dll", ".ds_store", ".keep", ".settings", ".asmdef", ".asmref", ".bundle.settings"
        };

        private const string TemplateAAConst = @"using System.Collections.Generic;
using UnityEngine;
public class AAConst
{{{0}

    public static readonly IReadOnlyDictionary<string, string> keyAddressDict = new Dictionary<string, string>()
    {{{1}
    }};

    public static string GetAddress(string key)
    {{
        if (string.IsNullOrEmpty(key)) return """";
        if (keyAddressDict.TryGetValue(key, out var address))
        {{
            return address;
        }}
        Debug.Log($""[AAConst] address not found: key == {{key}}"");
        return """";
    }}
}}
";
        private const string TemplateAAConstStatic = "\n    public const string {0} = \"{1}\";";
        private const string TemplateAAConstDict = "\n        {{\"{0}\", {1}}},";

        // ------------------------------------------------------------------ 菜单 / 入口
        [MenuItem("Tools/Addressable/Build Addressable Contents")]
        public static void BuildAddressableContentsMenu()
        {
            RepeatAddressErrors.Clear();
            try
            {
                BuildAddressableContents();
                EditorUtility.ClearProgressBar();
                Debug.Log($"[AddressableUtil] 分组重建完成。{(RepeatAddressErrors.Count > 0 ? "地址告警 " + RepeatAddressErrors.Count + " 条" : "")}");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[AddressableUtil] 分组重建失败: " + e);
                throw;
            }
        }

        [MenuItem("Tools/Addressable/Build Remote Content")]
        public static void BuildRemoteContentMenu() => BuildRemoteContent();

        public static void BuildContentsBatch()
        {
            int code = 0;
            try { BuildAddressableContents(); }
            catch (Exception e) { Debug.LogError(e); code = 1; }
            finally { if (Application.isBatchMode) EditorApplication.Exit(code); }
        }

        public static void BuildRemoteBatch()
        {
            int code = 0;
            try { BuildRemoteContent(); }
            catch (Exception e) { Debug.LogError(e); code = 1; }
            finally { if (Application.isBatchMode) EditorApplication.Exit(code); }
        }

        // ------------------------------------------------------------------ 主流程
        public static void BuildAddressableContents()
        {
            if (Application.isBatchMode)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            }

            CheckAddressableSettings();
            RemoveAllGroups();
            var aaConst = ScanGroupedEntities();
            ScanUnGroupedEntities();
            RemoveAllEmptyGroups();
            SaveAAConst(aaConst);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void CheckAddressableSettings()
        {
            var path = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
            if (settings == null) throw new Exception("Addressable settings not found: " + path);
            AddressableAssetSettingsDefaultObject.Settings = settings;
        }

        private static void RemoveAllGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var group in settings.groups.ToArray())
            {
                if (group == null || group.Name == BuiltInDataGroupName) continue;
                settings.RemoveGroup(group);
            }
            EditorUtility.SetDirty(settings);
        }

        [MenuItem("Tools/Addressable/Remove All Empty Groups")]
        public static void RemoveAllEmptyGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = new List<AddressableAssetGroup>(settings.groups);
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                var group = groups[i];
                if (group == null || group.Name == BuiltInDataGroupName || group.Default) continue;
                if (group.entries != null && group.entries.Count > 0) continue;
                settings.RemoveGroup(group);
            }
            EditorUtility.SetDirty(settings);
        }

        private static Dictionary<string, string> ScanGroupedEntities()
        {
            var keyAddressMap = new Dictionary<string, string>();
            var groupNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            FindOrCreateLocalGroup(settings, DefaultLocalGroupName, false, false);

            string rootDir = Path.Combine(Environment.CurrentDirectory, RootGroupPath);
            if (!Directory.Exists(rootDir)) return keyAddressMap;

            var directories = BFSEnumerateDirectories(rootDir,
                    dir => dir.Name.StartsWith(MultiPackPrefix) || dir.Name.StartsWith(SinglePackPrefix) ||
                           BundleSettings.Load(dir.FullName)?.isBundle == true)
                .OrderBy(p => p.Name).ToArray();

            for (int i = 0; i < directories.Length; i++)
            {
                var dirInfo = directories[i];
                string dirName = dirInfo.Name;
                if (dirName.Contains(IgnoreBuildTag)) continue;

                if (!Application.isBatchMode)
                    EditorUtility.DisplayProgressBar("Grouping", $"Grouping {dirName}...", (i + 1) / (float)directories.Length);

                if (!groupNameSet.Add(dirName)) throw new Exception("Duplicate addressable group: " + dirInfo.FullName);

                var bundleSettings = BundleSettings.Load(dirInfo.FullName);
                bool isSingle = dirName.StartsWith(SinglePackPrefix) || bundleSettings?.isSingle == true;
                bool isDynamic = DynamicLoadSuffix.IsMatch(dirName) || bundleSettings?.isDynamic == true;
                bool isRemote = RemoteLoadSuffix.IsMatch(dirName) || bundleSettings?.isRemote == true;
                bool isUncompressed = bundleSettings?.isUncompressed == true;

                var group = isRemote
                    ? FindOrCreateRemoteGroup(settings, dirName, isSingle, isUncompressed)
                    : FindOrCreateLocalGroup(settings, dirName, isSingle, isUncompressed);

                foreach (var fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).OrderBy(f => f.FullName))
                {
                    if (fileInfo.Name.Contains(IgnoreBuildTag)) continue;
                    if (IgnoreExtensions.Contains(fileInfo.Extension)) continue;

                    bool isDynamicFile = isDynamic || BundleSettings.IsDynamicFile(fileInfo.FullName, dirInfo.FullName);
                    var assetPath = fileInfo.FullName.Substring(Environment.CurrentDirectory.Length + 1).Replace('\\', '/');
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (string.IsNullOrEmpty(guid)) continue;

                    var address = fileInfo.FullName.Substring(dirInfo.Parent.FullName.Length + 1).Replace('\\', '/');
                    var entry = group.GetAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, group, false, false);
                    if (entry == null) throw new Exception($"CreateOrMoveEntry failed: {assetPath}");
                    entry.SetAddress(address);

                    if (isDynamicFile)
                        RegisterKey(keyAddressMap, fileInfo, dirInfo.FullName, address, assetPath);

                    if (isRemote)
                    {
                        settings.AddLabel(dirName, false);
                        entry.SetLabel(dirName, true);
                        settings.AddLabel(RemoteLabel, false);
                        entry.SetLabel(RemoteLabel, true);
                        if (bundleSettings?.labels != null)
                            foreach (var label in bundleSettings.labels)
                            {
                                settings.AddLabel(label, false);
                                entry.SetLabel(label, true);
                            }
                    }
                }
            }

            EditorUtility.SetDirty(settings);
            return keyAddressMap;
        }

        /// <summary>生成 AAConst 的 key（组内相对路径去掉扩展名，冲突时带上父路径/扩展名）。</summary>
        private static void RegisterKey(Dictionary<string, string> map, FileInfo fileInfo, string groupDir, string address, string assetPath)
        {
            var rel = fileInfo.FullName.Substring(groupDir.Length + 1).Replace('\\', '/');
            var key = ParseKey(Path.ChangeExtension(rel, null).Replace('/', '_'));
            if (map.ContainsKey(key))
            {
                // 冲突：退化为“路径(含扩展名)”保证唯一
                var fallback = ParseKey(rel.Replace('/', '_').Replace('.', '_'));
                if (map.ContainsKey(fallback))
                {
                    var msg = "Duplicate address key: " + key + " @ " + assetPath;
                    Debug.LogError(msg);
                    RepeatAddressErrors.Add(msg);
                    return;
                }
                key = fallback;
            }
            map[key] = address;
        }

        private static void SaveAAConst(Dictionary<string, string> keyAddressMap)
        {
            var keys = keyAddressMap.Keys.OrderBy(x => x).ToList();
            var statics = keys.Select(k => string.Format(TemplateAAConstStatic, k, keyAddressMap[k]));
            var dict = keys.Select(k => string.Format(TemplateAAConstDict, k, k));
            var content = string.Format(TemplateAAConst, string.Join("", statics), string.Join("", dict));

            var destPath = Path.Combine(Environment.CurrentDirectory, KeyAddressScriptPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            File.WriteAllText(destPath, content, new UTF8Encoding(false));
            Debug.Log($"[AddressableUtil] AAConst 生成: {keys.Count} 条 -> {KeyAddressScriptPath}");
        }

        private static string ParseKey(string str)
        {
            str = str.Trim().Replace(" ", "_").Replace(".", "_").Replace("-", "_").Replace("(", "_").Replace(")", "_");
            if (str.Length > 0 && char.IsDigit(str[0])) str = "_" + str;
            return str;
        }

        // ------------------------------------------------------------------ 组 / Schema
        public static AddressableAssetGroup FindOrCreateLocalGroup(AddressableAssetSettings settings, string groupName,
            bool isSingle, bool isUncompressed)
        {
            var group = settings.FindGroup(groupName);
            if (group != null) return group;
            group = CreateGroup(settings, groupName);
            var schema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = isSingle ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately
                                         : BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.FullPath;
            schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            schema.AssetBundledCacheClearBehavior = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenSpaceIsNeededInCache;
            schema.UseAssetBundleCrc = false;
            schema.UseAssetBundleCrcForCachedBundles = false;
            schema.IncludeInBuild = true;
            schema.IncludeGUIDInCatalog = false;
            schema.Compression = isUncompressed ? BundledAssetGroupSchema.BundleCompressionMode.Uncompressed
                                                : BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            schema.UseUnityWebRequestForLocalBundles = schema.Compression == BundledAssetGroupSchema.BundleCompressionMode.LZMA;
            var update = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
            update.StaticContent = true;
            EditorUtility.SetDirty(settings);
            return group;
        }

        public static AddressableAssetGroup FindOrCreateRemoteGroup(AddressableAssetSettings settings, string groupName,
            bool isSingle, bool isUncompressed)
        {
            var group = settings.FindGroup(groupName);
            if (group != null) return group;
            group = CreateGroup(settings, groupName);
            var schema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = isSingle ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately
                                         : BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.FullPath;
            schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
            schema.AssetBundledCacheClearBehavior = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenSpaceIsNeededInCache;
            schema.UseAssetBundleCrc = true;
            schema.UseAssetBundleCrcForCachedBundles = false;
            schema.Compression = isUncompressed ? BundledAssetGroupSchema.BundleCompressionMode.Uncompressed
                                                : BundledAssetGroupSchema.BundleCompressionMode.LZMA;
            schema.Timeout = 20;
            schema.RetryCount = 3;
            schema.IncludeInBuild = true;
            schema.IncludeGUIDInCatalog = false;
            schema.UseUnityWebRequestForLocalBundles = schema.Compression == BundledAssetGroupSchema.BundleCompressionMode.LZMA;
            var update = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
            update.StaticContent = false;
            EditorUtility.SetDirty(settings);
            return group;
        }

        private static AddressableAssetGroup CreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.CreateGroup(groupName, false, false, false, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            if (groupName == DefaultLocalGroupName) settings.DefaultGroup = group;
            return group;
        }

        /// <summary>BFS 遍历子目录，命中 predicate 的目录作为叶子返回。</summary>
        private static IEnumerable<DirectoryInfo> BFSEnumerateDirectories(string path, Func<DirectoryInfo, bool> predicate)
        {
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(new DirectoryInfo(path));
            while (queue.TryDequeue(out var current))
            {
                if (predicate(current)) { yield return current; continue; }
                foreach (var sub in current.EnumerateDirectories().OrderBy(d => d.Name))
                    if (!current.Name.StartsWith('.')) queue.Enqueue(sub);
            }
        }

        /// <summary>把被引用但未分组的 AddressableRes 下资源放进默认本地组。</summary>
        private static void ScanUnGroupedEntities()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groupedEntries = settings.groups.SelectMany(g => g.entries).Select(e => e.AssetPath).ToArray();
            var allEntries = AssetDatabase.GetDependencies(groupedEntries, true)
                .Where(f => f.StartsWith(RootGroupPath, StringComparison.Ordinal) &&
                            !IgnoreExtensions.Contains(Path.GetExtension(f)));
            var ungrouped = allEntries.Except(groupedEntries).OrderBy(f => f);
            var defaultGroup = FindOrCreateLocalGroup(settings, DefaultLocalGroupName, false, false);
            foreach (var path in ungrouped)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid)) continue;
                var entry = defaultGroup.GetAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, defaultGroup, false, false);
                entry?.SetAddress(path.Substring(RootGroupPath.Length + 1).Replace('\\', '/'));
            }
        }

        // ------------------------------------------------------------------ 构建 / 远端配置
        public static void BuildRemoteContent()
        {
            BuildAddressableContents();
            ConfigureForBuild(EditorUserBuildSettings.activeBuildTarget.ToString());
            AddressableAssetSettings.BuildPlayerContent(out var result);
            if (!string.IsNullOrEmpty(result.Error))
                throw new InvalidOperationException("BuildPlayerContent failed: " + result.Error);
            Debug.Log($"[AddressableUtil] BuildPlayerContent 完成: {result.Duration}s");
        }

        /// <summary>
        /// 配置远端构建/加载路径与 RemoteCatalog。
        /// 有 BUILD_VERSION 环境变量 -> CI 远端目录；否则本地开发路径。
        /// </summary>
        public static void ConfigureForBuild(string platform)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            var version = Environment.GetEnvironmentVariable("BUILD_VERSION");
            var branch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "main";
            var isRelease = Environment.GetEnvironmentVariable("RELEASE_BUILD") == "true";
            var profileId = settings.activeProfileId;

            settings.BuildRemoteCatalog = true;
            settings.DisableCatalogUpdateOnStartup = true;

            if (string.IsNullOrEmpty(version))
            {
                // 本地开发：内网 Nginx / 本地静态服务器
                settings.ContentStateBuildPath = "ServerData";
                settings.OverridePlayerVersion = "";
                settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath,
                    "ServerData/Remote/[BuildTarget]");
                settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath,
                    "http://127.0.0.1:8080/dilao/[BuildTarget]");
                ResetRemoteCatalogToRemoteVariables(settings);
                Debug.Log("[AddressableUtil] ConfigureForBuild: 本地开发路径");
            }
            else
            {
                var cdn = isRelease ? "https://cdn.example.com" : "https://cdn-test.example.com";
                var outputPath = $"Build/ServerData/{version}/{platform}/native-content";
                var loadBase = $"{cdn}/dilao/{branch}/delivery-v2/native-content/{version}/{platform}";

                settings.ContentStateBuildPath = "Build/ServerData";
                settings.OverridePlayerVersion = $"{version}_{platform}";
                settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath, outputPath);
                settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, loadBase + "/bundles");

                const string rcBuild = "RemoteCatalog.BuildPath";
                const string rcLoad = "RemoteCatalog.LoadPath";
                settings.profileSettings.CreateValue(rcBuild, outputPath);
                settings.profileSettings.SetValue(profileId, rcBuild, outputPath);
                settings.profileSettings.CreateValue(rcLoad, loadBase);
                settings.profileSettings.SetValue(profileId, rcLoad, loadBase);
                settings.RemoteCatalogBuildPath.SetVariableByName(settings, rcBuild);
                settings.RemoteCatalogLoadPath.SetVariableByName(settings, rcLoad);
                Debug.Log($"[AddressableUtil] ConfigureForBuild: CI version={version}, platform={platform}, load={loadBase}");
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static void ResetRemoteCatalogToRemoteVariables(AddressableAssetSettings settings)
        {
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        }
    }

    /// <summary>每次构建前重建 Addressable 分组（内容构建仍由 BuildRemoteContent 显式触发）。</summary>
    public class AddressablePreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, AddressableUtil.RootGroupPath))) return;
            AddressableUtil.BuildAddressableContents();
        }
    }
}
