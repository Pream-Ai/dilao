using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class FurniData : ScriptableObject
{
    public int ID;
    public string furnitureName;//家具名
    public GameObject prefab;//预制体
    public int buildCost;//建造成本
    public int baseIncome;//基础收入
    public Vector2Int buildSize;//占地尺寸
    public Vector2Int setPos;//摆放坐标
    public Vector2Int naviSize;//寻路尺寸
    public Vector2Int serviveOffset;//服务点定位偏移值
}
