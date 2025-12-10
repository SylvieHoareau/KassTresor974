using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLevelLoader : MonoBehaviour
{
    // Fonction à appeler quand on clique sur le point
    public void LoadLevel(string levelName)
    {
        Debug.Log("Chargement du niveau : " + levelName);
        SceneManager.LoadScene(levelName);
    }
}