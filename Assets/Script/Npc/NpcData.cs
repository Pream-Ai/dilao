using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class NpcData : ScriptableObject
{
    public int ID;
    public string npcName;
    public GameObject prefab;
    public int level;
    public int money;
}
