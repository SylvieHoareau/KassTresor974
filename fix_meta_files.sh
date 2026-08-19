#!/bin/bash
# ==============================================================================
# Script de détection et suppression des fichiers .meta corrompus ou vides
# ==============================================================================

PROJECT_ASSETS="./Assets"

echo "=== Analyse des fichiers .meta dans $PROJECT_ASSETS ==="

# Recherche de tous les fichiers .meta
find "$PROJECT_ASSETS" -type f -name "*.meta" | while read -r meta_file; do
    # 1. Vérifie si le fichier .meta est totalement vide (taille 0 octet)
    if [ ! -s "$meta_file" ]; then
        echo "[Suppression] Fichier .meta vide détecté : $meta_file"
        rm -f "$meta_file"
    # 2. Vérifie si le fichier .meta contient la clé 'guid:'
    elif ! grep -q "guid:" "$meta_file"; then
        echo "[Suppression] Fichier .meta sans GUID valide : $meta_file"
        rm -f "$meta_file"
    fi
done

echo ""
echo "=============================================================================="
echo " Nettoyage terminé ! Relance ou rafraîchis Unity (Ctrl + R) pour régénérer "
echo " les fichiers .meta valides. "
echo "=============================================================================="

