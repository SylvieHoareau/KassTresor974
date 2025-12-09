using UnityEngine;

public class UI_Espion : MonoBehaviour
{
    private void OnEnable()
    {
        // Cette ligne va imprimer la liste exacte des scripts qui ont ordonné l'activation
        Debug.LogError("🚨 LE MENU S'EST ACTIVÉ ! Voici le coupable :\n" + System.Environment.StackTrace);
    }
}