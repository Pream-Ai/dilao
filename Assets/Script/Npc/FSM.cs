using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class baseState
{
    protected NpcController owenr;
    public baseState(NpcController npc) { owenr = npc; }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
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
    public void Update()
    {
        _currentState?.OnUpdate();
    }
}
public class IdleState : baseState
{
    public IdleState(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
public class MoveState : baseState
{
    public MoveState(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
       
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        move(Vector3.zero);
    }
    void move(Vector3 targetPos)
    {

    }
}
public class WaitState : baseState
{
    public WaitState(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
public class BuyState : baseState
{
    public BuyState (NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
public class EnterPool : baseState
{
    public EnterPool(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        owenr.cleanNpcData();
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        
    }
}
public class ExitPool : baseState
{
    public ExitPool(NpcController npc) : base(npc) { }

    public override void OnEnter()
    {
        owenr.initNpcData();
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        
    }
}
