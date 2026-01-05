using UnityEngine;


[CreateAssetMenu(fileName = "NouvelObjet", menuName = "Inventaire/Objet")]
public class Item : ScriptableObject
{
    public string nom; // Nom de l'objet
    public Sprite icone; // Image à afficher dans l'inventaire
    public bool estParchemin; // Pour différencier parchemins et objets insolites
    
}
