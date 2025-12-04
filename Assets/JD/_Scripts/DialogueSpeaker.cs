using UnityEngine;

[CreateAssetMenu(fileName = "New Speaker", menuName = "Dialogue/Speaker")]
public class DialogueSpeaker : ScriptableObject
{
    public string characterName;
    public Color nameColor = Color.white;
    public Sprite portrait;
}