using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (SpriteRenderer),typeof(ClickMarker))]
public class furniController : MonoBehaviour
{
    [Header("data")]
    public FurniData furnidata;
    public int ID;
    public string furniName;
    public GameObject prefab;
    public int buildCost;
    public int baseIncome;
    public int maxCapacity;
    public Vector2Int buildSize;
    public Vector2Int setPos;
    public Vector2Int naviSize;
    public Vector2Int offset;
    public int buildId;
    [Header("state")]
    public bool beUsing = false;
    protected virtual void Awake()
    {
        initFurni();
    }
    protected virtual void Update()
    {
        
    }
    protected virtual void initFurni()
    {
        ID = furnidata.ID;
        furniName = furnidata.furnitureName;
        prefab = furnidata.prefab;
        buildCost = furnidata.buildCost;
        baseIncome = furnidata.baseIncome;
        maxCapacity = furnidata.maxCapacity;
        buildSize = furnidata.buildSize;
        setPos = Vector2Int.zero;
        naviSize = furnidata.naviSize;
        offset = furnidata.serviveOffset;
        buildId = furniManager.instance.RegisterFurni(this);
        transform.GetComponent<ClickMarker>().Init(0,this.furnidata );
        initSort();
    }
    public virtual void initSort()=>GetComponent<SpriteRenderer>().sortingOrder=100-(int)transform.position.y+(buildSize.y-naviSize.y);
    public virtual void remove()
    {
        furniManager.instance.UnregisterFurni(buildId);
        buildSystem.instance.removeFurni(this);
        GameManager.instance.TotalFurni--;
        EnterPool();
    }
    public virtual void GainReward()=> GameManager.instance.TotalWorth += baseIncome;

    protected virtual void EnterPool()
    {
        gameObject.SetActive(false);
    }

    ////------------------∂‡Ã¨---------------------
    public virtual void OnInteract()
    {
        beUsing = true;
    }
    public virtual void EndInteract()
    {
        beUsing = false;
    }
}
