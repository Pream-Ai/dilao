using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Buffers;
public abstract class baseState
{
    protected NpcController owner;
    public baseState(NpcController npc) { this.owner = npc; }
    public virtual void OnEnter()
    {
        string stateName = this.GetType().Name;
        owner.currentState=stateName;
    }
    public abstract void OnExit();
    public abstract void TickPerFrame();
    public abstract void TickDecision();
}
public class FSM
{
    public baseState _currentState;
    public void stateChange(baseState targetState)
    {
        _currentState?.OnExit();
        _currentState = targetState;
        targetState?.OnEnter();
    }
}
/// <summary>
/// 状态中转站，站立转闲逛，移动
/// </summary>
public class IdleState : baseState
{
    public IdleState(NpcController npc) : base(npc) { }
    List<baseState> nextStatePool = new List<baseState>();
    public override void OnEnter()
    {
        base.OnEnter();
        nextStatePool.Clear();
        //Debug.Log("进入站立状态");
        owner.changeAnim(false);
        nextStatePool.Add(owner.idleState);
        nextStatePool.Add(owner.moveState);
        nextStatePool.Add(owner.wanderState);
    }

    public override void OnExit()
    {

    }

    public override void TickDecision()
    {
        owner.fsm.stateChange(nextStatePool[Random.Range(0, nextStatePool.Count)]);
    }

    public override void TickPerFrame()
    {

    }
}
public class WanderState : baseState
{
    public WanderState(NpcController npc) : base(npc) { }
    public Vector2Int[] AllDirs = new Vector2Int[]
    {
        new Vector2Int(-1,1),//左上
        new Vector2Int(0,1),//上
        new Vector2Int(1,1),//右上
         new Vector2Int(-1,0),//左
        new Vector2Int(1,0),//右
        new Vector2Int(-1,-1),//左下
        new Vector2Int(0,-1),//下
        new Vector2Int(1,-1)//右下
    };
    List<Vector2Int> targetPosPool = new List<Vector2Int>();
    Vector2Int target;
    Vector3 endPos;
    public override void OnEnter()
    {
        //Debug.Log("进入闲逛状态");
        base.OnEnter();
        owner.changeAnim(true);
        targetPosPool.Clear();
        owner.showEmo(6);
        for (int i = 0; i < 8; i++)
        {
            target = new Vector2Int((int)owner.transform.position.x / 1, (int)owner.transform.position.y / 1) + AllDirs[i];

            if (buildSystem.instance.naviData.ContainsKey(target)
                && buildSystem.instance.naviData[target])
            {
                targetPosPool.Add(target);
            }
        }
        if (targetPosPool.Count > 0)
        {
            var target = targetPosPool[Random.Range(0, targetPosPool.Count)];
            endPos = new Vector3(target.x + 0.5f, target.y + 0.5f);
        }
        owner.changeFlip(new Vector2Int((int)endPos.x, (int)endPos.y));
    }
    public override void OnExit()
    {
        //Debug.Log("离开闲逛状态");
    }
    public override void TickDecision()
    {

    }
    public override void TickPerFrame()
    {
        wander();
    }
    void wander()
    {
        owner.transform.position = Vector3.MoveTowards(owner.transform.position, endPos, 2 * Time.deltaTime);
        if (Vector3.Distance(owner.transform.position, endPos) < 0.12f)
        {
            owner.sortLayer();
            owner.fsm.stateChange(UnityEngine.Random.Range(0, 5) > 0 ? owner.idleState : owner.wanderState);
        }
    }
}
public class MoveState : baseState
{
    public MoveState(NpcController npc) : base(npc) { }
    private List<Vector2Int> path;
    private int currentIndex = 0;
    private float moveSpeed = 5f;
    public override void OnEnter()
    {
        base.OnEnter();
        if (owner.Decision())
        {
            path = owner.AStar();
            currentIndex = 0;
            //Debug.Log("进入寻路状态");
            owner.changeAnim(true);
            owner.changeFlip(owner.targetFurni.setPos);
            owner.showEmo(0);
        }
        else
        {
            owner.fsm.stateChange(owner.idleState);
        }
    }
    public override void OnExit()
    {

    }
    public override void TickDecision()
    {

    }
    public override void TickPerFrame()
    {
        move();
    }
    void move()
    {
        Vector3 targetPos = new Vector3(path[currentIndex].x + 0.5f, path[currentIndex].y + 0.5f);
        owner.transform.position = Vector3.MoveTowards(owner.transform.position, targetPos, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(owner.transform.position, targetPos) < 0.1f)
        {
            owner.sortLayer();
            currentIndex++;
            if (currentIndex >= path.Count)
            {
                //Debug.Log("到达目的地");
                owner.fsm.stateChange(owner.buyState);
            }
        }
    }
}
public class WaitState : baseState
{
    public WaitState(NpcController npc) : base(npc) { }
    float waitTime = Random.Range(1f, 3f);
    float timer = 0f;
    public override void OnEnter()
    {
        //Debug.Log("进入等待状态");
        base.OnEnter();
        owner.showEmo(4);

    }

    public override void OnExit()
    {
        //Debug.Log("离开等待状态");
    }

    public override void TickDecision()
    {
    }

    public override void TickPerFrame()
    {
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            var door = furniManager.instance.ExitDoor;
            owner.targetFurni = furniManager.instance.furniList[1];
            owner.fsm.stateChange(owner.moveState);
        }
    }
}
public class BuyState : baseState
{
    public BuyState(NpcController npc) : base(npc) { }
    float waitTime = 3;
    float timer = 0f;
    public override void OnEnter()
    {
        //Debug.Log("进入购物状态");
        base.OnEnter();
        owner.changeAnim(false);
        timer = 0f;
        owner.showEmo(2);
        owner.targetFurni.OnInteract();
    }
    public override void OnExit()
    {
        owner.showEmo(7);
        owner.targetFurni.EndInteract();
        //Debug.Log("离开购物状态");
    }
    public override void TickDecision()
    {
    }
    public override void TickPerFrame()
    {
        owner.buy();
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            var door = furniManager.instance.ExitDoor;
            NpcManager.instance.ReleaseSlot(owner.targetFurni);
            owner.money -= owner.targetFurni.baseIncome;
            if (owner.money<10)
            {
                owner.fsm.stateChange(owner.enterPool);
                return;
            }
            owner.fsm.stateChange(owner.wanderState);
        }
    }
}
public class EnterPool : baseState
{
    public EnterPool(NpcController npc) : base(npc) { }
    int currentIndex = 0;
    List<Vector2Int> path;
    bool hasArrive = false;
    public override void OnEnter()
    {
        //Debug.Log("进入对象池");
        base.OnEnter();
        owner.targetFurni = furniManager.instance.ExitDoor;
        path = owner.AStar();
    }

    public override void OnExit()
    {
    }
    public override void TickDecision()
    {
    }
    public override void TickPerFrame()
    {
        move();
    }
    void move()
    {
        if (!hasArrive)
        {
            Vector3 targetPos = new Vector3(path[currentIndex].x, path[currentIndex].y);
            owner.transform.position = Vector3.MoveTowards(owner.transform.position, targetPos, 5 * Time.deltaTime);
            if (Vector3.Distance(owner.transform.position, targetPos) < 0.1f)
            {
                owner.sortLayer();
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    hasArrive = true;
                }
            }
        }
        else
        {
            owner.transform.DOMove(owner.transform.position + new Vector3(0, 0.33f, 0), 1f);
            owner.GetComponent<SpriteRenderer>().DOFade(0, 1).OnComplete(() =>
            {
                owner.gameObject.SetActive(false);
                owner.transform.SetParent(NpcManager.instance.NpcPool);
                owner.transform.localPosition = Vector3.zero;
            });
        }
    }
}
public class ExitPool : baseState
{
    public ExitPool(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        //Debug.Log("出对象池");
        base.OnEnter();
        owner.gameObject.SetActive(true);
        owner.transform.SetParent(GameManager.instance.npcParent);
        owner.GetComponent<SpriteRenderer>().DOFade(1, 2);
        owner.transform.DOBlendableMoveBy(new Vector3(0, 2.2f, 0), 1f).OnComplete(() =>
        {
            owner.sortLayer();
            owner.fsm.stateChange(owner.wanderState);
        });
    }
    public override void OnExit()
    {

    }
    public override void TickDecision()
    {

    }
    public override void TickPerFrame()
    {

    }
}
