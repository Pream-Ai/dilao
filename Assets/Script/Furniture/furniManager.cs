using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class furniManager : MonoBehaviour
{
    public static furniManager instance;
    public List<FurniData> furniDataList = new List<FurniData>();
    public List<furniController> furniList = new List<furniController>();
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
