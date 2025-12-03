using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class S_DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Nom attendu de la carte")]
    // public string expectedItemName;

    public int expectedID; 
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

        if (cardInSlot.id == expectedID)
        {
            Debug.Log($"{gameObject.name} : Bonne réponse !");
            if (slotImage != null)
                slotImage.color = correctColor;
            return true;
        }
        else
        {
            Debug.Log($"{gameObject.name} : Mauvaise réponse...");

            if (slotImage != null)
                slotImage.color = incorrectColor;
            // rejeter la carte si la réponse est fausse
            // cardInSlot.transform.SetParent(cardInSlot.originalParent);

            return false;
        }
    }
}
