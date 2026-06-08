using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Emo : MonoBehaviour
{
    public List<Sprite> EmoList = new List<Sprite>();
    public Transform bubble;
    public Transform emo;
    private Sequence _currentEmoSeq;

    void Start()
    {
    }

    public void showEmo(int sortOrder, int emoIndex)
    {
        if (Random.Range(0, 10) > 1)
        {
            Reset();
            return;
        }
        if (_currentEmoSeq != null && _currentEmoSeq.IsActive())
        {
            _currentEmoSeq.Kill(); // 掐死动画
        }
        ForceResetTransforms();
        gameObject.SetActive(true);
        bubble.GetComponent<SpriteRenderer>().sortingOrder = sortOrder;
        emo.GetComponent<SpriteRenderer>().sprite = EmoList[emoIndex];
        emo.GetComponent<SpriteRenderer>().sortingOrder = sortOrder;
        _currentEmoSeq = DOTween.Sequence();
        _currentEmoSeq.Append(bubble.DOScale(new Vector3(5, 5, 5), 0.2f));
        _currentEmoSeq.AppendInterval(0.2f);
        _currentEmoSeq.Join(emo.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.2f));
        _currentEmoSeq.AppendInterval(2f);
        _currentEmoSeq.AppendCallback(() => Reset());
    }
    private void ForceResetTransforms()
    {
        bubble.localScale = new Vector3(1, 1, 1);
        emo.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    void Reset()
    {
        if (_currentEmoSeq != null && _currentEmoSeq.IsActive())
        {
            _currentEmoSeq.Kill();
        }

        gameObject.SetActive(false);
        ForceResetTransforms();
        transform.SetParent(NpcManager.instance.EmoPool);
    }
}