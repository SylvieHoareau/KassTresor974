using UnityEngine;

public class SceneAudioPlayer : MonoBehaviour
{
    [SerializeField] private SceneAudioProfile audioProfile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (audioProfile != null)
        {
            AudioManager.Instance.PlayMusic(audioProfile.musicToPlay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
