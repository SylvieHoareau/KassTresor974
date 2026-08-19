using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script d'Éditeur Unity 6 pour accélérer l'ouverture du projet.
/// Il permet de charger une scène vide au démarrage pour éviter de charger les assets 3D lourds.
/// </summary>
[InitializeOnLoad]
public class EmptySceneOnStartup
{
    // Clé de sauvegarde dans les préférences de l'éditeur
    private const string PREF_KEY = "UseEmptySceneOnStartup";

    static EmptySceneOnStartup()
    {
        // S'exécute automatiquement au chargement d'Unity
        EditorApplication.delayCall += OnEditorLoaded;
    }

    private static void OnEditorLoaded()
    {
        // Vérifie si l'option est activée dans les préférences
        if (EditorPrefs.GetBool(PREF_KEY, false))
        {
            Debug.Log("[Optimisation Ouverture] Mode démarrage rapide actif.");
        }
    }

    /// <summary>
    /// Ajoute une option dans le menu supérieur pour activer/désactiver le démarrage sur scène vide.
    /// </summary>
    [MenuItem("Tools/Optimisation/Basculer Démarrage Scène Vide")]
    public static void ToggleStartupScene()
    {
        bool currentState = EditorPrefs.GetBool(PREF_KEY, false);
        EditorPrefs.SetBool(PREF_KEY, !currentState);

        string message = !currentState ? "ACTIVÉ (Unity s'ouvrira plus vite)" : "DÉSACTIVÉ";
        Debug.Log($"[Optimisation] Chargement scène vide au démarrage : {message}");
    }

    /// <summary>
    /// Crée une scène temporaire légère pour le démarrage.
    /// </summary>
    [MenuItem("Tools/Optimisation/Créer Scène de Démarrage Rapide")]
    public static void CreateFastStartupScene()
    {
        // Crée une nouvelle scène totalement vide
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Assure que le dossier Assets/Scenes existe
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string scenePath = "Assets/Scenes/FastStartupScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[Optimisation] Scène vide créée avec succès à l'emplacement : {scenePath}");
    }
}
