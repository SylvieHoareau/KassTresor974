using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject slotPrefab; // Prefab de slot
    public Transform gridTransform; // Le Panel avec le GridLayoutGroup

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AfficherInventaire();
    }

    public void AfficherInventaire()
    {
        // On nettoie la grille actuelle
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        // On récupère la liste des objets depuis le Manager
        List<Item> objetsPossedes = InventoryManager.Instance.listeObjets;

        // On crée un slot pour chaque objet dans l'inventaire
        foreach (Item item in objetsPossedes)
        {
            GameObject nouveauSlot = Instantiate(slotPrefab, gridTransform);
            InventorySlot slotScript = nouveauSlot.GetComponent<InventorySlot>();
                
            if (slotScript != null)
            {
                slotScript.Setup(item);
            }

            if (item.estParchemin)
            {
                // Créer le slot ...
            }
        }

    }
}
