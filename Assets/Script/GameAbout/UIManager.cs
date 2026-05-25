using System;
using System.Collections;
using System.Collections.Generic;
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
}
