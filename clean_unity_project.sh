#!/bin/bash
# ==============================================================================
# Script d'automatisation pour débloquer et nettoyer un projet Unity sous Debian
# ==============================================================================

echo "=== 1. Fermeture des processus Unity résiduels ==="
# Force l'arrêt des processus bloqués en arrière-plan
pkill -9 -f Unity
pkill -9 -f UnityHub
sleep 2
echo "Processus arrêtés."

echo ""
echo "=== 2. Suppression des fichiers de verrouillage (Lockfiles) ==="
# Suppression des fichiers temporaires qui bloquent le lancement depuis Unity Hub
rm -rf Temp/
rm -rf Logs/
rm -f UnityLockfile
rm -f Assets/*.lock
echo "Fichiers de verrouillage nettoyés."

echo ""
echo "=== 3. Option : Nettoyage du cache Library ==="
echo "Le dossier 'Library' contient le cache de compilation."
echo "Sa suppression force Unity à tout réindexer proprement au démarrage."
read -p "Souhaites-tu réinitialiser le dossier Library ? (o/N) : " reponse

if [[ "$reponse" =~ ^[oO]$ ]]; then
    echo "Suppression du dossier Library en cours (cela peut prendre quelques secondes)..."
    rm -rf Library/
    echo "Dossier Library supprimé. Unity le reconstruira automatiquement."
else
    echo "Dossier Library conservé."
fi

echo ""
echo "=============================================================================="
echo " Opération terminée ! Tu peux maintenant relancer Unity Hub et ton projet. "
echo "=============================================================================="
