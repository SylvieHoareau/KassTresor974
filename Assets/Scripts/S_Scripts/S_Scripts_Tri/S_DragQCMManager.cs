using UnityEngine;
using UnityEngine.SceneManagement;

public class S_DragQCMManager : MonoBehaviour
{
    [SerializeField] private S_DropSlot[] slots;
    [SerializeField] private GameObject wrongFeedbackPanel;
    [SerializeField] private GameObject goodFeedbackPanel;
    [Header("SFX")]
    [SerializeField] private AudioClip wrongClip;
    [SerializeField] private AudioClip rightClip;
    [SerializeField] private AudioSource sfxAudioSource;
    [Range(0f,1f)] [SerializeField] private float sfxVolume = 1f;

    public void CheckAnswers()
    {
        // S'assurer que tous les slots soient remplis
        if (!AllSlotsFilled())
        {
            Debug.Log("CheckAnswers: Tous les slots ne sont pas remplis.");
            // Optionnel: Afficher un message à l'utilisateur pour remplir tous les slots
            wrongFeedbackPanel.SetActive(true);
            PlaySound(wrongClip);
            return;
        }

        bool all_correct = true;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isActiveAndEnabled) continue;
            bool result = slots[i].CheckSlot();
            Debug.Log($"Slot {i} → CheckSlot() = {result}, cardInSlot = {slots[i].cardInSlot}");
            if(!result) all_correct = false;
        }

        Debug.Log($"CheckAnswers: all_correct = {all_correct}");

        if(all_correct)
        {
            // Continue to next game
            Debug.Log("Showing good feedback panel");
            if (wrongFeedbackPanel != null)
            {
                wrongFeedbackPanel.SetActive(false);
            }
            if (goodFeedbackPanel != null)
            {
                // SceneManager.LoadScene("HugoLabo");
                // KeepInLoad.Instance.ShowDialogue(true);
                // DialogueManager.Instance.isTalking = true;
                goodFeedbackPanel.SetActive(true);
                PlaySound(rightClip);
            }
            else
            {
                Debug.LogWarning("goodFeedbackPanel is not assigned!");
            }
        }
        else
        {
            // Play wrong feedback
            Debug.Log("Showing wrong feedback panel");
            if (goodFeedbackPanel != null)
            {
                goodFeedbackPanel.SetActive(false);
            }
            if (wrongFeedbackPanel != null)
            {
                wrongFeedbackPanel.SetActive(true);
                PlaySound(wrongClip);
            }
            else
            {
                Debug.LogWarning("wrongFeedbackPanel is not assigned!");
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
        }
    }

    // Called by the UI "Rejouer" button (or from code) to restart the current scene
    public void Replay()
    {
        // Désactive le panneau de feedback
        if (wrongFeedbackPanel != null)
        {
            wrongFeedbackPanel.SetActive(false);
        }

        // Recharge la scène actuelle pour redémarrer le niveau
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        if (goodFeedbackPanel != null)
        {
            goodFeedbackPanel.SetActive(false);
        }

        // Load the next scene in Build Settings if it exists.
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // Last scene reached: reload first scene (index 0) or change behaviour as needed.
            Debug.Log("NextScene: last scene reached, loading scene 0.");
            SceneManager.LoadScene(0);
        }
    }

    // Fonction utilitaire pour vérifier si tous les slots sont remplis
    private bool AllSlotsFilled()
    {
        foreach (var slot in slots)
        {
            if (slot.isActiveAndEnabled && slot.cardInSlot == null)
            {
                return false; // Au moins un slot actif est vide
            }
        }
        return true; // Tous les slots actifs sont remplis
    }
}
