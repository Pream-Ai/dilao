using System;
using JetBrains.Annotations;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public static Action<FurniData> onFurniDataSelect;
    private void Awake()
    {
        instance = this;
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
    // Update is called once per frame
    void Update()
    {
        
    }
    public void selectFurni(int furniID)
    {
        Debug.Log($"选择了{furniID}号家具");
        Click.instance.isPreview = true;
        onFurniDataSelect?.Invoke(furniManager.instance.furniDataList[furniID]);
    }

    public void OpenFurniWindow(FurniData data)
    {
        Debug.Log(data.name);
    }
    public void OpenNpcWindow(NpcData data)
    {
        Debug.Log(data.name);
    }
}
