using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Sécurité
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager introuvable !");
            return;
        }

        // Initialisation des sliders avec les valeurs sauvegardées
        musicSlider.value = AudioManager.Instance.MusicVolume;
        sfxSlider.value = AudioManager.Instance.SFXVolume;

        // Abonnements aux événements
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.MusicVolume =  value;
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SFXVolume =  value;
    }

     private void OnDestroy()
    {
        // Nettoyage (bonne pratique)
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
