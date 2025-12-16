using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;

/// <summary>
/// Gère les animations de feedback visuel (Correct/Incorrect) pour un bouton
/// Nécessite le package DOTween
/// </summary>
public class AnswerButtonAnimator : MonoBehaviour
{
    [Header("Références UI")]
    // Composant Image du bouton (pour changer la couleur)
    [SerializeField] private Image buttonImage;

    // Le RectTransform de l'objet (pour le scaling)
    private RectTransform rectTransform;

    [Header("Paramères d'Animation")]
    [Tooltip("Durée totale de l'animation de succès (en secondes)")]
    [SerializeField] private float duration = 0.05f;

    // Couleur d'accentuation pour la bonne réponse
    [SerializeField] private Color correctColor = Color.green;

    // Couleur d'accentuation pour la mauvaise réponse
    [SerializeField] private Color incorrectColor = Color.red;

    // Echelle maximale lors de l'effet de pop
    [SerializeField] private float scaleUpFactor = 1.1f;

    // Couleur originale du bouton (à récupérer au départ)
    private Color originalColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Sécurité : S'assurer que le composant Image est là
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
    }

    /// <summary>
    /// Anime le bouton pour signaler une réponse CORRECTE
    /// </summary>
    public void AnimateCorrectAnswer()
    {
        // Arrêter toute animation DOTween en cours
        DOTween.Kill(this.transform);

        // SCALING 
        rectTransform.DOScale(scaleUpFactor, duration * 0.4f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => rectTransform.DOScale(1f, duration * 0.6f));

        // COULEUR
        buttonImage.DOColor(correctColor, duration * 0.4f)
            .SetLoops(2, LoopType.Yoyo)
            .SetDelay(0.1f);

    }

    /// <summary>
    /// Anime le bouton pour signaler une réponse INCORRECTE
    /// </summary>
    public void AnimateIncorrectAnswer()
    {
        DOTween.Kill(this.transform);

        // COULEUR
        buttonImage.DOColor(incorrectColor, 0.2f)
            .SetLoops(2, LoopType.Yoyo); 

        // SHAKE
        rectTransform.DOShakeAnchorPos(duration, strength: new Vector3(10f, 0f, 0f), vibrato: 10)
            .SetDelay(0.1f);

        // La couleur revient à l'originale après le shake
        rectTransform.DOScale(1f, 0.01f);
    }

    /// <summary>
    /// Anime le bouton pour signaler la bonne réponse lorsque le joueur a échoué (Hint/Indication).
    /// </summary>
    public void AnimateHint()
    {
        // Tuer les tweens en cours pour éviter les conflits
        DOTween.Kill(this.transform); 

        // Animer la couleur pour flasher la bonne réponse (légèrement moins agressif que la victoire)
        buttonImage.DOColor(correctColor, 0.3f)
            .SetLoops(3, LoopType.Yoyo) // Flasher 3 fois et revenir à la couleur originale
            .SetEase(Ease.InOutSine); // Courbe douce pour l'indication

        // Optionnel : un petit pop léger
        rectTransform.DOScale(1.05f, 0.15f)
            .SetEase(Ease.OutSine)
            .OnComplete(() => rectTransform.DOScale(1f, 0.15f));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
