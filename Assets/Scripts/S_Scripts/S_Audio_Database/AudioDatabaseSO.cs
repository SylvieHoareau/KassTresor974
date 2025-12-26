using UnityEngine;
using System.Collections.Generic;

// public enum MusicType
// {
//     Menu,
//     Level,
//     Boss,
//     Victory,
//     Defeat
// }

// public enum SFXType
// {
//     Click,
//     Pickup,
//     Error,
//     Explosion,
//     Win,
//     Lose
// }

[CreateAssetMenu(
    fileName = "AudioDatabase",
    menuName = "Audio/Audio Database"
)]

public class AudioDatabaseSO : ScriptableObject
{

    [Header("Librairies")]
    public List<Music> musics;
    public List<SFX> sfxs;
}
