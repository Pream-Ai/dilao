using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class furniController : MonoBehaviour
{
    public FurniData furnidata;
    public int ID;
    public string furniName;
    public GameObject prefab;
    public int buildCost;
    public int baseIncome;
    public Vector2Int size;
    public Vector2Int setPos;
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
        size = furnidata.size;
        setPos = Vector2Int.zero;
    }
}
