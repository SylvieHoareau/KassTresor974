using UnityEngine;
using System.Collections;

/// <summary>
/// Gère l'affichage dynamique des objets 3D en fonction de leur distance avec la caméra.
/// Permet d'alléger considérablement le chargement et le rendu des scènes 3D lourdes.
/// </summary>
public class SceneLODManager : MonoBehaviour
{
    [Header("Configuration de la Caméra")]
    [Tooltip("La caméra principale utilisée pour calculer les distances.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Paramètres d'Optimisation 3D")]
    [Tooltip("Distance maximale (en mètres) à laquelle les objets 3D restent visibles.")]
    [SerializeField] private float maxVisibilityDistance = 50.0f;

    [Tooltip("Intervalle de temps (en secondes) entre chaque vérification des distances.")]
    [SerializeField] private float checkInterval = 0.5f;

    // Tableau contenant tous les rendus 3D à optimiser
    private Renderer[] allRenderers;

    private void Start()
    {
        // Recherche automatique de la caméra principale si non assignée
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Récupère tous les composants de rendu 3D présents dans la scène
        allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        Debug.Log($"[SceneLODManager] {allRenderers.Length} objet(s) 3D enregistrés pour l'optimisation.");

        // Lancement de la boucle de vérification en arrière-plan (Coroutine)
        StartCoroutine(OptimizeRenderersRoutine());
    }

    /// <summary>
    /// Coroutine qui vérifie périodiquement la distance entre la caméra et les objets 3D.
    /// </summary>
    private IEnumerator OptimizeRenderersRoutine()
    {
        while (true)
        {
            if (cameraTransform != null && allRenderers != null)
            {
                Vector3 cameraPosition = cameraTransform.position;

                // Parcourt tous les objets 3D de la scène
                foreach (Renderer objRenderer in allRenderers)
                {
                    if (objRenderer != null)
                    {
                        // Calcule la distance entre la caméra et l'objet 3D
                        float distance = Vector3.Distance(cameraPosition, objRenderer.transform.position);

                        // Active l'objet s'il est proche, le désactive s'il est trop loin
                        bool shouldBeVisible = distance <= maxVisibilityDistance;

                        if (objRenderer.enabled != shouldBeVisible)
                        {
                            objRenderer.enabled = shouldBeVisible;
                        }
                    }
                }
            }

            // Attend l'intervalle spécifié avant la prochaine vérification pour ne pas surcharger le processeur
            yield return new WaitForSeconds(checkInterval);
        }
    }
}
