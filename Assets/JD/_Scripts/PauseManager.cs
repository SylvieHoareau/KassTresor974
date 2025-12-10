using UnityEngine;
using UnityEngine.InputSystem; // Pour écouter la touche Echap
using UnityEngine.SceneManagement; // Pour le bouton Menu Principal

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private GameControls inputActions;
    private bool isPaused = false;

    private void Awake()
    {
        inputActions = new GameControls();
        
        // On dit : Quand on appuie sur "Pause", lance la fonction TogglePause()
        inputActions.Gameplay.Pause.performed += ctx => TogglePause();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    public void TogglePause()
    {
        isPaused = !isPaused; // On inverse (Vrai devient Faux, Faux devient Vrai)

        if (isPaused)
        {
            ActivateMenu();
        }
        else
        {
            DeactivateMenu();
        }
    }

    void ActivateMenu()
    {
        Time.timeScale = 0f; // ARRETE LE TEMPS
        pauseMenuUI.SetActive(true); // Affiche le menu
    }

    public void DeactivateMenu() // Public pour pouvoir le lier au bouton "Reprendre"
    {
        Time.timeScale = 1f; // REMET LE TEMPS
        pauseMenuUI.SetActive(false); // Cache le menu
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Important : remettre le temps avant de changer de scène !
        SceneManager.LoadScene("MainMenu");
    }
}