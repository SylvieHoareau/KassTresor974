using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RomainCarBlockSurface : MonoBehaviour
{
    [Header("Layers des objets à bloquer (ex: Car)")]
    [SerializeField] private LayerMask blockableLayers;

    [Header("Distance mini hors de la surface")]
    [SerializeField] private float skinOffset = 0.05f;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();

        // IMPORTANT : pour OnCollisionStay, le collider NE DOIT PAS être en Trigger.
        if (col.isTrigger)
        {
            Debug.LogWarning("[RomainCarBlockSurface] Le collider est en Trigger, les collisions ne seront pas reçues. Désactive 'Is Trigger' sur " + name);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null)
            return;

        // Filtre par layer
        if ((blockableLayers.value & (1 << rb.gameObject.layer)) == 0)
            return;

        // On cherche un script bloquable sur la voiture
        RomainCarBlockable blockable = rb.GetComponent<RomainCarBlockable>();
        if (blockable == null)
            return;

        int contactCount = collision.contactCount;
        if (contactCount == 0)
            return;

        // Moyenne des points et normales de contact (plus stable)
        Vector3 avgPoint = Vector3.zero;
        Vector3 avgNormal = Vector3.zero;

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint cp = collision.GetContact(i);
            avgPoint += cp.point;
            avgNormal += cp.normal;
        }

        avgPoint /= contactCount;
        avgNormal.Normalize();

        if (avgNormal.sqrMagnitude < 0.0001f)
            return;

        // avgNormal pointe du mur vers la voiture => direction de poussée vers l'extérieur
        blockable.BlockFromSurface(avgPoint, avgNormal, skinOffset);
    }
}
