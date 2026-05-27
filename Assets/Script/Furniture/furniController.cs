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
    public Vector2Int buildSize;
    public Vector2Int setPos;
    public Vector2Int naviSize;
    public Vector2Int offset;

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
        buildSize = furnidata.buildSize;
        setPos = Vector2Int.zero;
        naviSize = furnidata.naviSize;
        offset = furnidata.serviveOffset;
        initSort();
    }
    public void initSort()
    {
        transform.GetComponent<SpriteRenderer>().sortingOrder =100-(int)transform.position.y;
    }
    public void destory()
    {
        //÷ÿ≈≈À≥–Ú
        furniManager.instance.furniList.RemoveAt(index);
        for (int i=index;i<furniManager.instance.furniList.Count;i++)
        {
            furniManager.instance.furniList[i].index--;
        }
        for (int i=setPos.x;i<setPos.x+buildSize.x;i++)
        {
            for (int j=setPos.y;j<setPos.y+buildSize.y;j++)
            {
                
            }
        }
    }
}
