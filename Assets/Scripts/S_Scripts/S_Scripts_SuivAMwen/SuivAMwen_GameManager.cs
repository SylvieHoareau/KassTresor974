using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SuivAMwen_GameManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Temps de pause entre deux notes de la séquence")]
    public float pauseEntreNotes = 0.1f;

    [Header("Références UI - Instruments (Bas de l'écran)")]
    [Tooltip("Les 4 boutons d'instruments (Kayamb, Triangle, Roulèr, Djembe)")]
    public List<S_InstrumentButton> boutonsInstruments;

    [Header("Références UI - Séquence Visuelle (Haut de l'écran)")]
    [Tooltip("Les 4 flèches de la séquence visuelle (Haut, Droite, Bas, Gauche)")]
    public List<S_InstrumentButton> boutonsFlechesSequence;

    [Header("Références UI - Textes & Boutons")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI scoreText;
    public GameObject boutonRejouer;

    // Variables internes
    private List<int> sequenceDeJeu = new List<int>();
    private int indexJoueur = 0;
    private bool tourDuJoueur = false;
    private int scoreActuel = 0;

    void Start()
    {
        DemarreNouvellePartie();
    }

    void DemarreNouvellePartie()
    {
        sequenceDeJeu.Clear();
        indexJoueur = 0;
        scoreActuel = 0;
        MettreAJourScoreUI();
        StartCoroutine(LancerProchainTour());
    }

    IEnumerator LancerProchainTour()
    {
        tourDuJoueur = false;
        indexJoueur = 0;
        messageText.text = "Ecoute bien la musique...";

        yield return new WaitForSeconds(1f);

        // Ajout d'une direction aléatoire (0=Haut, 1=Droite, 2=Bas, 3=Gauche)
        sequenceDeJeu.Add(Random.Range(0, 4));

        // Joue la séquence (son + flash instrument + flash flèche)
        foreach (int indexInstrument in sequenceDeJeu)
        {
            // 1. Joue le son de l'instrument
            S_SuivAMwen_AudioManager.Instance.JouerInstrument(indexInstrument);

            // 2. Flash visuel sur le bouton instrument (bas)
            if (indexInstrument < boutonsInstruments.Count && boutonsInstruments[indexInstrument] != null)
            {
                boutonsInstruments[indexInstrument].ActiverBoutonAutomatiquement();
            }

            // 3. Flash visuel sur la flèche correspondante (haut)
            if (indexInstrument < boutonsFlechesSequence.Count && boutonsFlechesSequence[indexInstrument] != null)
            {
                boutonsFlechesSequence[indexInstrument].ActiverBoutonAutomatiquement();
            }

            // 4. Pause basée sur la durée du clip audio
            float dureeNote = S_SuivAMwen_AudioManager.Instance.ObtenirDureeClip(indexInstrument);
            yield return new WaitForSeconds(dureeNote + pauseEntreNotes);
        }

        messageText.text = "Té suiv' à mwen ! (A toi)";
        tourDuJoueur = true;
    }

    public void TraiterInputJoueur(int idButton)
    {
        if (!tourDuJoueur) return;

        if (idButton != sequenceDeJeu[indexJoueur])
        {
            GameOver();
            return;
        }

        indexJoueur++;

        if (indexJoueur >= sequenceDeJeu.Count)
        {
            tourDuJoueur = false;
            scoreActuel += 100;
            MettreAJourScoreUI();

            messageText.text = "Gayar ! (Bravo)";
            S_SuivAMwen_AudioManager.Instance.JouerVictoire();
            StartCoroutine(LancerProchainTour());
        }
    }

    private void MettreAJourScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + scoreActuel;
        }
    }

    public void RejouerPartie()
    {
        if (boutonRejouer != null)
        {
            boutonRejouer.SetActive(false);
        }

        DemarreNouvellePartie();
    }

    void GameOver()
    {
        tourDuJoueur = false;
        messageText.text = "Aie aie aie... Ou la perdu !";
        S_SuivAMwen_AudioManager.Instance.JouerDefaite();

        if (boutonRejouer != null)
        {
            boutonRejouer.SetActive(true);
        }
    }
}
