using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    // LA SEULE ET UNIQUE TÉLÉCOMMANDE DU JEU
    public GameControls inputs; 

    public enum GameState { MainMenu, Gameplay, Paused, Dialogue, GameOver }
    public GameState CurrentState;

    [Header("Interfaces")]
    public GameObject gameOverUI;

    [Header("Exploration")]
    // La liste des noms de lieux découverts (ex: "Village", "Forêt")
    public List<string> visitedLocations = new List<string>();

    public void UnlockLocation(string locationName)
    {
        // Si on ne connait pas encore ce lieu, on l'ajoute
        if (!visitedLocations.Contains(locationName))
        {
            visitedLocations.Add(locationName);
            Debug.Log("Nouveau lieu découvert : " + locationName);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 1. On crée les inputs ICI et une seule fois
            inputs = new GameControls();
            inputs.Enable(); // On les active pour tout le jeu
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ... Le reste ne change pas (TriggerGameOver, etc.) ...
    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f; 
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}