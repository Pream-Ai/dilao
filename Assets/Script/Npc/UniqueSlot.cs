using UnityEngine;

public class UniqueSlot 
{
    public int ID { get; private set; }
    public furniController OwnerCtrl { get; private set; }
    public Vector2Int interactPos { get; private set; }
    public bool isReserved { get; set; }
    public int ReserveNpcID { get; set; }
    UniqueSlot(int id, furniController ctrl,Vector2Int Pos,bool reserved,int npcid)
    {
        ID = id;
        OwnerCtrl = ctrl;
        interactPos = Pos;
        isReserved = reserved;
        ReserveNpcID = npcid;
    }
}
