using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class gacha : furniController
{
    public Transform Bubble;
    public Transform Node;
    public Sprite[] rewardSprites;
    public Sprite egg;
    
    public void Start()
    {

    }
    public void getReward()
    {
        Node.DORotate(new Vector3(0, 0, 20), 0.1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(Mathf.CeilToInt(1.5f / 0.1f), LoopType.Yoyo)
            .OnComplete(() =>
            {
                Node.rotation = Quaternion.identity;
                Node.GetComponent<SpriteRenderer>().sprite=rewardSprites[Random.Range(0,rewardSprites.Length)];
            });
    }
    public override void OnInteract()
    {
        base.OnInteract();
        Bubble.gameObject.SetActive(true);
        Bubble.DOScale(Vector3.one,0.1f);
        getReward();
    }
    public override void EndInteract()
    {
        base.EndInteract();
        Bubble.DOScale(Vector3.zero, 0.1f).OnComplete(() =>
        {
            Bubble.gameObject.SetActive(false);
            Node.GetComponent<SpriteRenderer>().sprite = egg;
        });
    }
}
