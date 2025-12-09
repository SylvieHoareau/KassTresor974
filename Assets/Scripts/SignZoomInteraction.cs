using UnityEngine;
using System.Collections;

public class SignZoomInteraction : MonoBehaviour
{
    [Header("Zoom")]
    public Transform zoomPoint;
    public float zoomDuration = 0.3f;
    public float zoomFOV = 30f;

    [Header("UI")]
    public PromptUIAnimator promptUI;     // <- ANIMATION du texte d'interaction

    [Header("Scripts à désactiver")]
    public MonoBehaviour playerController;  // déplacement joueur
    public MonoBehaviour playerCameraLook;  // rotation caméra (3ème personne)

    private bool playerInRange = false;
    private bool isZoomed = false;

    private Camera cam;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private float originalFOV;

    private void Start()
    {
        cam = Camera.main;

        // prompt masqué avec anim
        if (promptUI != null)
            promptUI.Hide();

        if (zoomPoint == null)
            Debug.LogWarning("ZoomPoint non assigné !");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isZoomed && promptUI != null)
                promptUI.Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.Hide();

            if (isZoomed)
                StartCoroutine(ZoomOut());
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomed)
                StartCoroutine(ZoomIn());
            else
                StartCoroutine(ZoomOut());
        }
    }

    private IEnumerator ZoomIn()
    {
        isZoomed = true;

        // cacher le prompt
        if (promptUI != null)
            promptUI.Hide();

        // sauvegarde caméra
        originalCamPos = cam.transform.position;
        originalCamRot = cam.transform.rotation;
        originalFOV = cam.fieldOfView;

        // désactiver contrôles joueur
        if (playerController != null) playerController.enabled = false;
        if (playerCameraLook != null) playerCameraLook.enabled = false;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / zoomDuration;
            cam.transform.position = Vector3.Lerp(originalCamPos, zoomPoint.position, t);
            cam.transform.rotation = Quaternion.Slerp(originalCamRot, zoomPoint.rotation, t);
            cam.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        isZoomed = false;

        float startFOV = cam.fieldOfView;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / zoomDuration;
            cam.transform.position = Vector3.Lerp(startPos, originalCamPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, originalCamRot, t);
            cam.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            yield return null;
        }

        // réactivation mouvement joueur
        if (playerController != null) playerController.enabled = true;
        if (playerCameraLook != null) playerCameraLook.enabled = true;

        // remettre prompt si le joueur est encore dans la zone
        if (playerInRange && promptUI != null)
            promptUI.Show();
        else if (promptUI != null)
            promptUI.Hide();
    }
}
