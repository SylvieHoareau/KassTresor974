using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Outil d'Éditeur pour corriger automatiquement les références d'arbres manquantes sur le Terrain.
/// </summary>
public class TerrainTreeFixer : EditorWindow
{
    [MenuItem("Tools/Kass Tresor 974/Corriger les Arbres du Terrain")]
    public static void FixMissingTerrainTrees()
    {
        // Récupération de tous les terrains actifs dans la scène courante
        Terrain[] terrains = Terrain.activeTerrains;

        if (terrains.Length == 0)
        {
            Debug.LogWarning("[TerrainTreeFixer] Aucun terrain actif n'a été trouvé dans la scène ouverte.");
            EditorUtility.DisplayDialog("Correction Terrain", "Aucun terrain actif trouvé dans cette scène.", "OK");
            return;
        }

        int totalRemoved = 0;

        foreach (Terrain terrain in terrains)
        {
            if (terrain.terrainData == null) continue;

            TerrainData data = terrain.terrainData;
            TreePrototype[] currentPrototypes = data.treePrototypes;
            List<TreePrototype> validPrototypes = new List<TreePrototype>();

            for (int i = 0; i < currentPrototypes.Length; i++)
            {
                // Détection d'un préfabriqué manquant (référence nulle)
                if (currentPrototypes[i].prefab == null)
                {
                    Debug.LogWarning($"[TerrainTreeFixer] Arbre manquant détecté à l'index {i} sur le terrain '{terrain.name}'.");
                    totalRemoved++;
                }
                else
                {
                    validPrototypes.Add(currentPrototypes[i]);
                }
            }

            // Si des éléments invalides ont été trouvés, mise à jour des données du terrain
            if (currentPrototypes.Length != validPrototypes.Count)
            {
                Undo.RecordObject(data, "Nettoyage des arbres manquants");
                data.treePrototypes = validPrototypes.ToArray();
                EditorUtility.SetDirty(data);
                terrain.Flush();
            }
        }

        // Affichage du compte rendu
        if (totalRemoved > 0)
        {
            Debug.Log($"[TerrainTreeFixer] Opération terminée : {totalRemoved} référence(s) supprimée(s).");
            EditorUtility.DisplayDialog("Correction réussie", $"{totalRemoved} référence(s) d'arbre manquant(s) ont été retirée(s).\n\nTu peux relancer le Build WebGL !", "OK");
        }
        else
        {
            Debug.Log("[TerrainTreeFixer] Aucune référence d'arbre manquante trouvée.");
            EditorUtility.DisplayDialog("Information", "Tous les préfabriqués d'arbres sur le terrain sont valides.", "OK");
        }
    }
}
