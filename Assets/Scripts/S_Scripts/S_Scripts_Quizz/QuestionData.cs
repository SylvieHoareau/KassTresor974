using UnityEngine;

[System.Serializable]
public class Question
{
    public string question;
    public string[] replies;
    public int correctOptionIndex;
    public Sprite questionImage;
}

[CreateAssetMenu(fileName = "New Category", menuName = "Quiz/QuestionData")]
public class QuestionData : ScriptableObject
{
    public string category;
    public Question[] questions;
}
