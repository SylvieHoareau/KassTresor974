using UnityEngine;

[CreateAssetMenu(fileName = "MusicToPlay", menuName = "Audio/MusicToPlay")]
public class SceneAudioProfile : ScriptableObject
{
    public MusicType musicToPlay;
}
