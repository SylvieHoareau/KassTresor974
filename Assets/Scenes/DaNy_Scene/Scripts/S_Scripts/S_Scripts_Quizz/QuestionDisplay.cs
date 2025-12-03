using UnityEngine;
using UnityEngine.UI;

public class QuestionDisplay : MonoBehaviour
{
    public Image questionUIImage; // L'image UI dans la scène
    public QuestionData questionData; // Le scriptableObject

    public int index; // Index de la question à afficher

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        Question q = questionData.questions[index];

        // On applique le sprite à l'image UI
        if (q.questionImage != null)
        {
            questionUIImage.sprite = q.questionImage;
        }
        else 
        {
            questionUIImage.sprite = null;
        }
    }
}
