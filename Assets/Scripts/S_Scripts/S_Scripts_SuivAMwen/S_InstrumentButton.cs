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
    public AudioSource sonInstrument; // Le son du Kayamb, Roulèr

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        imageBouton.color = couleurNormale;
    }

    // Appelé quand le joueur clique (via le composant Button de Unity)
    public void OnClickJoueur()
    {
        StartCoroutine(AnimationFlash());
        sonInstrument.Play(); // Joue le son
        gameManager.TraiterInputJoueur(idDirection); // Prévient le GameManager
    }

    // Appelé par le GameManager lors de la démonstration
    public void ActiverBoutonAutomatiquement()
    {
        StartCoroutine(AnimationFlash());
        sonInstrument.Play();
    }

    // Petite animation de couleur
    IEnumerator AnimationFlash()
    {
        imageBouton.color = couleurActive;
        yield return new WaitForSeconds(0.3f);
        imageBouton.color = couleurNormale;
    }
}
