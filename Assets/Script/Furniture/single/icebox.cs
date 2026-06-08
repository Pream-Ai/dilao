using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class icebox : furniController
{
    public Sprite closeIceBox;
    public Sprite openIceBox;
    private SpriteRenderer SR;
    protected override void Awake()
    {
        SR = GetComponent<SpriteRenderer>();
        SR.sprite = closeIceBox;
        base.Awake();
    }
    protected override void initFurni()
    {
        base.initFurni();
        SR.sprite = closeIceBox;
    }

    public override void OnInteract()
    {
        base.OnInteract();
        SR.sprite = openIceBox;
    }
    public override void EndInteract()
    {
        base.EndInteract(); 
        SR.sprite=closeIceBox;
    }
}
