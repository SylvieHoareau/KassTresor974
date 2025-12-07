using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    public string nextSceneName = "Game";
    
    [Header("Options")]
    public bool triggerOnWalk = true; // Si VRAI : Change de scène quand on marche dedans. Si FAUX : Attend un ordre.

    private void OnTriggerEnter(Collider other)
    {
        // On ne change de scène QUE si l'option est cochée
        if (triggerOnWalk && other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Attention : Pas de nom de scène dans ExitZone !");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}