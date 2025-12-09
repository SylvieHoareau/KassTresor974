using UnityEngine;

public class ComboVisualFX : MonoBehaviour
{
    public static ComboVisualFX Instance;

    public GameObject comboPopupPrefab;
    public Canvas uiCanvas;

    void Awake()
    {
        Instance = this;
    }

    public void ShowMultiplier(int multiplier)
    {
        GameObject popup = Instantiate(comboPopupPrefab, uiCanvas.transform);
        popup.GetComponent<ComboPopup>().Setup(multiplier);
    }
}