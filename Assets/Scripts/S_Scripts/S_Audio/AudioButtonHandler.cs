using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class AudioButtonHandler : MonoBehaviour
{
    [SerializeField] private SFXType buttonClickSFX;
    [SerializeField] private float delay = 0.2f;

    // On utilise un UnityEvent pour appeler n'importe quelle fonction après le délai
    public UnityEvent onActionTriggered;

    public void PlayAndTrigger()
    {
        // Jouer le SFX de clic
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX);
        }

        // Démarrer la Coroutine pour le délai avant d'exécuter l'action
        StartCoroutine(ExecuteActionAfterDelay());
    }

    private IEnumerator ExecuteAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        onActionTriggered.Invoke(); // Appelle la fonction de chargement de scène
    }
}
