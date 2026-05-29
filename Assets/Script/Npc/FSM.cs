using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public abstract class baseState
{
    protected NpcController owner;
    public baseState(NpcController npc) { this.owner = npc; }
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void TickPerFrame();
    public abstract void TickDecision();
}
public class FSM 
{
    public baseState _currentState;
    public void stateChange(baseState targetState )
    {
        _currentState.OnExit();
        _currentState = targetState;
        targetState.OnEnter();
    }
}
public class IdleState : baseState
{
    public IdleState(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        Debug.Log("进入站立状态");
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
public class MoveState : baseState
{
    public MoveState(NpcController npc) : base(npc) { }
    private List<Vector2Int> path;
    private int currentIndex = 0;
    private float moveSpeed = 5f;
    public override void OnEnter()
    {
        path = owner.AStar();
        currentIndex = 0 ;
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
                Debug.Log("到达目的地");
                //bool isLine
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
        Debug.Log("进入等待状态");
    }

    public override void OnExit()
    {
        Debug.Log("离开等待状态");
    }

    public override void TickDecision()
    {
    }

    public override void TickPerFrame()
    {
        timer += Time.deltaTime;
        if (timer>=waitTime)
        {
            var door = furniManager.instance.ExitDoor;
            owner.targetFurni = furniManager.instance.furniList[1];
            owner.fsm.stateChange(owner.moveState);
        }
    }
}
public class BuyState : baseState
{
    public BuyState (NpcController npc) : base(npc) { }
    float waitTime = 3;
    float timer = 0f;
    public override void OnEnter()
    {
        Debug.Log("进入购物状态");
        timer = 0f;
    }

    public override void OnExit()
    {
        Debug.Log("离开购物状态");
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
            owner.targetFurni = owner.getNextFurni();
            owner.fsm.stateChange(owner.moveState);
        }
    }
}
public class EnterPool : baseState
{
    public EnterPool(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        owner.cleanNpcData();
    }

    public override void OnExit()
    {
        
    }

    public override void TickDecision()
    {
        throw new System.NotImplementedException();
    }

    public override void TickPerFrame()
    {
        throw new System.NotImplementedException();
    }
}
public class ExitPool : baseState
{
    public ExitPool(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        owner.initNpcData();
    }

    public override void OnExit()
    {

    }

    public override void TickDecision()
    {
        throw new System.NotImplementedException();
    }

    public override void TickPerFrame()
    {
        throw new System.NotImplementedException();
    }
}
