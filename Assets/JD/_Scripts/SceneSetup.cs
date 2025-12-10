using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [Header("UI à nettoyer au démarrage")]
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;

    void Start()
    {
        Time.timeScale = 1f; // On remet le temps

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentState = GameManager.GameState.Gameplay;
            GameManager.Instance.gameOverUI = gameOverUI; // On donne la référence au Chef
        }

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    // --- NOUVEAU : LES FONCTIONS POUR LES BOUTONS ---

    public void OnRetryButton()
    {
        // On demande au Chef (GameManager) de relancer
        GameManager.Instance.RestartGame();
    }

    public void OnQuitButton()
    {
        // On demande au Chef de quitter
        GameManager.Instance.QuitToMenu();
    }
}