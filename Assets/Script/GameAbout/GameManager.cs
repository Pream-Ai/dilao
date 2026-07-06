using UnityEngine;
using XLua;
using System.IO;
[XLua.LuaCallCSharp]
public class GameManager : Singleton<GameManager>
{
    [Header("父容器")]
    public Transform furniParent;
    public Transform furniPool;
    public Transform npcParent;
    public Transform npcPool;
    [Header("商店参数")]
    public int TotalWorth;
    public int TotalFurni;
    public int TotalNpc;
    [Header("日期")]
    public int Year;
    public int Month;
    public int Day;


    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        
    }
    void Update()
    {
    }
    void GameLanuch()
    {

    }
    
}
