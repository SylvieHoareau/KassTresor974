#!/bin/bash
# ==============================================================================
# Script de détection des marqueurs de conflit Git dans les fichiers Unity (YAML)
# ==============================================================================

PROJECT_DIR="."

echo "=== Recherche des marqueurs de conflit Git dans $PROJECT_DIR ==="
echo ""

# Recherche des balises de conflit Git : <<<<<<<, =======, >>>>>>>
CONFLICT_FILES=$(grep -rl -E "^(<<<<<<<|=======|>>>>>>>)" "$PROJECT_DIR/Assets" "$PROJECT_DIR/Settings" 2>/dev/null)

if [ -n "$CONFLICT_FILES" ]; then
    echo "[ATTENTION] Des marqueurs de conflit ont été trouvés dans les fichiers suivants :"
    echo "$CONFLICT_FILES"
    echo ""
    echo "Résolution recommandée :"
    echo "Exécute 'git checkout --theirs <fichier>' pour accepter la version GitHub,"
    echo "ou 'git checkout --ours <fichier>' pour conserver ta version locale."
else
    echo "[OK] Aucun marqueur de conflit détecté dans les fichiers Assets et Settings !"
fi
