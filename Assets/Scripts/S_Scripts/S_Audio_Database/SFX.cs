using UnityEngine;

[CreateAssetMenu(fileName = "NewSFX", menuName = "Audio/SFX")]
public class SFX : ScriptableObject
{
    public SFXType type; // Utilise l'enum défini dans AudioDatabaseSO.cs
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 2f)]
    public float pitch = 1f;

    // Variation de pitch pour plus de réalisme
    public bool useRandomPitch = false;
}
