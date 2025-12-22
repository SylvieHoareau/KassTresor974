using UnityEngine;
using DG.Tweening;

public class MenuSlide : MonoBehaviour
{
    public RectTransform menuPanel;
    public RectTransform openButton;
    public float duration = 0.6f;
    private bool isOpen = false;
    private float hiddenY;

    void Start()
    {
        hiddenY = -menuPanel.rect.height;
        // Le menu est caché au démarrage
        menuPanel.anchoredPosition = new Vector2(0, hiddenY);
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        menuPanel.DOAnchorPosY(
            isOpen ? 0f : hiddenY,
            duration
        ).SetEase(Ease.OutExpo);

        openButton.DOScale(isOpen ? 0.8f : 1f, 0.2f);
        openButton.DORotate(
            isOpen ? new Vector3(0, 0, 180) : Vector3.zero,
            0.25f,
            RotateMode.FastBeyond360
        ).SetEase(Ease.OutBack);
    }
}
