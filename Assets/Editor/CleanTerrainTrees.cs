using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Outil d'Éditeur pour détecter et supprimer les prototypes d'arbres manquants sur le Terrain.
/// </summary>
public class CleanTerrainTrees : EditorWindow
{
    [MenuItem("Tools/Terrain/Nettoyer les Arbres Manquants")]
    public static void CleanMissingTrees()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        if (terrains.Length == 0)
        {
            Debug.LogWarning("[Terrain Cleaner] Aucun Terrain actif trouvé dans la scène.");
            EditorUtility.DisplayDialog("Avertissement", "Aucun Terrain actif trouvé dans la scène ouverte.", "OK");
            return;
        }

        int totalRemoved = 0;

        foreach (Terrain terrain in terrains)
        {
            if (terrain.terrainData == null) continue;

            TreePrototype[] currentPrototypes = terrain.terrainData.treePrototypes;
            List<TreePrototype> validPrototypes = new List<TreePrototype>();

            for (int i = 0; i < currentPrototypes.Length; i++)
            {
                // Vérifie si le préfabriqué de l'arbre à l'index actuel est manquant
                if (currentPrototypes[i].prefab == null)
                {
                    Debug.LogWarning($"[Terrain Cleaner] Prototype d'arbre manquant détecté à l'index {i} sur le terrain '{terrain.name}'. Suppression en cours...");
                    totalRemoved++;
                }
                else
                {
                    validPrototypes.Add(currentPrototypes[i]);
                }
            }

            // Si des arbres manquants ont été trouvés, réactualise le TerrainData
            if (currentPrototypes.Length != validPrototypes.Count)
            {
                Undo.RecordObject(terrain.terrainData, "Suppression des prototypes d'arbres manquants");
                terrain.terrainData.treePrototypes = validPrototypes.ToArray();
                EditorUtility.SetDirty(terrain.terrainData);
                terrain.Flush();
            }
        }

        if (totalRemoved > 0)
        {
            Debug.Log($"[Terrain Cleaner] Opération terminée : {totalRemoved} prototype(s) d'arbre(s) supprimé(s).");
            EditorUtility.DisplayDialog("Nettoyage Réussi", $"{totalRemoved} prototype(s) d'arbre(s) manquant(s) ont été retiré(s) du Terrain.\n\nTu peux relancer la compilation WebGL !", "OK");
        }
        else
        {
            Debug.Log("[Terrain Cleaner] Aucun prototype d'arbre manquant n'a été détecté.");
            EditorUtility.DisplayDialog("Information", "Tous les prototypes d'arbres sur le Terrain sont valides.", "OK");
        }
    }
}
