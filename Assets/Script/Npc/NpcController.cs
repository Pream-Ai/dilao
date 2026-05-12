using System.Collections;
using System.Collections.Generic;
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
}
