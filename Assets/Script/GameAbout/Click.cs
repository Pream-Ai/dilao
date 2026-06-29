using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
public class Click : Singleton<Click> 
{
    public bool isPreview = false;
    private Dictionary<int, Action<object[]>> methodDict = new Dictionary<int, Action<object[]>>();
    protected override void Awake()
    {
        base.Awake();
    }
    private Vector2Int lastPreviewPos;
    private Vector2Int? mousePos;
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //鼠标点击触发建造预览
            if (isPreview)
            {
                Vector2Int? gridPos = GetGridPosUnderMouse();

                if (gridPos.HasValue && !EventSystem.current.IsPointerOverGameObject())//防止点击ui
                {
                    buildSystem.instance.buildFurni(gridPos.Value);
                    buildSystem.instance.initLawPreview();
                }
            }
        }
        if (isPreview)
        {
            mousePos = GetGridPosUnderMouse();
            if (lastPreviewPos!=mousePos&&mousePos.HasValue)
            {
                buildSystem.instance.UpdateLawPreview(mousePos.Value);
                lastPreviewPos = mousePos.Value;
            }
            if (Input.GetMouseButton(1)) isPreview = false;
            buildSystem.instance.UpdatePreview(isPreview);
        }
    }
    public Vector2Int? GetGridPosUnderMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Vector3 hitPoint = hit.point;
            var result = WorldToGrid(hitPoint);
            var buildsystem = buildSystem.instance;
            //限位
            if (result.x > buildsystem.mapWidth-buildsystem.furniBeSelect.buildSize.x) result.x = buildsystem.mapWidth-buildsystem.furniBeSelect.buildSize.x;
            if (result.x < 0) result.x = 0;
            if (result.y > buildsystem.mapHight-2) result.y = buildsystem.mapHight-2;//背景墙高2
            if (result.y < 0) result.y = 0;
            return result;
        }
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
    public void RegisterAction(int key, Action<object[]> action)
    {
        methodDict[key] = action;
    }
    public void Execute(int key,params object[] args)
    {
        if(methodDict.TryGetValue(key,out var action))
        {
            action?.Invoke(args);
        }
    }
}
