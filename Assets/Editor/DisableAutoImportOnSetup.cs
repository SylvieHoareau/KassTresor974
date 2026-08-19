using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Script d'éditeur pour désactiver l'importation automatique au démarrage
/// et optimiser le traitement des assets très lourds.
/// </summary>
[InitializeOnLoad]
public class DisableAutoImportOnStartup : AssetPostprocessor
{
    // Seuil de taille de fichier en octets (ici 20 Mo)
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    /// <summary>
    /// Constructeur statique exécuté automatiquement dès le chargement d'Unity.
    /// </summary>
    static DisableAutoImportOnStartup()
    {
        // Désactive le rafraîchissement automatique des assets au démarrage de l'éditeur
        if (EditorPrefs.GetBool("kAutoRefresh", true))
        {
            EditorPrefs.SetBool("kAutoRefresh", false);
            Debug.Log("[Optimisation] Rafraîchissement automatique désactivé pour accélérer l'ouverture.");
        }
    }

    /// <summary>
    /// Intercepte l'importation des textures avant leur traitement complet par Unity.
    /// </summary>
    private void OnPreprocessTexture()
    {
        FileInfo fileInfo = new FileInfo(assetPath);

        // Vérifie si le fichier existe et dépasse le seuil défini (20 Mo)
        if (fileInfo.Exists && fileInfo.Length > MaxFileSizeBytes)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;

            // Réduit la taille maximale et force la compression pour éviter de figer l'éditeur
            textureImporter.maxTextureSize = 1024;
            textureImporter.textureCompression = TextureImporterCompression.Compressed;

            long sizeInMB = fileInfo.Length / (1024 * 1024);
            Debug.LogWarning($"[Optimisation Asset] Texture lourde détectée ({sizeInMB} Mo) : '{assetPath}'. Paramètres d'importation réduits automatiquement.");
        }
    }

    /// <summary>
    /// Ajoute une option dans la barre de menu d'Unity pour réactiver ou désactiver l'Auto-Refresh à tout moment.
    /// </summary>
    [MenuItem("Tools/Optimisation/Basculer Auto-Refresh")]
    public static void ToggleAutoRefresh()
    {
        bool currentStatus = EditorPrefs.GetBool("kAutoRefresh", true);
        EditorPrefs.SetBool("kAutoRefresh", !currentStatus);

        string statusText = !currentStatus ? "ACTIVÉ" : "DÉSACTIVÉ";
        Debug.Log($"[Optimisation] Rafraîchissement automatique des assets : {statusText}");
    }
}
