using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class FurniData : ScriptableObject
{
    public int ID;
    public string furnitureName;//家具名
    public GameObject prefab;//预制体
    public int buildCost;//建筑消耗
    public int baseIncome;//基础收入
    public Vector2Int buildSize;//占地面积
    public Vector2Int setPos;//建造坐标
    public Vector2Int naviSize;//
    public Vector2Int serviveOffset;//最终定位偏移值
}
