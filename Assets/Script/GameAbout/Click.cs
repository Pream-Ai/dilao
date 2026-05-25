using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
public class Click : MonoBehaviour
{
    public static Click instance;
    public FurniData furniData;
    public bool isPreview = false;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //鼠标点击触发建造预览
            if (isPreview)
            {
                Vector2Int? gridPos = GetGridPosUnderMouse();

                if (gridPos.HasValue && !EventSystem.current.IsPointerOverGameObject())
                {
                    buildSystem.instance.buildFurni(gridPos.Value);
                }
                else
                {
                    Debug.Log("无法在该地皮上建造");
                }
            }
            //鼠标点击触发面板弹出
        }
        if (isPreview&&Input.GetMouseButton(1)) isPreview = false;
        buildSystem.instance.UpdatePreview(isPreview);
    }
    public Vector2Int? GetGridPosUnderMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Vector3 hitPoint = hit.point;
            return WorldToGrid(hitPoint);
        }//

        return null;
    }
    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float gx = worldPos.x / 1;
        float gy = worldPos.y / 1;

        int x = Mathf.FloorToInt(gx);
        int y = Mathf.FloorToInt(gy);

        return new Vector2Int(x, y);
    }
}
