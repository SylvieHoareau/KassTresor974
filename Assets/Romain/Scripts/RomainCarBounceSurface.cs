using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RomainCarBlockSurface : MonoBehaviour
{
    [Header("Layer des objets à bloquer")]
    [Tooltip("Tous les objets sur ce(s) layer(s) seront bloqués par ce mur (ex: 'Car')")]
    [SerializeField] private LayerMask carLayers;

    private Collider wallCollider;

    private void Awake()
    {
        wallCollider = GetComponent<Collider>();

        if (wallCollider == null)
        {
            Debug.LogError("[RomainCarBlockSurface] Aucun Collider trouvé sur le mur.");
        }
        else if (wallCollider.isTrigger)
        {
            Debug.LogWarning("[RomainCarBlockSurface] Le collider est en Trigger. "
                            + "Pour bloquer les objets, il doit être en mode collision (isTrigger = false).");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        GameObject otherObj = collision.gameObject;

        // On ne traite que les objets sur le layer Car
        if (!IsInLayerMask(otherObj.layer, carLayers))
            return;

        Collider otherCol = collision.collider;
        if (otherCol == null || wallCollider == null)
            return;

        // On calcule la translation minimale pour sortir l'objet du mur
        Vector3 direction;
        float distance;

        bool overlapping = Physics.ComputePenetration(
            otherCol, otherCol.transform.position, otherCol.transform.rotation,
            wallCollider, wallCollider.transform.position, wallCollider.transform.rotation,
            out direction, out distance);

        if (overlapping && distance > 0f)
        {
            // On déplace l'objet en dehors du mur
            // direction = direction à appliquer à l'objet pour ne plus être en contact
            otherObj.transform.position += direction * distance;
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
