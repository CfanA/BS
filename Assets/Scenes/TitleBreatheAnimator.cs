using UnityEngine;
using DG.Tweening;

public class TitleBreatheAnimator : MonoBehaviour
{
    void Start()
    {
        // 极度缓慢的缩放呼吸 (8秒一个来回)
        transform.DOScale(1.015f, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}