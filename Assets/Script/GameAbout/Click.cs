using Unity.VisualScripting;
using UnityEngine;

public class Click : MonoBehaviour
{
    public furniController furni;
    GameObject previewShadow;
    bool isPreview = false;
    private void Start()
    {
        previewShadow = new GameObject();
        previewShadow.AddComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int? gridPos = GetGridPosUnderMouse();

            if (gridPos.HasValue)
            {
                Debug.Log($"点击格子: {gridPos.Value}");
                buildSystem.instance.buildFurni(furni,gridPos.Value);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) isPreview = !isPreview;
        buildFurni(furni);
    }
     
    Vector2Int? GetGridPosUnderMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Vector3 hitPoint = hit.point;
            return WorldToGrid(hitPoint);
        }

        return null;
    }
        
    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float gx = worldPos.x /1;
        float gy = worldPos.y /1; 

        int x = Mathf.FloorToInt(gx);
        int y = Mathf.FloorToInt(gy);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 预览虚影
    /// </summary>
    /// <param name="furni"></param>
    void buildFurni(furniController furni)
    {
        if (isPreview)
        {
            previewShadow.SetActive(true);
            Vector2Int? previewPos = GetGridPosUnderMouse();
            if (previewPos.HasValue)
            {
                previewShadow.transform.position = new Vector3(previewPos.Value.x, previewPos.Value.y, 0);
                previewShadow.GetComponent<SpriteRenderer>().sprite = furni.prefab.GetComponent<SpriteRenderer>().sprite;
                previewShadow.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.5f);
            }
        }
        else
        {
            previewShadow.SetActive(false);
        }
    }
}