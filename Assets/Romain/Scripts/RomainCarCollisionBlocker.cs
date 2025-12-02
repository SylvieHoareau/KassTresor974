using UnityEngine;

public class RomainCarResetZone : MonoBehaviour
{
    [Header("Zones interdites")]
    [SerializeField] private LayerMask forbiddenLayers;

    [Header("Point de spawn")]
    [SerializeField] private Transform spawnPoint;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet appartient au layer interdit
        if ((forbiddenLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Reset de la voiture
        ResetCarToSpawn();
    }

    private void ResetCarToSpawn()
    {
        // Coupe les forces actuelles pour éviter de respawn et glisser
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Téléporte la voiture au spawn
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}
