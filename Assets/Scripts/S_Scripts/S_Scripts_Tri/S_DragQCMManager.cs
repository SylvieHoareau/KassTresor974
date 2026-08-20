using UnityEngine;
using UnityEngine.SceneManagement;

public class S_DragQCMManager : MonoBehaviour
{
    [Header("Slots & Configuration")]
    [SerializeField] private S_DropSlot[] slots;
    [SerializeField] private bool requireAllSlotsFilled = false;

    [Header("Panneaux de retour")]
    [SerializeField] private GameObject wrongFeedbackPanel;
    [SerializeField] private GameObject goodFeedbackPanel;

    [Header("SFX")]
    [SerializeField] private AudioClip wrongClip;
    [SerializeField] private AudioClip rightClip;
    [SerializeField] private AudioSource sfxAudioSource;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    public void CheckAnswers()
    {
        // Si l'option exige que tous les slots soient remplis et que ce n'est pas le cas
        if (requireAllSlotsFilled && !AllSlotsFilled())
        {
            Debug.Log("CheckAnswers: Tous les slots ne sont pas remplis.");
            ShowWrongFeedback();
            return;
        }

        // On initialise la variable avec le bon nom : allCorrect
        bool allCorrect = true;
        int checkedSlotsCount = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || !slots[i].isActiveAndEnabled) continue;

            // Si le slot a une carte, on vérifie si elle est au bon endroit
            if (slots[i].cardInSlot != null)
            {
                checkedSlotsCount++;
                bool result = slots[i].CheckSlot();
                Debug.Log($"Slot {i} ({slots[i].gameObject.name}) -> CheckSlot = {result}");

                if (!result)
                {
                    allCorrect = false;
                }
            }
            else if (slots[i].slotType == SlotType.AGarder)
            {
                // Si un slot "À garder" est laissé vide, la réponse est incomplète
                allCorrect = false;
            }
        }

        // S'assurer qu'au moins un objet a été déposé
        if (checkedSlotsCount == 0)
        {
            allCorrect = false;
        }

        Debug.Log($"CheckAnswers : Résultat final = {allCorrect}");

        if (allCorrect)
        {
            ShowGoodFeedback();
        }
        else
        {
            ShowWrongFeedback();
        }
    }

    private void ShowGoodFeedback()
    {
        if (wrongFeedbackPanel != null) wrongFeedbackPanel.SetActive(false);
        if (goodFeedbackPanel != null)
        {
            goodFeedbackPanel.SetActive(true);
            PlaySound(rightClip);
        }
    }

    private void ShowWrongFeedback()
    {
        if (goodFeedbackPanel != null) goodFeedbackPanel.SetActive(false);
        if (wrongFeedbackPanel != null)
        {
            wrongFeedbackPanel.SetActive(true);
            PlaySound(wrongClip);
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

    public void Replay()
    {
        if (wrongFeedbackPanel != null) wrongFeedbackPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        if (goodFeedbackPanel != null) goodFeedbackPanel.SetActive(false);

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("NextScene: dernière scène atteinte, chargement de la scène 0.");
            SceneManager.LoadScene(0);
        }
    }

    private bool AllSlotsFilled()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.isActiveAndEnabled && slot.cardInSlot == null)
            {
                return false;
            }
        }
        return true;
    }
}
