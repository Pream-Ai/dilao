using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>
    /// Bundle 文件夹设置：放在组目录下的 .bundle.settings 可覆盖命名约定的默认行为。
    /// （移植自 merge_vertex，改用 Unity JsonUtility，去掉 Newtonsoft 依赖）
    /// </summary>
    [Serializable]
    public class BundleSettings
    {
        public bool isBundle;
        public bool isSingle;
        public bool isDynamic;
        public bool isRemote;
        public bool isAtlas;
        public bool isUncompressed;
        public string[] labels;

        private const string SettingFileName = ".bundle.settings";
        private static readonly Dictionary<string, BundleSettings> Cache = new();

        public static BundleSettings Load(string folderPath)
        {
            if (folderPath.StartsWith("Assets/", StringComparison.Ordinal))
                folderPath = Path.Combine(Environment.CurrentDirectory, folderPath);
            if (Cache.TryGetValue(folderPath, out var settings) && settings != null) return settings;
            var settingPath = Path.Combine(folderPath, SettingFileName);
            if (File.Exists(settingPath))
            {
                try { settings = JsonUtility.FromJson<BundleSettings>(File.ReadAllText(settingPath, Encoding.UTF8)); }
                catch { settings = null; }
            }
            Cache[folderPath] = settings;
            return settings;
        }

        public static void Save(string folderPath, BundleSettings settings)
        {
            if (folderPath.StartsWith("Assets/", StringComparison.Ordinal))
                folderPath = Path.Combine(Environment.CurrentDirectory, folderPath);
            var settingPath = Path.Combine(folderPath, SettingFileName);
            File.WriteAllText(settingPath, JsonUtility.ToJson(settings, true), Encoding.UTF8);
            Cache[folderPath] = settings;
        }

        [MenuItem("Assets/Refresh Bundle Cache", false, 1)]
        public static void ClearCache()
        {
            Cache.Clear();
            EditorApplication.RepaintProjectWindow();
        }

        /// <summary>路径是否处于动态加载目录（目录名含 _dl 或祖先目录标记 isDynamic）</summary>
        public static bool IsDynamicFile(string path, string rootFolderPath)
        {
            if (AddressableUtil.DynamicLoadSuffix.IsMatch(path.Substring(rootFolderPath.Length + 1))) return true;
            for (path = Path.GetDirectoryName(path); path != null && path.Length > rootFolderPath.Length; path = Path.GetDirectoryName(path))
                if (Load(path)?.isDynamic == true) return true;
            return false;
        }
    }
}
