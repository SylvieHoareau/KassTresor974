using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public enum SlotType
{
    AGarder, // Pour les objets volés par la Buse (croix, médaille, collier)
    AEcarter  // Pour les autres objets à laisser de côté
}

public class S_DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Configuration du slot")]
    [Tooltip("Type de zone : A Garder ou A Ecarter")]
    public SlotType slotType = SlotType.AGarder;

    [Tooltip("Les ID des objets considérés comme de VRAIS trésors à garder (ex: croix, médaille, collier)")]
    public int[] validKeepIDs = { 0, 1, 2 }; // Modifiable depuis l'Inspector

    [Header("Composants & Visuels")]
    public Image slotImage;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public S_Draggable cardInSlot;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        S_Draggable card = eventData.pointerDrag.GetComponent<S_Draggable>();

        if (card == null) return;

        // Si le slot est déjà occupé, on refuse la nouvelle carte
        if (cardInSlot != null)
        {
            card.ResetPos();
            return;
        }

        // On dépose la carte dans ce slot
        cardInSlot = card;
        card.current_slot = this;
        card.transform.SetParent(transform);
        card.rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Vérifie si la carte actuellement placée dans ce slot correspond à la consigne.
    /// </summary>
    public bool CheckSlot()
    {
        if (cardInSlot == null) return false;

        // La carte déposée fait-elle partie de la liste des trésors à garder ?
        bool isCardToKeep = validKeepIDs.Contains(cardInSlot.id);

        bool isCorrectMatch;

        if (slotType == SlotType.AGarder)
        {
            // Dans la zone "À garder", la carte doit être un vrai trésor
            isCorrectMatch = isCardToKeep;
        }
        else // SlotType.AEcarter
        {
            // Dans la zone "À écarter", la carte NE DOIT PAS être un vrai trésor
            isCorrectMatch = !isCardToKeep;
        }

        // Retour visuel (couleur du slot)
        if (slotImage != null)
        {
            slotImage.color = isCorrectMatch ? correctColor : incorrectColor;
        }

        Debug.Log($"Slot [{gameObject.name}] ({slotType}) -> Carte ID {cardInSlot.id} : {(isCorrectMatch ? "VALIDE" : "INVALIDE")}");

        return isCorrectMatch;
    }
}
