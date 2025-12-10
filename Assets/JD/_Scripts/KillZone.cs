using UnityEngine;
using System.Collections; // Nécessaire pour le chrono

public class KillZone : MonoBehaviour
{
    private bool canKill = false; // La zone est inoffensive au début

    private void Start()
    {
        // On lance un petit chrono de sécurité
        StartCoroutine(StartSafetyDelay());
    }

    IEnumerator StartSafetyDelay()
    {
        // On attend 0.5 seconde que le chargement soit fini et le joueur placé
        yield return new WaitForSeconds(0.5f);
        canKill = true; // Maintenant la zone devient mortelle
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si on est encore dans le délai de sécurité, on ignore la collision
        if (!canKill) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Die();
            }
        }
    }
}