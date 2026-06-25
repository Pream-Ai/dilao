using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine;

public class AssetRenameTool
{
    //public static void RenameSelectedAssetsToPinyin()
    //{
    //    Object[] selectedObjects = Selection.objects;
    //    if (selectedObjects.Length == 0)
    //    {
    //        Debug.LogWarning("军师提示：请先在 Project 窗口框选你需要改名的中文资源！");
    //        return;
    //    }

    //    int successCount = 0;

    //    foreach (Object obj in selectedObjects)
    //    {
    //        string assetPath = AssetDatabase.GetAssetPath(obj);
    //        if (string.IsNullOrEmpty(assetPath) || Directory.Exists(assetPath)) continue;

    //        string oldName = obj.name; // 比如 "哥布林_头像_01"

    //        // ⚡ 核心转换：物理翻译成拼音
    //        string newName = GetPinyin(oldName);

    //        if (oldName == newName) continue; // 如果本来就是英文/拼音，跳过

    //        string error = AssetDatabase.RenameAsset(assetPath, newName);

    //        if (string.IsNullOrEmpty(error))
    //        {
    //            successCount++;
    //        }
    //        else
    //        {
    //            Debug.LogError($"文件 {oldName} 改名失败: {error}");
    //        }
    //    }

    //    AssetDatabase.Refresh();
    //    Debug.Log($"🎉 战役大捷！成功将 {successCount} 个中文资源无缝洗脑为【纯拼音】！");
    //}

    ///// <summary>
    ///// 纯 C# 物理级汉字转全拼实现（通过区位码精准拦截映射）
    ///// </summary>
    //private static string GetPinyin(string chineseString)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    byte[] array = new byte[2];

    //    foreach (char c in chineseString)
    //    {
    //        int ascii = (int)c;
    //        // 1. 如果本来就是英文、数字、下划线，直接保留
    //        if ((ascii >= 48 && ascii <= 57) || (ascii >= 65 && ascii <= 90) || (ascii >= 97 && ascii <= 122) || c == '_')
    //        {
    //            sb.Append(c.ToString().ToLower()); // 统一强制小写，防御大写乱入
    //            continue;
    //        }

    //        // 2. 如果是汉字，通过字节区位码强行解码
    //        array = Encoding.Default.GetBytes(c.ToString());
    //        if (array.Length < 2) continue; // 非中文字符过滤

    //        int i = (short)(array[0] << 8) + (short)(array[1]);

    //        if (i >= 45217 && i <= 45252) sb.Append("a");
    //        else if (i >= 45253 && i <= 45760) sb.Append("b");
    //        else if (i >= 45761 && i <= 46317) sb.Append("c");
    //        else if (i >= 46318 && i <= 46825) sb.Append("d");
    //        else if (i >= 46826 && i <= 47009) sb.Append("e");
    //        else if (i >= 47010 && i <= 47296) sb.Append("f");
    //        else if (i >= 47297 && i <= 47613) sb.Append("g");
    //        else if (i >= 47614 && i <= 48118) sb.Append("h");
    //        else if (i >= 48119 && i <= 49061) sb.Append("j");
    //        else if (i >= 49062 && i <= 49323) sb.Append("k");
    //        else if (i >= 49324 && i <= 49895) sb.Append("l");
    //        else if (i >= 49896 && i <= 50370) sb.Append("m");
    //        else if (i >= 50371 && i <= 50613) sb.Append("n");
    //        else if (i >= 50614 && i <= 50621) sb.Append("o");
    //        else if (i >= 50622 && i <= 50905) sb.Append("p");
    //        else if (i >= 50906 && i <= 51386) sb.Append("q");
    //        else if (i >= 51387 && i <= 51445) sb.Append("r");
    //        else if (i >= 51446 && i <= 52217) sb.Append("s");
    //        else if (i >= 52218 && i <= 52697) sb.Append("t");
    //        else if (i >= 52698 && i <= 52979) sb.Append("w");
    //        else if (i >= 52980 && i <= 53688) sb.Append("x");
    //        else if (i >= 53689 && i <= 54480) sb.Append("y");
    //        else if (i >= 54481 && i <= 55289) sb.Append("z");
    //    }
    //    return sb.ToString();
    //}
}