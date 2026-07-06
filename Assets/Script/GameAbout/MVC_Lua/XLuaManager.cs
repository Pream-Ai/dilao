using UnityEngine;
using XLua;
using System.IO;
public class XLuaManager : Singleton<XLuaManager>
{
    private LuaEnv globalLuaEnv;

    protected override void Awake()
    {
        base.Awake();
        InitLuaEnviroment();
    }
    private void Start()
    {
        
    }
    private void Update()
    {

        if (globalLuaEnv != null)
        {
            globalLuaEnv.Tick();
        }
    }
    public void EnterGame()
    {

    }
    private void InitLuaEnviroment()
    {
        if (globalLuaEnv == null)
        {
            globalLuaEnv = new LuaEnv();
        }
        globalLuaEnv.AddLoader(CustomLuaLoader);
    }
    private byte[] CustomLuaLoader(ref string filePath)
    {
        string path = "";
#if UNITY_EDITOR
        path = Path.Combine(Application.streamingAssetsPath, "Lua", filePath + ".lua");
#else
        path = Path.Combine(Application.persistentDataPath, "Lua", filePath + ".lua");
#endif
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        Debug.LogWarning($"【LuaLoader】物理路径未找到脚本: {path}");
        return null; // 返回 null 意味着这个加载器没捞到，虚拟机会去下一个加载器捞
    }
    public LuaEnv GetEnv() => globalLuaEnv;

    public void OnDestroy()
    {
        if (globalLuaEnv != null)
        {
            globalLuaEnv.Dispose();
            globalLuaEnv = null;
        }
    }
}
