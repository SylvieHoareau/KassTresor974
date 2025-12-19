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


public class AudioDatabase : MonoBehaviour
{

    [Header("Music Library")]
    public List<Music> musics; // Scriptable Objects

    [Header("SFX Library")]
    public List<SFX> sfxs; // Scriptable Objects
}

