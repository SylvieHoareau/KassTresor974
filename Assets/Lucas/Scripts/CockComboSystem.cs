using UnityEngine;
using TMPro;

public class CockComboSystem : MonoBehaviour
{
    public static CockComboSystem Instance;

    [Header("Combo Settings")]
    public int comboCount = 0;       // nombre de coups consécutifs
    public int multiplier = 1;       // multiplicateur actuel
    public int comboStep = 10;       // tous les X coups, on augmente le multiplicateur
    public TextMeshProUGUI comboText;

    void Awake()
    {
        Instance = this;
    }

    // Appelée quand le joueur tue un ennemi
    public void AddCombo()
    {
          int oldMultiplier = multiplier;

        comboCount++;

        if (comboCount < comboStep)
        {
            multiplier = 1;
        }
        else
        {
            multiplier = (comboCount / comboStep) + 1;
        }

        // 🔥 Afficher FX uniquement si le multiplicateur augmente
        if (multiplier > oldMultiplier)
        {
            ComboVisualFX.Instance.ShowMultiplier(multiplier);
        }
    }

    // Appelée si le joueur rate ou prend un dégât
    public void ResetCombo()
    {
        int finalCombo = comboCount;
        CockStatsManager.SaveBestCombo(finalCombo);
        comboCount = 0;
        multiplier = 1;
    }

    // Récupérer le multiplicateur final utilisé pour les points
    public int GetMultiplier()
    {
        return multiplier;
    }

    void FixedUpdate()
    {
        comboText.text = "Combo X" + multiplier;
       
    }
}

