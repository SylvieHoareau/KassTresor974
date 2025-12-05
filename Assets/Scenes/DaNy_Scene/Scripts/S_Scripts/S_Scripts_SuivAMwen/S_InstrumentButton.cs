using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_InstrumentButton : MonoBehaviour
{
    [Header("Identité du Bouton")]
    public int idDirection; // 0=Haut, 1=Droite, 2=Bas, 3=Gauche
    public SuivAMwen_GameManager gameManager;

    [Header("Effets")]
    public Image imageBouton;
    public Color couleurNormale;
    public Color couleurActive; // La couleur quand çà s'allume
    public Color couleurDemonstration;
    // public AudioSource sonInstrument; // Le son du Kayamb, Roulèr

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(imageBouton != null)
            imageBouton.color = couleurNormale;
    }

    // Appelé quand le joueur clique (via le composant Button de Unity)
    public void OnClickJoueur()
    {
        StartCoroutine(AnimationFlash(couleurActive));
        S_SuivAMwen_AudioManager.Instance.JouerInstrument(idDirection);
        gameManager.TraiterInputJoueur(idDirection); // Prévient le GameManager
    }

    // Appelé par le GameManager lors de la démonstration
    public void ActiverBoutonAutomatiquement()
    {
        StartCoroutine(AnimationFlash(couleurDemonstration));
        S_SuivAMwen_AudioManager.Instance.JouerInstrument(idDirection);
    }

    // Petite animation de couleur
    IEnumerator AnimationFlash(Color flashColor)
    {
        if(imageBouton != null) 
            imageBouton.color = flashColor;  // Utilise la couleur passée en paramètres      
        
        yield return new WaitForSeconds(0.3f);
        
        if(imageBouton != null) 
            imageBouton.color = couleurNormale;
    }
}
