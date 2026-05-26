using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class buildSystem : MonoBehaviour
{
    public static buildSystem instance;
    public int mapWidth = 20;
    public int mapHight = 14;
    //false为墙
    public Dictionary<Vector2Int, bool> gridData = new Dictionary<Vector2Int, bool>();//建筑地图bool表
    public Dictionary<Vector2Int, bool> naviData = new Dictionary<Vector2Int, bool>();//寻路地图bool表
    public FurniData furniBeSelect;
    public GameObject previewShadow;
    private void Awake()
    {
        instance = this;
        UIManager.onFurniDataSelect += (FurniData furni) => furniBeSelect = furni;
    }
    private void Start()
    {
        initMap();
        previewShadow = new GameObject();
        previewShadow.AddComponent<SpriteRenderer>();
        previewShadow.GetComponent<SpriteRenderer>().sortingLayerName = "sort";
        previewShadow.GetComponent<SpriteRenderer>().sortingOrder = 1000;
    }
    /// <summary>
    /// 初始化建筑地图
    /// </summary>
    public void initMap()
    {
        gridData.Clear();
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHight ; j++)
            {
                gridData.Add(new Vector2Int(i, j), true);
            }
        }
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHight; j++)
            {
                naviData.Add(new Vector2Int(i, j), true);
            }
        }
    }
    /// <summary>
    /// 建造家具
    /// </summary>
    /// <param name="furniData"></param>
    /// <param name="setPos"></param>
    public void buildFurni(Vector2Int setPos)
    {
        Vector2Int build_size = furniBeSelect.buildSize;
        Vector2Int navi_size = furniBeSelect.naviSize;
        if (build_size.x <= 0) build_size.x = 1;
        if (build_size.y <= 0) build_size.y = 1;

        if (canBuildFurni(build_size, setPos))
        {
            for (int i = setPos.x; i < setPos.x + build_size.x; i++)
            {
                for (int j = setPos.y; j < setPos.y + build_size.y; j++)
                {
                    gridData[new Vector2Int(i, j)] = false;
                }
            }
            for (int i= setPos.x;i<setPos.x+navi_size.x;i++)
            {
                for (int j=setPos.y;j<setPos.y+navi_size.y;j++)
                {
                    naviData[new Vector2Int(i, j)] = false;
                }
            }
            GameObject furniInstance = Instantiate(
                furniBeSelect.prefab
                , new Vector3(setPos.x, setPos.y, 0)
                , Quaternion.identity
                , GameManager.instance.furniParent
                );
            furniController controller = furniInstance.GetComponent<furniController>();
            controller.setPos = setPos;
            furniManager.instance.furniList.Add(controller);
            Click.instance.isPreview = false;
            Debug.Log($"建造成功：{furniBeSelect.furnitureName} 在 {setPos}");
            //发布事件，所有寻路中的对象重新制定寻路路线
        }
        else
        {
            Debug.Log("无法在该地皮上建造");
            var erroBuild = DOTween.Sequence();
            erroBuild.Append(
                previewShadow.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0.5f, 0.5f, 0.5f), 0.25f)
                );
            erroBuild.Join(previewShadow.transform.DOShakePosition(
                duration: 0.5f,                         // 摇晃持续时间
                strength: new Vector3(0.05f, 0.05f, 0f),// X 轴强度大，Y 轴强度小
                vibrato: 10,                            // 震动次数
                randomness: 90f,                        // 随机方向角度
                snapping: false,                        // 不对齐像素
                fadeOut: true                           // 慢慢停止
            ));
            erroBuild.Append(
                previewShadow.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 0.5f), 0.25f)
                );
        }
    }
    bool canBuildFurni(Vector2Int size, Vector2Int setPos)
    {
        if (setPos.y > mapHight - 2) return false;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector2Int checkPos = new Vector2Int(setPos.x + i, setPos.y + j);
                if (!gridData.TryGetValue(checkPos, out bool isFree) || !isFree)
                {
                    return false;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 预览虚影
    /// </summary>
    /// <param name="furni"></param>
    public void UpdatePreview(bool isPreview)
    {
        if (isPreview)
        {
            previewShadow.SetActive(true);
            Vector2Int? previewPos =Click.instance.GetGridPosUnderMouse();
            if (previewPos.HasValue)
            {
                previewShadow.transform.position = new Vector3(previewPos.Value.x, previewPos.Value.y, 0);
                if (furniBeSelect != null && furniBeSelect.prefab != null)
                {
                    previewShadow.GetComponent<SpriteRenderer>().sprite = furniBeSelect.prefab.GetComponent<SpriteRenderer>().sprite;
                }
                previewShadow.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            previewShadow.GetComponent<SpriteRenderer>().sprite = null;
            previewShadow.SetActive(false);
        }
    }
    public bool[,] getWall(furniController targetFurni)
    {
        bool[,] wall = new bool[mapWidth, mapHight];
        //初始化墙
        for (int i = 0; i < wall.GetLength(0); i++)
        {
            for (int j = 0; j < wall.GetLength(1); j++)
            {
                wall[i, j] = false;
            }
        }
        //抠出已占用地皮
        foreach (KeyValuePair<Vector2Int,bool>kvp in naviData)
        {
            if (kvp.Value == false) wall[kvp.Key.x, kvp.Key.y] = true;
        }
        //重置目标地皮，防止与寻路算法冲突
        for (int i = targetFurni.setPos.x; i < targetFurni.setPos.x + targetFurni.naviSize.x; i++)
        {
            for (int j = targetFurni.setPos.y; j < targetFurni.setPos.y + targetFurni.naviSize.y; j++)
            {
                wall[i, j] = false;
            }
        }
        return wall;
    }
}
