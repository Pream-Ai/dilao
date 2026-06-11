using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class task_bar : furniController
{
    public Transform Node;
    public override void OnInteract()
    {
        base.OnInteract();
        Node.gameObject.SetActive(true);
        Node.DOScale(Vector3.one,0.1f);
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
