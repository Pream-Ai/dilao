using System.Collections.Generic;
using UnityEngine;

public class furniManager : MonoBehaviour
{
    public static furniManager instance;
    public List<FurniData> furniDataList = new List<FurniData>();
    public Dictionary<int, furniController> furniList = new Dictionary<int, furniController>();
    private int nextID = 0;
    public furniController ExitDoor;

    private void Awake() => instance = this;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    /// <summary>
    /// 注册家具，返回一个唯一id
    /// </summary>
    /// <param name="ctrl"></param>
    /// <returns></returns>
    public int RegisterFurni(furniController ctrl)
    {
        int id = nextID++;
        furniList[id]=ctrl;
        NpcManager.instance.furniList[id] = ctrl;
        return id;
    }
    /// <summary>
    /// 注销家具
    /// </summary>
    /// <param name="id"></param>
    public void UnregisterFurni(int id) 
    {
        foreach (var kv in furniList)
        {
            if (kv.Key == id)
            {
                furniList.Remove(id);
                break;
            }
        }
        foreach (var kv in NpcManager.instance.furniList)
        {
            if (kv.Key == id)
            {

                NpcManager.instance.furniList.Remove(id);
                break;
            }
        }
    }
}
