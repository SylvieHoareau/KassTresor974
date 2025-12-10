using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // Pour toucher aux Sliders et Toggles

public class SettingsMenu : MonoBehaviour
{
    public void SetVolume(float volume)
    {
        // Change le volume global du jeu (de 0 à 1)
        AudioListener.volume = volume;
        // Sauvegarde le choix
        PlayerPrefs.SetFloat("gameVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        // Sauvegarde le choix (1 = Vrai, 0 = Faux)
        PlayerPrefs.SetInt("isFullscreen", isFullscreen ? 1 : 0);
    }
    
    // Au lancement, on recharge les options sauvegardées
    private void Start()
    {
        // Charger le volume (1 par défaut)
        float savedVolume = PlayerPrefs.GetFloat("gameVolume", 1f);
        AudioListener.volume = savedVolume;
        
        // Charger le plein écran (Vrai par défaut)
        bool savedFullscreen = PlayerPrefs.GetInt("isFullscreen", 1) == 1;
        Screen.fullScreen = savedFullscreen;

        // Note: Idéalement, il faudrait aussi mettre à jour visuellement le Slider et le Toggle ici,
        // mais pour l'instant on fait simple pour que ça marche.
    }
}