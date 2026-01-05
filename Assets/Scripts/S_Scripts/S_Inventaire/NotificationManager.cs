using UnityEngine;
using TMPro;
using DG.Tweening; // Ne pas oublier DOTween !

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI Elements")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
        // On s'assure que c'est invisible au départ
        canvasGroup.alpha = 0;
        notificationPanel.SetActive(false);
    }

    public void AfficherNotification(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);

        // --- Séquence d'animation avec DOTween ---
        Sequence s = DOTween.Sequence();

        // 1. Apparition (Pop + Fade)
        notificationPanel.transform.localScale = Vector3.zero;
        s.Append(notificationPanel.transform.DOScale(1.1f, 0.4f).SetEase(Ease.OutBack));
        s.Join(canvasGroup.DOFade(1f, 0.3f));

        // 2. Petit retour à la taille normale
        s.Append(notificationPanel.transform.DOScale(1f, 0.1f));

        // 3. Pause (le joueur lit le message)
        s.AppendInterval(2.0f);

        // 4. Disparition (Fade + mouvement vers le haut)
        s.Append(canvasGroup.DOFade(0f, 0.5f));
        s.Join(notificationPanel.transform.DOMoveY(notificationPanel.transform.position.y + 50f, 0.5f));

        // 5. Nettoyage
        s.OnComplete(() => {
            notificationPanel.SetActive(false);
            // On remet la position d'origine pour la prochaine fois
            notificationPanel.transform.position -= new Vector3(0, 50f, 0);
        });
    }
}