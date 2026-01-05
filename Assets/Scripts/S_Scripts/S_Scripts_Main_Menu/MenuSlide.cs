using UnityEngine;
using DG.Tweening;

public class MenuSlide : MonoBehaviour
{
    public RectTransform menuPanel;
    public RectTransform openButton;
    public CanvasGroup menuCanvasGroup;
    public float duration = 0.6f;
    private bool isOpen = false;
    private float hiddenY;

    void Start()
    {
        // Position cachée : menu entiérement hors écran 
        hiddenY = -menuPanel.rect.height;
        // Le menu est caché au démarrage
        menuPanel.anchoredPosition = new Vector2(
            menuPanel.anchoredPosition.x, 
            hiddenY
        );

        // Optionnel : Désactiver les clics sur le menu s'il est fermé au départ
        if (menuCanvasGroup != null) 
            menuCanvasGroup.blocksRaycasts = false;
    }
    public void ToggleMenu()
    {
        isOpen = !isOpen;

        // Bloque ou débloque les clics sur le panel

        if(menuCanvasGroup != null) {
            menuCanvasGroup.blocksRaycasts = isOpen;
        }

        // menuPanel.DOAnchorPosY(
        //     isOpen ? 0f : hiddenY,
        //     duration
        // ).SetEase(Ease.OutExpo);

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
