using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tv : furniController
{
    public Sprite tv_light;
    GameObject light;
    public void Start()
    {
        light = new GameObject();
        light.AddComponent<SpriteRenderer>();
        light.GetComponent<SpriteRenderer>().sprite = tv_light;
        light.SetActive(false);
    }

    public override void OnInteract()
    {
        base.OnInteract();
        light.SetActive(true);
    }
    public override void EndInteract()
    {
        base.EndInteract();
        light.SetActive(false);
    }
}
