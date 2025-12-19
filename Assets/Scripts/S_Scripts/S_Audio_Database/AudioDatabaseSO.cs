using UnityEngine;
using System.Collections.Generic;

public enum MusicType
{
    Menu,
    Level,
    Boss,
    Victory,
    Defeat
}

public enum SFXType
{
    Click,
    Pickup,
    Error,
    Explosion,
    Win,
    Lose
}

[CreateAssetMenu(
    fileName = "AudioDatabase",
    menuName = "Audio/Audio Database"
)]

public class AudioDatabaseSO : ScriptableObject
{

    [Header("Music Library")]
    public List<MusicEntry> musics;

    [Header("SFX Library")]
    public List<SFXEntry> sfxs;
}

[System.Serializable]
public class MusicEntry
{
    public MusicType type;
    public AudioClip clip;
}

[System.Serializable]
public class SFXEntry
{
    public SFXType type;
    public AudioClip clip;
}
