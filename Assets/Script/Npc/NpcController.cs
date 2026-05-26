using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// npc终端三层解耦框架
/// ---npcBrain决策层
/// ---npcAction行为层
/// ---npcView表现层
/// </summary>
public class NpcController : MonoBehaviour
{
    [Header("属性数据")]
    public NpcData data;
    public FSM fsm = new FSM();
    public int ID;
    public string name;
    public GameObject prefab;
    public int level;
    public int money;

    [Header("状态数据")]
    private bool hasChoseFurni = false;
    private void Start()
    {
        initNpcData();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("开始寻路");
            if (furniManager.instance.furniList.Count<=0)
            {
                Debug.Log("无目标");
                return;
            }
            navigationMove(furniManager.instance.furniList[0]);
        }
    }
    public void initNpcData()
    {
        ID = data.ID;
        name = data.name;
        prefab = data.prefab;
        level = data.level;
        money = data.money;
    }
    public void cleanNpcData()
    {
        ID = -1;
        name = null;
        prefab = null;
        level = -1;
        money = -1;
    }

    #region 决策系统
    //生成购物倾向
    #endregion

    #region 行为系统
    int manhattan(int x1, int y1, int x2, int y2)
    {
        int result = 0;
        result = math.abs(x2 - x1) + math.abs(y2 - y1);
        return result;
    }
    bool isValid(int x, int y, int width, int height, bool[,] walls)
    {
        return x >= 0 && x < width && y >= 0 && y < height && !walls[x, y];
    }
    public List<Vector2Int> AStar(bool[,] walls, Vector2Int start, Vector2Int end)
    {
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);
        Vector2Int[,] cameFrom = new Vector2Int[width, height];
        int[,] gscore = new int[width, height];
        int[,] fscore = new int[width, height];
        //初始化容器
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gscore[i, j] = int.MaxValue;
                fscore[i, j] = int.MaxValue;
                cameFrom[i, j] = new Vector2Int(-1, -1);
            }
        }
        gscore[start.x, start.y] = 0;
        fscore[start.x, start.y] = manhattan(start.x, end.x, start.y, end.y);

        List<Vector2Int> openList = new List<Vector2Int> { start };
        Vector2Int[] dirs = {
            new Vector2Int(0,1),new Vector2Int(0,-1),
            new Vector2Int(1,0),new Vector2Int(-1,0)
        };

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];
            foreach (var node in openList)
            {
                if (fscore[node.x, node.y] < fscore[current.x, current.y])
                {
                    current = node;
                }
            }
            if (current == end+new Vector2Int(0,-1))
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int node = end;
                while (node.x != -1)
                {
                    path.Add(node);
                    node = cameFrom[node.x, node.y];
                }
                path.Reverse();
                return path;
            }
            openList.Remove(current);

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (!isValid(neighbor.x, neighbor.y, width, height, walls))
                    continue;

                int tentativeG = gscore[current.x, current.y] + 1;
                if (tentativeG < gscore[neighbor.x, neighbor.y])
                {
                    cameFrom[neighbor.x, neighbor.y] = current;
                    gscore[neighbor.x, neighbor.y] = tentativeG;
                    fscore[neighbor.x, neighbor.y] = tentativeG + manhattan(neighbor.x, neighbor.y, end.x, end.y);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        Debug.Log("无路可走");
        return null;
    }
    public void navigationMove(furniController targetFurni)
    {
        var path = AStar(buildSystem.instance.getWall(targetFurni), npcPosToGrid(),targetFurni.setPos+targetFurni.offset);
        Sequence moveSeq = DOTween.Sequence();
        for (int i=1;i<path.Count;i++)
        {
            moveSeq.Append(transform.DOMove(new Vector3(path[i].x + 0.5f, path[i].y + 0.5f), 1f));
            moveSeq.AppendCallback(()=> sortLayer());
        }
        moveSeq.SetEase(Ease.Linear);
        moveSeq.Play();
    }
    Vector2Int npcPosToGrid()
    {
        return new Vector2Int((int)transform.position.x/1,(int)transform.position.y/1);
    }
    #endregion

    #region 表现层
    //构造透视效果
    void sortLayer()
    {
        transform.GetComponent<SpriteRenderer>().sortingOrder =100-(int)transform.position.y;
    }

    #endregion
}
