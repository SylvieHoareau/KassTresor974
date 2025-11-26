using UnityEngine;

public class RomainCameraFollow : MonoBehaviour
{
    [Header("Cible à suivre (met ton CameraTarget ici)")]
    public Transform target;

    [Header("Axes à suivre")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = false;

    [Header("Déplacement caméra")]
    public float smoothSpeed = 0.15f;
    public Vector3 offset;

    [Header("Dead Zone (tolérance avant que la caméra bouge)")]
    public Vector2 deadZone = new Vector2(0.3f, 0.2f);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Position désirée (avant dead zone)
        Vector3 desiredPosition = transform.position;

        Vector3 targetPos = target.position + offset;

        // Calcul des différences
        Vector3 diff = targetPos - transform.position;

        // Dead zone sur X
        if (followX)
        {
            if (Mathf.Abs(diff.x) > deadZone.x)
                desiredPosition.x = targetPos.x;
        }

        // Dead zone sur Y
        if (followY)
        {
            if (Mathf.Abs(diff.y) > deadZone.y)
                desiredPosition.y = targetPos.y;
        }

        // Suivi éventuel du Z
        if (followZ)
        {
            desiredPosition.z = targetPos.z;
        }

        // Lissage
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothSpeed
        );
    }
}
