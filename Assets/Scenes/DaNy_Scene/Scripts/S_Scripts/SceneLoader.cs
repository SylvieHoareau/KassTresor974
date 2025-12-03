using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadQuizzScene()
    {
        // Charger la scène Quizz
        SceneManager.LoadScene("Quizz");
    }

    // Afficher les options du panel
    public void LoadOptionsScene()
    {
        // Charger la scène Options
        SceneManager.LoadScene("Options");
    }

    public void Quit()
    {
        // Quitter l'application
        Application.Quit();
    }
    
}
