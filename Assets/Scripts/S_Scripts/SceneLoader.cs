using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // pour les Coroutines

public class SceneLoader : MonoBehaviour
{
    // Configuration
    [Header("Audio Settings")]
    [Tooltip("L'effet sonore joué au clic du bouton.")]
    [SerializeField] private AudioClip buttonClickSFX;

    [Tooltip("Délai en secondes avant de charger la scène ou quitter")]
    [SerializeField] private float transitionDelay = 0.2f;

    // Méthodes des transition (Patron)
    /// <summary>
    /// Méthode patron générique pour jouer un SFX puis charger une scène après un 
    /// </summary>
    /// <param name="sceneName">Nom de la scène à changer.</param>
    private void StartTransitionToScene(string sceneName)
    {
        // Jouer le SFX de clic
        if (AudioManager.Instance != null && buttonClickSFX != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX);
        }

        // Démarrer la Coroutine responsable du délai
        StartCoroutine(LoadSceneAfterDelay(sceneName));
    } 

    /// <summary>
    /// Coroutine qui attend un certain temps avant de charger la scène
    /// </summary>
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        // Attendre le délai configuré (permet au SFX de jouer)
        yield return new WaitForSeconds(transitionDelay);

        // Charger la nouvelle scène
        SceneManager.LoadScene(sceneName);
    }

    // Méthodes publiques (appelées par les Boutons)
    // public void LoadQuizzScene()
    // {
    //     // Charger la scène Quizz
    //     SceneManager.LoadScene("Quizz");
    // }

    // public void LoadMainScene()
    // {
    //     // Charger la scène de Romain
    //     SceneManager.LoadScene("RomainScene");
    // }

    // Afficher les options du panel
    // public void LoadOptionsScene()
    // {
    //     // Charger la scène Options
    //     SceneManager.LoadScene("Options");
    // }

    // public void LoadParcPouleScene()
    // {
    //     // Charger la scène du Parc Poule
    //     SceneManager.LoadScene("ParcPouleScene");
    // }

    // public void LoadBatayCoq()
    // {
    //     // Charger la scène du jeu Batay Cok
    //     SceneManager.LoadScene("BatayCok");
    // }

    // public void LoadTri()
    // {
    //     // Charger la scène du mini-jeu Tri
    //     SceneManager.LoadScene("Tri");
    // }

    // public void LoadCredits()
    // {
    //     // Charger la scène de Crédits
    //     SceneManager.LoadScene("Credits");
    // }

    // public void LoadEnigme()
    // {
    //     // Charger la scène du mini-jeu Enigme Final
    //     SceneManager.LoadScene("Enigme Final");
    // }

    public void Quit()
    {
        // 1. Jouer le SFX de clic
        if (AudioManager.Instance != null && buttonClickSFX != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX);
        }

        // 2. Utiliser Invoke pour exécuter l'arrêt après le délai
        // C'est souvent plus simple que les Coroutines pour un seul appel sans argument.
        Invoke(nameof(QuitApplication), transitionDelay);
    }

    private void QuitApplication()
    {
        // Quitter l'application (Fonction réelle)
        Application.Quit();

        #if UNITY_EDITOR
        // Ligne ajoutée pour permettre de tester l'arrêt dans l'éditeur Unity
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    public void LoadMainMenu() => StartTransitionToScene("MainMenu");
    public void LoadMainScene() => StartTransitionToScene("ParcPouleScene");
    public void LoadBatayCoq() => StartTransitionToScene("BatayCok");
    public void LoadMap() => StartTransitionToScene("MapScene");
    public void LoadTri() => StartTransitionToScene("Tri");
    public void LoadQuizzScene() => StartTransitionToScene("Quizz");
    public void LoadOptionsScene() => StartTransitionToScene("Options");
    public void LoadCredits() => StartTransitionToScene("Credits");
    public void LoadEnigmeFinale() => StartTransitionToScene("Enigme Finale");

}
