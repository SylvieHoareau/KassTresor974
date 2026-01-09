using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    // Instance statique pour y accéder depuis n'importe quel script
    public static InventoryManager Instance;

    public System.Action OnInventoryChanged;

    public List<Item> listeObjets = new List<Item>();

    private string savePath;

    private void Awake()
    {
        // On s'assure qu'il n'y a qu'un seul InventoryManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Garde l'objet actif
            // Définir le chemin de sauvegarde
            savePath = Application.persistentDataPath + "/save_inventory.json";
            LoadInventory(); // Charger automatiquement au lancement
        }
        else
        {
            Destroy(gameObject); // Détruit l'objet en double
        }
    }

    // Fonction pour ajouter un objet à l'inventaire
    public void AjouterObjet(Item nouvelObjet)
    {
        listeObjets.Add(nouvelObjet);
        Debug.Log("Objet ajouté à l'inventaire: " + nouvelObjet.nom);
        SaveInventory(); // Sauvegarder après l'ajout

        // On prévient tous ceux qui écoutent (comme l'UI) que çà a changé
        OnInventoryChanged?.Invoke();
    }

    public void SaveInventory()
    {
        SaveData data = new SaveData();
        foreach (Item item in listeObjets)
        {
            data.nomsObjetsPossedes.Add(item.name); // On stocke le nom de l'objet
        }

        string json = JsonUtility.ToJson(data); // Convertit la liste
        File.WriteAllText(savePath, json); // Ecrit le texte dans le fichier
        Debug.Log("Jeu sauvegardé à: " + savePath);
    }

    public void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath); // Lit le fichier
            SaveData data = JsonUtility.FromJson<SaveData>(json); // Reconvertit en liste de noms

            listeObjets.Clear();
            foreach (string nom in data.nomsObjetsPossedes)
            {
                // On va chercher l'objet dans le dossier Resources par son nom
                Item itemCharge = Resources.Load<Item>(nom);
                if (itemCharge != null)
                {
                    listeObjets.Add(itemCharge);
                }
            }
            Debug.Log("Inventaire chargé !");
        }
    }

    public void ResetGame()
    {
        // 1. Supprimer le fichier de sauvegarde
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Fichier de sauvegarde supprimé !");
        }

        // 2. Vider la liste actuelle en mémoire
        listeObjets.Clear();

        // 3. (Optionnel) Recharger la scène pour mettre à jour l'affichage
        // Nécessite : using UnityEngine.SceneManagement; en haut du script
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

}

[System.Serializable]
public class SaveData
{
    public List<string> nomsObjetsPossedes = new List<string>();
}
