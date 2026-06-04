using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Emo : MonoBehaviour
{
    public List<Sprite> EmoList = new List<Sprite>();
    Transform bubble;
    Transform emo;
    Sequence emoSeq = DOTween.Sequence();
    void Start()
    {
        bubble = transform.GetChild(0);
        emo = transform.GetChild(1);
        showEmo();
    }
    void showEmo()
    {
        emoSeq.Append(bubble.DOScale(new Vector3(5,5,5),0.5f));
        emoSeq.AppendInterval(0.2f);
        emoSeq.Append(emo.DOScale(new Vector3(1.25f, 1.25f, 1.25f),0.5f));
        emoSeq.AppendCallback(()=>Reset());
    }
    void Reset()
    {
        bubble.localScale = new Vector3(1,1,1);
        emo.localScale = new Vector3(0.25f,0.25f,0.25f);
        transform.SetParent(NpcManager.instance.EmoPool);
    }
}
