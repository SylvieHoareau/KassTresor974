using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // pour les Coroutines

public class SceneLoader : MonoBehaviour
{
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    } 

    private void Quit()
    {
        // Quitter l'application (Fonction réelle)
        Application.Quit();

        #if UNITY_EDITOR
        // Ligne ajoutée pour permettre de tester l'arrêt dans l'éditeur Unity
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void StartTransitionToScene(string sceneName)
    {
        // Ici, vous pouvez ajouter des animations de transition si nécessaire
        LoadScene(sceneName);
    }
    
    public void LoadMainMenu() => StartTransitionToScene("MainMenu");
    public void LoadMainScene() => StartTransitionToScene("Scene_Radio");
    public void LoadParcPoule() => StartTransitionToScene("ParcPouleScene");
    public void LoadBatayCoq() => StartTransitionToScene("BatayCok");
    public void LoadMap() => StartTransitionToScene("MapScene");
    public void LoadTri() => StartTransitionToScene("Tri");
    public void LoadQuizzScene() => StartTransitionToScene("Quizz");
    public void LoadOptionsScene() => StartTransitionToScene("Options");
    public void LoadCredits() => StartTransitionToScene("Credits");
    public void LoadCryptogramme() => StartTransitionToScene("Cryptogramme");

}
