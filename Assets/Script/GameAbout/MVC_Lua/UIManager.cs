using System;
using JetBrains.Annotations;
using UnityEngine;
using XLua;
using System.IO;
[XLua.LuaCallCSharp]
public class UIManager : Singleton<UIManager>
{
    public static Action<FurniData> onFurniDataSelect;
    /// <summary>
    /// 格式:
    /// --name
    /// --texture
    /// --income
    /// </summary>
    public GameObject furniWindow;
    public GameObject npcWindow;
    protected override void Awake()
    {
        base.Awake();
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

    }

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
}
