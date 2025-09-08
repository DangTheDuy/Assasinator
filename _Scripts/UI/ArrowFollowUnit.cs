using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArrowFollowUnit : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);
    private Transform arrowVisual;
    private Tween arrowTween;
    private Vector3 baseLocalPos;

    void Awake()
    {
        arrowVisual = transform.Find("ArrowImage");
        if (arrowVisual != null)
            baseLocalPos = arrowVisual.localPosition; 
    }

    void OnEnable()
    {
        if (arrowVisual != null)
            arrowVisual.localPosition = baseLocalPos;

        StartTween();
    }

    void OnDisable()
    {
        arrowTween?.Kill();
    }

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    private void StartTween()
    {
        if (arrowVisual == null) return;

        arrowTween?.Kill();

        arrowTween = arrowVisual.DOLocalMoveY(baseLocalPos.y + 0.2f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}



