using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage; // L'image enfant du prefab

    public void Setup(Item item)
    {
        iconImage.sprite = item.icone;
        iconImage.enabled = true; // On affiche l'image
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false; // On cache l'image si le slot est vide
    }
}