using System;
using System.Collections.Generic;

[Serializable]
public class FurniSaveData
{
    public int furniID;
    public int posX;
    public int posY;
}
[Serializable]
public class NpcSaveData
{
    public int npcID;
    public int posX;
    public int posY;
    public int remainMoney;
    public baseState currentState;
}
[Serializable]
public class GameSavePackage
{
    public int totalWorth;
    public int totalFurni;
    public int totalNpc;
    public int currentDay;

    public List<FurniSaveData> saveFurnitures = new List<FurniSaveData>();
    public List<NpcSaveData> saveNpc = new List<NpcSaveData>();
}
