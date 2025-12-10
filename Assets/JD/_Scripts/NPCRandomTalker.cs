using UnityEngine;
using TMPro; // Pour le texte 3D
using System.Collections;
using System.Collections.Generic;

public class NpcRandomTalker : MonoBehaviour
{
    [Header("Configuration")]
    public List<string> randomSentences; // Tes phrases aléatoires
    public float timeBetweenSentences = 5f; // Temps entre chaque phrase
    public float displayDuration = 3f; // Combien de temps le texte reste affiché

    [Header("Lien Visuel")]
    public TextMeshPro floatingText; // Le texte au-dessus de sa tête

    private bool isPlayerClose = false;

    void Start()
    {
        // On lance la boucle infinie de blabla
        StartCoroutine(ChatterRoutine());
    }

    IEnumerator ChatterRoutine()
    {
        while (true) // Boucle infinie
        {
            // On attend le délai prévu
            yield return new WaitForSeconds(timeBetweenSentences);

            // On ne parle QUE si le joueur est loin
            if (!isPlayerClose && randomSentences.Count > 0)
            {
                // 1. Choisir une phrase au hasard
                string randomPhrase = randomSentences[Random.Range(0, randomSentences.Count)];
                
                // 2. L'afficher
                floatingText.text = randomPhrase;

                // 3. Attendre un peu (temps de lecture)
                yield return new WaitForSeconds(displayDuration);

                // 4. Effacer
                floatingText.text = "";
            }
        }
    }

    // Détection automatique grâce au Trigger qui est DÉJÀ sur le PNJ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true; // Chut ! Le joueur est là.
            floatingText.text = ""; // On efface la bulle immédiatement
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false; // Le joueur part, je peux recommencer à marmonner.
        }
    }
}