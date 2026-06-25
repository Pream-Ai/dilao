 using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class buildSystem : MonoBehaviour
{
    public static buildSystem instance;
    public int mapWidth = 20;
    public int mapHight = 14;
    //false为墙
    public Dictionary<Vector2Int, bool> gridData = new Dictionary<Vector2Int, bool>();//建筑地图bool表
    public Dictionary<Vector2Int, bool> naviData = new Dictionary<Vector2Int, bool>();//寻路地图bool表
    public FurniData furniBeSelect;
    [Header("建造预览")]
    public GameObject previewShadow;
    public GameObject greenGrid;
    public GameObject redGrid;
    public Transform greenGridParent;
    public Transform redGridParent;
    public bool showHot = false;
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!showHot) showHotMap();
            else initLawPreview();
            showHot = !showHot;
        }
    }
    /// <summary>
    /// 初始化建筑地图
    /// </summary>
    public void initMap()
    {
        gridData.Clear();
        for (int i = 0; i < mapWidth; i++)
        {
            gridData.Add(new Vector2Int(i, 0), false);
            for (int j = 1; j < mapHight ; j++)
            {
                gridData.Add(new Vector2Int(i, j), true);
            }
        }
        for (int i = 0; i < mapWidth; i++)
        {
            naviData.Add(new Vector2Int(i,mapHight-1),false);
            for (int j = 0; j < mapHight-1; j++)
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
            Click.instance.isPreview = false;
            Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
            previewShadow.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("无法在该地皮上建造");
            var erroBuild = DOTween.Sequence();
            erroBuild.Append(
                previewShadow.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0.5f, 0.5f, 0.8f), 0.25f)
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
                previewShadow.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 0.8f), 0.25f)
                );
        }
    }
    /// <summary>
    /// 返回是否可以在该位置建造家具
    /// </summary>
    /// <param name="size"></param>
    /// <param name="setPos"></param>
    /// <returns></returns>
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
            Camera.main.GetComponent<Physics2DRaycaster>().enabled = false;
            previewShadow.SetActive(true);
            Vector2Int? previewPos =Click.instance.GetGridPosUnderMouse();
            if (previewPos.HasValue)
            {
                previewShadow.transform.position = new Vector3(previewPos.Value.x, previewPos.Value.y, 0);
                if (furniBeSelect != null && furniBeSelect.prefab != null)
                {
                    previewShadow.GetComponent<SpriteRenderer>().sprite = furniBeSelect.prefab.GetComponent<SpriteRenderer>().sprite;
                }
                previewShadow.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.8f);
            }
        }
        else
        {
            previewShadow.GetComponent<SpriteRenderer>().sprite = null;
            previewShadow.SetActive(false);
            initLawPreview();
        }
    }
    /// <summary>
    /// 预览合法地板
    /// </summary>
    /// <param name="currentPos"></param>
    public void initLawPreview()
    {
        for (int i = 0; i < greenGridParent.childCount; i++)
        {
            greenGridParent.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < redGridParent.childCount; i++)
        {
            redGridParent.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void UpdateLawPreview(Vector2Int currentPos)
    {
        initLawPreview();
        Vector2Int size = furniBeSelect.buildSize;
        int greenIndex = 0;
        int redIndex = 0;
        for (int i=0;i<size.x;i++)
        {
            for (int j=0;j<size.y;j++)
            {
                var targetGrid = new Vector2Int(currentPos.x + i, currentPos.y + j);
                if(targetGrid.y>mapHight-2)targetGrid.y = mapHight-2;
                if (gridData.TryGetValue(targetGrid, out var isFree) && isFree)
                {
                    if (greenIndex<greenGridParent.childCount)
                    {
                        Transform pooled = greenGridParent.GetChild(greenIndex++);
                        pooled.position = new Vector3(targetGrid.x, targetGrid.y);
                        pooled.gameObject.SetActive(true);
                    }
                    else
                    {
                        Instantiate(
                            greenGrid
                            ,new Vector3(targetGrid.x,targetGrid.y)
                            ,Quaternion.identity,greenGridParent
                            );
                    }
                }
                else
                {
                    if (redIndex < redGridParent.childCount)
                    {
                        Transform pooled = redGridParent.GetChild(redIndex++);
                        pooled.position = new Vector3(targetGrid.x, targetGrid.y);
                        pooled.gameObject.SetActive(true);
                    }
                    else
                    {
                        Instantiate(
                            redGrid
                            ,new Vector3(size.x, size.y)
                            ,Quaternion.identity
                            ,redGridParent
                            );
                    }
                }
            }
        }
    }
    public void showHotMap()
    {
        initLawPreview();
        Vector2Int size = new Vector2Int(mapWidth,mapHight);
        int greenIndex = 0;
        int redIndex = 0;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y-1; j++)
            {
                if (naviData[new Vector2Int(i,j)])//可走
                {
                    if (greenIndex < greenGridParent.childCount)
                    {
                        greenGridParent.GetChild(greenIndex).gameObject.SetActive(true);
                        greenGridParent.GetChild(greenIndex).position = new Vector3(i, j);
                    }
                    else
                    {
                        Instantiate(
                            greenGrid
                            , new Vector3(i, j)
                            , Quaternion.identity
                            , greenGridParent
                            );
                    }
                    greenIndex++;
                }
                else
                {
                    if (redIndex < redGridParent.childCount)
                    {
                        redGridParent.GetChild(redIndex).gameObject.SetActive(true);
                        redGridParent.GetChild(redIndex).position = new Vector3(i,j);
                    }
                    else
                    {
                        Instantiate(
                           redGrid
                           , new Vector3(i, j)
                           , Quaternion.identity
                           , redGridParent
                           );
                    }
                    redIndex++;
                }
            }
        }
    }
    /// <summary>
    /// 获取墙地图，false为可通行，true为墙
    /// </summary>
    /// <param name="targetFurni"></param>
    /// <returns></returns>
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
        return wall;
    }
    /// <summary>
    /// 拆除家具，重置地皮
    /// </summary>
    /// <param name="furni"></param>
    public void removeFurni(furniController furni)
    {
        Vector2Int setPos = furni.setPos;
        Vector2Int build_size = furni.buildSize;
        Vector2Int navi_size = furni.naviSize;
        for (int i = setPos.x; i < setPos.x + build_size.x; i++)
        {
            for (int j = setPos.y; j < setPos.y + build_size.y; j++)
            {
                gridData[new Vector2Int(i, j)] = true;
            }
        }
        for (int i = setPos.x; i < setPos.x + navi_size.x; i++)
        {
            for (int j = setPos.y; j < setPos.y + navi_size.y; j++)
            {
                naviData[new Vector2Int(i, j)] = true;
            }
        }
    }
}
