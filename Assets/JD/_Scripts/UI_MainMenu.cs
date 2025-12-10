using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("LevelMap");
    }
    public void QuitGame()
    {
        Debug.Log("Le jeu se ferme !");
        Application.Quit();
    }
}