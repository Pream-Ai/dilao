using System.Collections.Generic;
using System.IO;

namespace Dilao.Tools
{
    /// <summary>
    /// 中文资源名 -> ASCII 映射表。
    /// 规则：目录/文件名只保留 ASCII；中文只允许存在于 ScriptableObject 的展示字段与 UI 文本中。
    /// 命名来源优先复用 Data/npc、Data/furni 已有的拼音。
    /// </summary>
    public static class AsciiNameMap
    {
        // 精确匹配（取“去掉最后一个扩展名”的主名）
        static readonly Dictionary<string, string> Exact = new Dictionary<string, string>
        {
            // ---- Resource 根目录 ----
            { "家具", "furniture" },
            { "客厅", "livingroom" },
            { "材料", "materials" },
            { "宠物", "pets" },
            { "音频", "audio" },
            { "字体", "fonts" },
            { "表情", "emoji" },
            { "像素字体", "pixelfont" },
            { "地砖", "floor" },
            { "墙", "wall" },

            // ---- 家具子目录 ----
            { "扭蛋机", "niudanji" },
            { "沙包受击", "punchbag" },
            { "饮料机", "yinliaoji" },
            { "饮料", "drinks" },
            { "饮水机", "watercooler" },
            { "抓娃娃机", "clawmachine" },

            // ---- 角色（复用 Data/npc 拼音） ----
            { "德鲁伊", "deluyi" },
            { "法师", "fashi" },
            { "工程师", "engineer" },
            { "程序猿", "chengxuyuan" },
            { "机器人", "jiqiren" },
            { "精灵", "jingling" },
            { "狂战士", "kzs" },
            { "狼人", "langren" },
            { "炼金术师", "alchemist" },
            { "唱片术士", "changpianshushi" },
            { "瓦克恩", "waken" },
            { "牧师", "mushi" },
            { "骑士", "qishi" },
            { "圣骑士", "shengqishi" },
            { "吸血鬼", "xixuegui" },
            { "游侠", "youxia" },
            { "奸商", "jianshang" },
            { "经典", "classic" },
            { "经典皮肤", "classic" },
            { "炼金经典", "lianjinshushi" },

            // ---- 具体文件 ----
            { "装修队长", "decorator" },
            { "180px-装修队长_0", "decorator" },
            { "常用汉字", "common_chinese" },
            { "元气满满！.ogg", "full_of_energy" },
            { "IPix中文像素字体 SDF", "IPixPixelFont SDF" },
            { "IPix中文像素字体", "IPixPixelFont" },
            { "冰箱(开)", "fridge_open" },
            { "兑换机-开启", "exchanger_on" },
            { "零件", "parts" },
            { "路障", "roadblock" },
            { "木材", "wood" },
            { "牛奶", "milk" },
            { "扭蛋", "gacha" },
            { "生物质", "biomass" },
            { "苏打水", "soda" },
            { "铁矿", "iron_ore" },
            { "咖啡", "coffee" },
            { "果汁", "juice" },
            { "红牛", "redbull" },
            { "电池", "battery" },
            { "邮箱", "mailbox" },
            { "进度圈", "progress_ring" },
            { "魔法碎片_橙", "magic_shard_orange" },
            { "魔法碎片_黑曜", "magic_shard_obsidian" },
            { "魔法碎片_红", "magic_shard_red" },
            { "魔法碎片_蓝", "magic_shard_blue" },
            { "魔法碎片_绿", "magic_shard_green" },
            { "魔法碎片_青", "magic_shard_cyan" },
            { "魔法碎片_紫", "magic_shard_purple" },
            { "“滴溜溜”牌抓抓乐待机_0 (1)", "clawmachine_idle" },
            { "“滴溜溜”牌抓抓乐待机", "clawmachine_idle" },
            { "大笑表情", "emoji_laugh" },
            { "微笑表情", "emoji_smile" },
            { "问号表情", "emoji_question" },
            { "兴奋表情", "emoji_excited" },
            { "冷汗表情", "emoji_sweat" },
            { "骰子表情", "emoji_dice" },
            { "钟表表情", "emoji_clock" },
            { "波子表情", "emoji_bozi" },
            { "炸天小姐姐", "emoji_girl" },
            { "33px-哭表情", "emoji_cry" },
            { "33px-滑稽表情", "emoji_funny" },
            { "33px-生气表情", "emoji_angry" },
            { "30px-点赞表情", "emoji_like" },
            { "144px-拳套-正", "glove_front" },
            { "24px-特殊房间", "specialroom" },
            { "Unity TMP Sprite Asset 使用方法 (3)", "unity_tmp_sprite_asset_usage_3" },
        };

        /// <summary>
        /// 把单个路径组件（目录名或文件名）转成 ASCII。
        /// </summary>
        /// <param name="name">目录名，或带扩展名的文件名</param>
        /// <param name="isFile">true 表示文件（会拆分扩展名）</param>
        /// <param name="changed">是否需要改名</param>
        /// <param name="ok">是否成功映射（false 表示存在未登记的中文，需人工处理）</param>
        public static string ToAscii(string name, bool isFile, out bool changed, out bool ok)
        {
            changed = false;
            ok = true;
            if (IsAscii(name))
                return name;

            string stem = name;
            string ext = string.Empty;
            if (isFile)
            {
                ext = Path.GetExtension(name) ?? string.Empty;
                if (ext.Length > 0)
                    stem = name.Substring(0, name.Length - ext.Length);
            }

            if (Exact.TryGetValue(stem, out var mapped))
            {
                changed = true;
                return mapped + ext;
            }

            // 通用后缀规则：<角色>行走 / <角色>站姿 / <角色>站立 / <角色>经典
            if (stem.EndsWith("行走")) { changed = true; return "walk" + ext; }
            if (stem.EndsWith("站姿")) { changed = true; return "idle" + ext; }
            if (stem.EndsWith("站立")) { changed = true; return "idle" + ext; }
            if (stem.EndsWith("经典")) { changed = true; return "classic" + ext; }

            ok = false;
            return name;
        }

        public static bool IsAscii(string s)
        {
            foreach (var c in s)
                if (c > 127) return false;
            return true;
        }
    }
}
