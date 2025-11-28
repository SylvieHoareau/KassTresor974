using UnityEngine;
using TMPro;
using System.Collections;

public class Cockdown : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public float countdownTime = 3f;

    void Start()
    {
        // Bloque tout le jeu
        Time.timeScale = 0f;

        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        float remaining = countdownTime;

        while (remaining > 0)
        {
            countdownText.text = Mathf.Ceil(remaining).ToString();
            yield return new WaitForSecondsRealtime(1f);
            remaining--;
        }

        countdownText.text = "GO !";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);

        // Relance le jeu
        Time.timeScale = 1f;
    }
}
