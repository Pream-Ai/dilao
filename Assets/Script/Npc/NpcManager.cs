using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager instance;
    public Dictionary<int, NpcData> npcDataList = new Dictionary<int, NpcData>();
    public Dictionary<int, furniController> furniList = new Dictionary<int, furniController>();
    public GameObject EmoPrefab;
    public Transform EmoPool;
    private void Awake() => instance = this;
    public void OnFurnitureEntityAdd(furniController furni)
    {
       
    }
    /// <summary>
    /// 预定服务位，返回是否预定成功
    /// </summary>
    /// <param name="furni"></param>
    public void TryReseveSlot(furniController furni)
    {
        if (furni.beUsing) return;
        furni.beUsing = true;
    }
    /// <summary>
    /// 服务位释放
    /// </summary>
    /// <param name="furni"></param>
    public void ReleaseSlot(furniController furni)
    {
        furni.beUsing = false;
    }

    public GameObject getEmo()
    {
        if (EmoPool.childCount>0)return EmoPool.GetChild(0).gameObject;
        return Instantiate(EmoPrefab);
    }
} 
