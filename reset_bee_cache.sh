#!/bin/bash
# ==============================================================================
# Script de nettoyage du cache de compilation Bee pour Unity
# ==============================================================================

# Indique le chemin de la racine de ton projet Unity
PROJECT_DIR="."

echo "=== Nettoyage du système de Build Bee Unity ==="
echo ""

# 1. Vérification si le dossier Library existe
if [ -d "$PROJECT_DIR/Library" ]; then

    # Suppression spécifique du dossier de cache Bee
    if [ -d "$PROJECT_DIR/Library/Bee" ]; then
        echo "[1/3] Suppression du cache Bee corrompu (Library/Bee)..."
        rm -rf "$PROJECT_DIR/Library/Bee"
        echo "      -> Cache Bee supprimé avec succès."
    else
        echo "[1/3] Le dossier Library/Bee n'existe pas."
    fi

    # Suppression des assemblies C# précompilées
    if [ -d "$PROJECT_DIR/Library/ScriptAssemblies" ]; then
        echo "[2/3] Nettoyage des assemblies C# (Library/ScriptAssemblies)..."
        rm -rf "$PROJECT_DIR/Library/ScriptAssemblies"
        echo "      -> Assemblies supprimées avec succès."
    fi

    # Nettoyage des logs de Build
    if [ -f "$PROJECT_DIR/Library/bee.backend.log" ]; then
        echo "[3/3] Suppression des fichiers de log verrouillés..."
        rm -f "$PROJECT_DIR/Library/bee.backend.log"*
    fi

    echo ""
    echo "=============================================================================="
    echo " Nettoyage terminé ! Tu peux maintenant rouvrir Unity."
    echo " Le moteur va recompiler le projet de manière propre."
    echo "=============================================================================="
else
    echo "[ERREUR] Le dossier 'Library' n'a pas été trouvé à l'emplacement actuel."
    echo "Assure-toi d'exécuter ce script à la racine de ton projet Unity."
fi
