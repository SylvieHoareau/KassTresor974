using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RomainCarBlockable : MonoBehaviour
{
    [Header("Correction de pénétration")]
    [Tooltip("Limite de correction par frame pour éviter les gros snaps")]
    [SerializeField] private float maxCorrectionPerFrame = 0.5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Appelé par une surface de blocage pour empêcher l'objet d'entrer dans le mur.
    /// </summary>
    /// <param name="surfacePoint">Point de contact sur la surface.</param>
    /// <param name="pushDir">Direction depuis le mur vers l'extérieur.</param>
    /// <param name="skin">Distance mini désirée entre l'objet et la surface.</param>
    public void BlockFromSurface(Vector3 surfacePoint, Vector3 pushDir, float skin)
    {
        // Position actuelle de la voiture
        Vector3 pos = rb.isKinematic ? transform.position : rb.position;

        // Vecteur surface -> voiture
        Vector3 toObj = pos - surfacePoint;

        // Distance projetée sur la normale
        float distAlongNormal = Vector3.Dot(toObj, pushDir);

        // On voudrait : distAlongNormal >= skin
        float correction = skin - distAlongNormal;

        // Si déjà assez dehors, rien à faire
        if (correction <= 0f)
            return;

        // Évite que ça téléporte trop loin en une seule frame
        correction = Mathf.Min(correction, maxCorrectionPerFrame);

        // Nouvelle position corrigée
        pos += pushDir * correction;

        if (rb.isKinematic)
        {
            transform.position = pos;
        }
        else
        {
            rb.position = pos;
        }

        // Si jamais tu utilises ce script un jour avec un rigidbody non-kinematic,
        // tu peux aussi limiter la vitesse, mais là en kinematic tu gères la vitesse toi-même.
    }
}
