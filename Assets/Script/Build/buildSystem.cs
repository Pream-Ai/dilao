using System.Collections.Generic;
using UnityEngine;

public class buildSystem : MonoBehaviour
{
    public static buildSystem instance;
    public int mapWidth = 22;
    public int mapHight = 14;
    public Dictionary<Vector2Int, bool> gridData = new Dictionary<Vector2Int, bool>();
    public FurniData furniBeSelect;
    GameObject previewShadow;
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
    }
    /// <summary>
    /// 初始化建筑地图
    /// </summary>
    public void initMap()
    {
        gridData.Clear();
        for (int i = (int)(-mapWidth * 0.5f)-1; i < mapWidth * 0.5f+1; i++)
        {
            for (int j = (int)(-mapHight * 0.5f)-1; j < mapHight * 0.5f+2; j++)
            {
                gridData.Add(new Vector2Int(i, j), true);
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
        Vector2Int size = furniBeSelect.size;
        if (size.x <= 0) size.x = 1;
        if (size.y <= 0) size.y = 1;

        if (canBuildFurni(size, setPos))
        {
            for (int i = setPos.x; i < setPos.x + size.x; i++)
            {
                for (int j = setPos.y; j < setPos.y + size.y; j++)
                {
                    gridData[new Vector2Int(i, j)] = false;
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
        }
    }
    bool canBuildFurni(Vector2Int size, Vector2Int setPos)
    {
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
}
