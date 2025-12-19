using UnityEngine;

[CreateAssetMenu(fileName = "Music", menuName = "Audio/Music")]
public class Music : ScriptableObject
{
    [Header("Identification")]
    public MusicType type;

    [Header("Audio")]
    public AudioClip clip;

    [Header("Scene")]
    public string sceneName;

    [Range(0f, 1f)]
    public float volume = 1f;

    public bool loop = true;
}
