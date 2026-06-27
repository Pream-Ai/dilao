using System;
using JetBrains.Annotations;
using UnityEngine;
using XLua;
using System.IO;
[XLua.LuaCallCSharp]
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public static Action<FurniData> onFurniDataSelect;

    private LuaEnv globalLuaEnv;

    /// <summary>
    /// 格式:
    /// --name
    /// --texture
    /// --income
    /// </summary>
    public GameObject furniWindow;
    public GameObject npcWindow;
    private void Awake()
    {
        instance = this;
        InitLuaEnviroment();
    }
    void Start()
    {
     //注册ui方法
        //事件1：点击跳出家具属性面板
        Click.instance.RegisterAction(0, (args) =>
        {
            FurniData data  = (FurniData)args[0];
            this.OpenFurniWindow(data);
        });
        //事件2：点击跳出NPC属性面板
        Click.instance.RegisterAction(1, (args) =>
        {
            NpcData data = (NpcData)args[0];
            this.OpenNpcWindow(data);
        });
    }
    void Update()
    {
        if (globalLuaEnv != null)
        {
            globalLuaEnv.Tick();  
        }
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
    public LuaEnv GetEnv()=> globalLuaEnv;

    public void selectFurni(int furniID)
    {
        Click.instance.isPreview = true;
        onFurniDataSelect?.Invoke(furniManager.instance.furniDataList[furniID]);
    }
    public void OpenFurniWindow(FurniData data)
    {
        Debug.Log(data.name);
        //if (furniWindow.activeSelf) return;//如果面板已经打开了就不重复打开了
        //furniWindow.SetActive(true);
    }
    public void OpenNpcWindow(NpcData data)
    {
        Debug.Log(data.name);
    }

    public void OnDestroy()
    {
        if (globalLuaEnv != null)
        {
            globalLuaEnv.DoString("mainPanel.OnDestroy()");
            globalLuaEnv.Dispose();
            globalLuaEnv = null;
        }
    }
}
