using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadQuizzScene()
    {
        // Charger la scène Quizz
        SceneManager.LoadScene("Quizz");
    }

    public void LoadMainScene()
    {
        // Charger la scène de Romain
        SceneManager.LoadScene("Scene_Balade");
    }

    // Afficher les options du panel
    public void LoadOptionsScene()
    {
        // Charger la scène Options
        SceneManager.LoadScene("Options");
    }

    public void LoadParcPouleScene()
    {
        // Charger la scène du Parc Poule
        SceneManager.LoadScene("ParcPouleScene");
    }

    public void LoadBatayCoq()
    {
        // Charger la scène du jeu Batay Cok
        SceneManager.LoadScene("BatayCok");
    }

    public void LoadTri()
    {
        // Charger la scène du mini-jeu Tri
        SceneManager.LoadScene("Tri");
    }

    public void LoadCredits()
    {
        // Charger la scène de Crédits
        SceneManager.LoadScene("Credits");
    }

    public void ParcheminMap()
    {
        // Charger la scène de Crédits
        SceneManager.LoadScene("LevelMap");
    }

    // public void LoadEnigme()
    // {
    //     // Charger la scène du mini-jeu Enigme Final
    //     SceneManager.LoadScene("Enigme Final");
    // }

    public void Quit()
    {
        // Quitter l'application
        Application.Quit();
    }
    
}
