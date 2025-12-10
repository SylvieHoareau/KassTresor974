using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InGameMapManager : MonoBehaviour
{
    public GameObject mapUI; // Le panneau complet
    
    // Une liste spéciale pour relier "Nom" et "Objet visuel" dans l'inspecteur
    [System.Serializable]
    public struct MapMarker
    {
        public string locationName; // Ex: "Village"
        public GameObject markerObject; // L'image/texte sur la carte
    }

    public List<MapMarker> allMarkers; // On remplira ça dans l'inspecteur

    private GameControls inputActions;
    private bool isMapOpen = false;

    private void Awake()
    {
        inputActions = new GameControls();
        // Quand on appuie sur "OpenMap" -> On lance ToggleMap()
        inputActions.Gameplay.OpenMap.performed += ctx => ToggleMap();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void ToggleMap()
    {
        isMapOpen = !isMapOpen;

        if (isMapOpen)
        {
            OpenMap();
        }
        else
        {
            CloseMap();
        }
    }

    void OpenMap()
    {
        UpdateVisuals(); // On met à jour les points avant d'afficher
        mapUI.SetActive(true);
        Time.timeScale = 0f; // Pause le jeu (optionnel, selon ton choix)
    }

    void CloseMap()
    {
        mapUI.SetActive(false);
        Time.timeScale = 1f; // Reprend le jeu
    }

    void UpdateVisuals()
    {
        // On récupère la liste des lieux visités chez le Chef
        List<string> visited = GameManager.Instance.visitedLocations;

        // On vérifie chaque marqueur possible
        foreach (MapMarker marker in allMarkers)
        {
            // Si le nom du marqueur est dans la liste des visités...
            if (visited.Contains(marker.locationName))
            {
                marker.markerObject.SetActive(true); // On l'affiche !
            }
            else
            {
                marker.markerObject.SetActive(false); // Sinon on le cache
            }
        }
    }
}