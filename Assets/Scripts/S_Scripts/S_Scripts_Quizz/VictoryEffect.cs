using UnityEngine;

public class VictoryEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem confetti;

    // void Start()
    // {
    //     PlayVictory();
    // }

    public void PlayVictory()
    {
        if (confetti == null)
        {
            Debug.Log("Confetti non assigné !");
            return;
        }

        Debug.Log("PlayVictory appelé");
        // Garantit que l'effet redémarre à chaque victoire
        confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        confetti.Play();
    }
}
