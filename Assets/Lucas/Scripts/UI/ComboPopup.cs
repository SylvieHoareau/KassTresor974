using UnityEngine;
using TMPro;

public class ComboPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 0.6f;
    public float riseSpeed = 1.5f;

    public float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // Légère montée vers le haut
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Fade-out progressif
        float alpha = 1f - (timer / lifetime);
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    public void Setup(int multiplier)
    {
        text.text = "x" + multiplier + "!";

            // Couleurs par niveau
        if (multiplier <= 2) text.color = Color.white;
        else if (multiplier <= 4) text.color = new Color(1f, 0.85f, 0.3f); // or
        else if (multiplier <= 6) text.color = Color.red;
        else text.color = new Color(0.5f, 0f, 1f); // violet "épique"

        // Animation de pop (échelle)
        transform.localScale = Vector3.one * 0.5f;
        LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseOutBack();
    }
}