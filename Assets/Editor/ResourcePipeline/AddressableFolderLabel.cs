using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dilao.Tools
{
    /// <summary>在 Project 窗口给资源目录画 {Bundle,Dynamic,Remote} 标签（移植自 merge_vertex）。</summary>
    [InitializeOnLoad]
    public static class AddressableFolderLabel
    {
        static AddressableFolderLabel()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/Project/AddressableRes", System.StringComparison.Ordinal)) return;
            if (!AssetDatabase.IsValidFolder(path)) return;

            var folderName = Path.GetFileName(path);
            var fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
            var settings = BundleSettings.Load(fullPath);

            bool isBundle = folderName.StartsWith(AddressableUtil.MultiPackPrefix) ||
                            folderName.StartsWith(AddressableUtil.SinglePackPrefix) ||
                            settings?.isBundle == true;
            bool isDynamic = AddressableUtil.DynamicLoadSuffix.IsMatch(folderName) || settings?.isDynamic == true;
            bool isRemote = AddressableUtil.RemoteLoadSuffix.IsMatch(folderName) || settings?.isRemote == true;
            bool isIgnored = folderName.Contains(AddressableUtil.IgnoreBuildTag);

            var tags = new List<(string, Color)>();
            if (isBundle) tags.Add(("Bundle", Color.red));
            if (isDynamic) tags.Add(("Dynamic", Color.yellow));
            if (isRemote) tags.Add(("Remote", Color.yellow));
            if (isIgnored) tags.Add(("Ignored", Color.gray));
            if (tags.Count == 0) return;

            SkipLabel(folderName, ref selectionRect);
            DrawLabel("{", Color.white, ref selectionRect);
            for (int i = 0; i < tags.Count; i++)
                DrawLabel(tags[i].Item1 + (i == tags.Count - 1 ? "" : ", "), tags[i].Item2, ref selectionRect);
            DrawLabel("}", Color.white, ref selectionRect);
        }

        static void SkipLabel(string text, ref Rect rect)
        {
            var width = new GUIStyle().CalcSize(new GUIContent(text)).x + 21f;
            rect.x += width;
            rect.width -= width;
        }

        static void DrawLabel(string text, Color color, ref Rect rect)
        {
            var style = new GUIStyle { normal = new GUIStyleState { textColor = color } };
            var width = style.CalcSize(new GUIContent(text)).x + 1f;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), text, style);
            rect.x += width;
            rect.width -= width;
        }
    }
}
