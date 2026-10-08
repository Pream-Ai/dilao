using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager instance;
    public List<NpcData> npcDataList = new List<NpcData>();
    public Dictionary<int, furniController> furniList = new Dictionary<int, furniController>();
    public Transform NpcPool;
    public float npcGenerateInterval = 10f;
    public float timer = 0f;
    public Vector3 bathPos=new Vector3(4,-2,0);
    public Sprite[] emoArray;
    private void Awake() => instance = this;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer>=npcGenerateInterval)
        {
            NpcGenerateMachine();
            timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            npcGenerateInterval = 1000000;
        }
    }
    public void OnFurnitureEntityAdd(furniController furni)
    {
       
    }
    /// <summary>
    /// Ԥ������λ�������Ƿ�Ԥ���ɹ�
    /// </summary>
    /// <param name="furni"></param>
    public void TryReseveSlot(furniController furni)
    {
        if (furni.beUsing) return;
        furni.beUsing = true;
    }
    /// <summary>
    /// ����λ�ͷ�
    /// </summary>
    /// <param name="furni"></param>
    public void ReleaseSlot(furniController furni)
    {
        furni.beUsing = false;
    }
    public async void NpcGenerateMachine()
    {
        if (npcDataList.Count == 0) return;
        int targetNpcId = Random.Range(0, npcDataList.Count);
        for (int i=0;i<NpcPool.childCount;i++)
        {
            var npc = NpcPool.GetChild(i).GetComponent<NpcController>();
            if (npc.data.ID==targetNpcId)
            {
                Debug.Log($"�;���������{targetNpcId}��");
                npc.transform.position = bathPos;
                npc.fsm.stateChange(npc.exitPool);
                return;
            }
        }

        var npcName = npcDataList[targetNpcId].name;
        var newNpc = await AssetService.Instance.InstantiateAsync($"Npc/{npcName}", GameManager.instance.npcParent);
        if (newNpc == null)
        {
            Debug.LogError($"加载NPC失败: {npcName}");
            return;
        }
        newNpc.transform.position = bathPos;
        var ctrl = newNpc.GetComponent<NpcController>();
        ctrl.fsm.stateChange(ctrl.exitPool);
    }
} 
