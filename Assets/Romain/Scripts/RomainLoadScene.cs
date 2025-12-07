using UnityEngine;
using UnityEngine.SceneManagement;

public class RomainLoadScene : MonoBehaviour
{
    [Header("Nom de la scène à charger")]
    public string sceneName;

    [Header("Tag autorisé pour déclencher")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("RomainLoadScene : Aucun nom de scène renseigné dans l'inspecteur.");
            }
        }
    }
}
