using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère le chargement asynchrone des scènes pour éviter que le jeu ne fige au lancement.
/// </summary>
public class SafeSceneLoader : MonoBehaviour
{
    [Header("Configuration du chargement")]
    [Tooltip("Nom de la scène à charger après l'initialisation.")]
    [SerializeField] private string targetSceneName = "MaSceneDeJeu";

    [Tooltip("Temps d'attente minimum en secondes avant d'afficher la scène.")]
    [SerializeField] private float minimumWaitTime = 1.0f;

    private void Start()
    {
        // Lancement de la coroutine de chargement pour ne pas bloquer le thread principal
        StartCoroutine(LoadSceneAsyncProcess());
    }

    /// <summary>
    /// Coroutine qui charge la scène cible en arrière-plan sans figer l'écran.
    /// </summary>
    private IEnumerator LoadSceneAsyncProcess()
    {
        Debug.Log("[SafeSceneLoader] Début du chargement asynchrone...");

        // Attente de sécurité initiale
        yield return new WaitForSeconds(minimumWaitTime);

        // Début du chargement en arrière-plan
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);

        // Empêche la scène de s'activer automatiquement avant d'être totalement prête
        asyncOperation.allowSceneActivation = false;

        // Boucle d'attente tant que le chargement n'est pas terminé à 90%
        while (!asyncOperation.isDone)
        {
            // asyncOperation.progress va de 0.0 à 0.9 pendant le chargement
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            Debug.Log($"[SafeSceneLoader] Progression du chargement : {progress * 100}%");

            // Si le chargement atteint 90%, la scène est prête
            if (asyncOperation.progress >= 0.9f)
            {
                Debug.Log("[SafeSceneLoader] Chargement terminé ! Activation de la scène.");
                asyncOperation.allowSceneActivation = true;
            }

            // Rend la main à Unity pour la frame suivante (évite le freeze)
            yield return null;
        }
    }
}
