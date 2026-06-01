using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager instance;
    private Dictionary<int, List<UniqueSlot>> reserveBook = new Dictionary<int, List<UniqueSlot>>();//×¢²á±í
    private Dictionary<furniController, List<UniqueSlot>> furniSlotMap = new Dictionary<furniController, List<UniqueSlot>>();
    public Action<UniqueSlot> onSlotAvailable;
    private void Awake() => instance = this;

}
