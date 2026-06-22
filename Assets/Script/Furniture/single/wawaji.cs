using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class wawaji : furniController
{
    [Header("娃娃配置")]
    public Sprite[] rewardSprites;
    public Transform Node;
    private SpriteRenderer targetSprite;

    [Header("动画参数")]
    public float fastInterval = 0.05f;
    public float slowInterval = 0.1f;
    public float duration = 2f;
    public AnimationCurve speedCurve;

    private Coroutine rollingCoroutine;
    private int finalRewardIndex = -1;
    private AnimationCurve DefaultCurve//默认曲线参数
    {
        get
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 1f);
            curve.AddKey(0.7f, 1f);
            curve.AddKey(1f, 0f);
            return curve;
        }
    }
    private void Start()
    {
        targetSprite = Node.GetComponent<SpriteRenderer>();
        if (speedCurve == null || speedCurve.keys.Length == 0) speedCurve = DefaultCurve;
    }
    public void getReward(int finalIndex)
    {
        if (rollingCoroutine != null)
            StopCoroutine(rollingCoroutine);

        finalRewardIndex = finalIndex;
        rollingCoroutine = StartCoroutine(RollingCoroutine());
    }
    private IEnumerator RollingCoroutine()
    {
        float elapsed = 0f;
        int currentFram = 0;
        while (elapsed < duration)
        {
            //计算进度
            float t = elapsed / duration;
            //根据曲线调整速度
            float speedFactor = speedCurve.Evaluate(t);
            float currentInterval = Mathf.Lerp(slowInterval, fastInterval, speedFactor);
            if (elapsed < duration - 0.3f)
            {
                int randomIndex = Random.Range(0, rewardSprites.Length);
                targetSprite.sprite = rewardSprites[randomIndex];
            }
            else if (elapsed >= duration - 0.3f && finalRewardIndex >= 0)
            {
                targetSprite.sprite = rewardSprites[finalRewardIndex];
            }
            yield return new WaitForSeconds(currentInterval);
            elapsed += currentInterval;
            currentFram++;
        }
        if (finalRewardIndex >= 0)
            targetSprite.sprite = rewardSprites[finalRewardIndex];
        rollingCoroutine = null;
    }
    public override void OnInteract()
    {
        base.OnInteract();
        Node.gameObject.SetActive(true);
        Node.DOScale(Vector3.one, 0.1f);
        getReward(Random.Range(0, rewardSprites.Length));
    }
    public override void EndInteract()
    {
        base.EndInteract();
        Node.DOScale(Vector3.zero, 0.1f).OnComplete(() =>
        {
            Node.gameObject.SetActive(false);
        });
    }
}
