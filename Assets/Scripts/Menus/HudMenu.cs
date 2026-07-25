using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class HudMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject topbar;
    [SerializeField] private GameObject bottombar;

    [Header("Hud Slide In Settings")]
    [SerializeField] private float moveDistance = 40f;
    [SerializeField] private float duration = 0.4f;

    void Start()
    {
        HudSlideIn();
    }

    private void HudSlideIn()
    {
        // topbar fades in from the top
        RectTransform topbarRect = topbar.GetComponent<RectTransform>();
        CanvasGroup topbarCanvasGroup = topbar.GetComponent<CanvasGroup>();
        topbarCanvasGroup.alpha = 0f;
        Vector2 topbarTarget = topbarRect.anchoredPosition;
        topbarRect.anchoredPosition = topbarTarget + Vector2.up * moveDistance;

        // bottom bar fades in from the bottom
        RectTransform bottombarRect = bottombar.GetComponent<RectTransform>();
        CanvasGroup bottombarCanvasGroup = bottombar.GetComponent<CanvasGroup>();
        bottombarCanvasGroup.alpha = 0f;
        Vector2 bottombarTarget = bottombarRect.anchoredPosition;
        bottombarRect.anchoredPosition = bottombarTarget + Vector2.down * moveDistance;

        // start slide in
        Sequence slideIn = DOTween.Sequence().SetUpdate(true);
        slideIn.Join(topbarRect.DOAnchorPos(topbarTarget, duration).SetEase(Ease.OutCubic));
        slideIn.Join(topbarCanvasGroup.DOFade(1f, duration));
        slideIn.Join(bottombarRect.DOAnchorPos(bottombarTarget, duration).SetEase(Ease.OutCubic));
        slideIn.Join(bottombarCanvasGroup.DOFade(1f, duration));
    }
}
