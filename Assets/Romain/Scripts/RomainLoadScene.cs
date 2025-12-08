using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RomainLoadScene : MonoBehaviour
{
    [Header("Scène à charger")]
    public string sceneName;

    [Header("Déclencheur")]
    public string playerTag = "Player";

    [Header("UI de chargement")]
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public string loadingMessage = "Chargement...";

    // Temps avant le début du chargement réel
    [Header("Timing")]
    [Tooltip("Pause avant que la scène ne commence réellement à charger")]
    public float preLoadDelay = 0.15f;

    private bool isLoading = false;

    private void Start()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;
        if (!other.CompareTag(playerTag)) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("RomainLoadScene : Aucun nom de scène renseigné dans l'inspecteur.");
            return;
        }

        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        isLoading = true;

        // 1. Afficher le panel immédiatement
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = loadingMessage;

        // 2. Attendre un tout petit moment AVANT le vrai chargement
        yield return new WaitForSeconds(preLoadDelay);

        // 3. Charger la scène en asynchrone
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // On attend la fin du chargement
        while (!asyncLoad.isDone)
            yield return null;
    }
}
