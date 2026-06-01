using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class furniController : MonoBehaviour
{
    public int index;

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
    void Awake()
    {
        initFurni();
    }
    void Update()
    {
        
    }
    void initFurni()
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
    public void initSort()=>GetComponent<SpriteRenderer>().sortingOrder=100-(int)transform.position.y+(buildSize.y-naviSize.y);

    public void destory()
    {
        furniManager.instance.UnregisterFurni(buildId);
    }
}
