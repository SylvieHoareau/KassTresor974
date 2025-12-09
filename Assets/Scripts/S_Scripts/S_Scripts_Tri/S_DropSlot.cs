using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq; // Pour utiliser .Contains() sur un tableau d'int

public enum SlotType
{
    AGarder, // Pour les vrais trésors
    AEcarter // Pour les faux trésors (butin)
}

public class S_DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Configuration du slot")]
    // La liste des ID des cartes considérées comme "bonnes" pour le jeu
    // 1, 3 et 5 sont les ID des objets "A GARDER"
    private static readonly int[] IDs_A_Garder = { 0, 2, 4 };
    // Pour définir si ce slot est pour les cartes "A GARDER" ou "A ECARTER"
    public SlotType slotType = SlotType.AGarder;
    [Header("Nom attendu de la carte")]
    // public string expectedItemName;

    // public int expectedID; 
    public Image slotImage;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public S_Draggable cardInSlot;

    public void OnDrop(PointerEventData eventData)
    {
        S_Draggable card =  eventData.pointerDrag.GetComponent<S_Draggable>();

        if (card == null)
            return;

        // Slot déjà occupé
        if (cardInSlot != null)
        {
            // On rejette la carte
            card.ResetPos();
            return;
        }

        // On dépose la carte
        cardInSlot = card;
        card.current_slot = this;
        card.transform.SetParent(transform);
        card.rectTransform.anchoredPosition = Vector2.zero;
    }

    public bool CheckSlot()
    {
        if(cardInSlot == null) return false;

        // Vérifier si la carte est un objet "A GARDER"
        bool isCardToKeep = IDs_A_Garder.Contains(cardInSlot.id);

        // Vérifier si le type de la carte correspond au type de slot
        bool isCorrectMatch;

        if (slotType == SlotType.AGarder)
        {
            isCorrectMatch = isCardToKeep;
        }
        else // SlotType.AEcarter
        {
            isCorrectMatch = !isCardToKeep;
        }
       
        // Changer la couleur du slot en fonction du résultat
        if (isCorrectMatch)
        {
            Debug.Log($"{gameObject.name} (Type: {slotType}) : Correct match with card ID {cardInSlot.id}");
            if (slotImage != null)
            {
                slotImage.color = correctColor; // Affiche en vert
            }

            return true;    
        }
        else
        {
            Debug.Log($"{gameObject.name} (Type: {slotType}) : Bad match with card ID {cardInSlot.id}");

            if (slotImage != null)
            {
                slotImage.color = incorrectColor; // Affiche en rouge
            }

            return false;
        }
    }
}
