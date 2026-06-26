using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
public class luaUIBehaviour : MonoBehaviour
{
    [Tooltip("对应的lua脚本名")]
    public string luaScriptName;
    [Header("UI组件绑定")]
    public Button[] buttons;
    public TextMeshProUGUI[] tmps;
    public GameObject[] gameobjects;

    private LuaEnv luaEnv;
    private LuaTable scriptEnv;

    private Action<LuaTable> luaAwake;
    private Action<LuaTable> luaStart;
    private Action<LuaTable> luaOnDestory;

    private void Awake()
    {
        // 1. 获取全局唯一的虚拟机指针
        luaEnv = UIManager.instance.GetEnv();
        if (luaEnv == null)
        {
            Debug.LogError("UIManager.instance.GetEnv() 返回了 null，请检查 UIManager 是否率先初始化。");
            return;
        }
        // 2. 初始化当前界面的独立数据盒子（Table）
        scriptEnv = luaEnv.NewTable();
        using (LuaTable meta = luaEnv.NewTable())
        {
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
        }
        // 3. 将当前 C# 实例指针传给 Lua 层的 env.Self
        scriptEnv.Set("Self", this);
        // 4. 🎯 核心修改：利用 require 替代 Resources.Load
        // 这一步执行时，xLua 会自动调用你在 UIManager 里注册的 CustomLuaLoader 去检索 Assets/LuaScripts/ 下的物理文件
        // 并将文件返回的母表存入 C# 变量 resultTable 中
        object[] results = luaEnv.DoString($"return require('{luaScriptName}')", luaScriptName, scriptEnv);
        if (results != null && results.Length > 0 && results[0] is LuaTable resultTable)
        {
            // 5. 🎯 从返回的母表中物理抓取生命周期函数指针
            resultTable.Get("Awake", out luaAwake);
            resultTable.Get("Start", out luaStart);
            resultTable.Get("OnDestroy", out luaOnDestory); 
            // 释放临时拿到的母表引用
            resultTable.Dispose();
        }
        else
        {
            Debug.LogError($"无法加载并解析 Lua 脚本: {luaScriptName}，请检查物理路径是否存在或脚本是否 return 母表。");
            return;
        }
        // 6. 执行 Lua 端的 Awake
        if (luaAwake != null)
        {
            luaAwake(scriptEnv);
        }
    }
    void Start()
    {
        if (luaStart != null) luaStart(scriptEnv);
    }

    private void OnDestroy()
    {
        if (luaOnDestory != null) luaOnDestory(scriptEnv);
        luaAwake = null;
        luaStart = null;
        luaOnDestory = null;
        if (scriptEnv != null)
        {
            scriptEnv.Dispose();
            scriptEnv = null;
        }
    }
}
