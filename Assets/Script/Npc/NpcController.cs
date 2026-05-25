using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// npc终端三层解耦框架
/// ---npcBrain决策层
/// ---npcAction行为层
/// ---npcVisual表现层
/// </summary>
public class NpcController : MonoBehaviour
{
    public NpcData data=new NpcData();
    public FSM fsm = new FSM();
    public int ID;
    public string name;
    public GameObject prefab;
    public int level;
    public int money;
    private void Start()
    {
        initNpcData();
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


    #region 寻路系统
    int manhattan(int x1,int y1,int x2,int y2)
    {
        int result = 0;
        result = (x2 - x1) + (y2 - y1);
        return result;
    }
    bool isValid(int x,int y,int width,int height, bool[,] walls)
    {
        return x > 0 && x < width && y > 0 && y < height && !walls[x, y];
    }
    public List<Vector2Int> AStar(bool[,] walls,Vector2Int start,Vector2Int end)
    {
        int width = walls.GetLength(0);
        int height = walls.GetLength(1);
        Vector2Int[,] cameFrom = new Vector2Int[width, height];
        int[,] gscore = new int[width,height];
        int[,] fscore = new int[width,height];
        //初始化容器
        for (int i=0;i<width;i++)
        {
            for(int j = 0; j < height; j++)
            {
                gscore[i, j] = int.MaxValue;
                fscore[i, j] = int.MaxValue;
                cameFrom[i, j] = new Vector2Int(-1,-1);
            }
        }
        gscore[start.x, start.y] = 0;
        fscore[start.x, start.y] = manhattan(start.x,end.x,start.y,end.y);

        List<Vector2Int> openList = new List<Vector2Int> { start };
        Vector2Int[] dirs = {
            new Vector2Int(0,1),new Vector2Int(0,-1),
            new Vector2Int(1,0),new Vector2Int(-1,0)
        };

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];
            foreach(var node in openList)
            {
                if (fscore[node.x, node.y] < fscore[current.x,current.y])
                {
                    current = node;
                }
            }
            if (current == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int node = end;
                while (node.x!=-1)
                {
                    path.Add(node);
                    node = cameFrom[node.x,node.y];
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
                    fscore[neighbor.x,neighbor.y] = tentativeG + manhattan(neighbor.x,neighbor.y,end.x,end.y);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return null;
    }
    #endregion
}
