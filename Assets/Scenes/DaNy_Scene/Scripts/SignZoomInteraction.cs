using UnityEngine;
using TMPro;
using System.Collections;

public class SignZoomInteraction : MonoBehaviour
{
    public Transform zoomPoint;          // Empty placé devant le panneau
    public float zoomDuration = 0.3f;    // Vitesse du zoom
    public float zoomFOV = 30f;          // Champ de vision pendant le zoom

    public GameObject promptUI;          // "Appuyez sur E pour lire"

    private bool playerInRange = false;
    private bool isZoomed = false;

    private Camera cam;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private float originalFOV;

    // Optionnel : ton script de contrôle joueur à désactiver pendant la lecture
    public MonoBehaviour playerController;

    private void Start()
    {
        cam = Camera.main;
        if (promptUI != null) promptUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isZoomed && promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
            if (isZoomed)
                StartCoroutine(ZoomOut());
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
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
        if (promptUI != null) promptUI.SetActive(false);

        // Sauvegarde état caméra
        originalCamPos = cam.transform.position;
        originalCamRot = cam.transform.rotation;
        originalFOV = cam.fieldOfView;

        // Optionnel : désactiver contrôle joueur
        if (playerController != null)
            playerController.enabled = false;

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

        // Réactiver le contrôle joueur
        if (playerController != null)
            playerController.enabled = true;

        if (playerInRange && promptUI != null)
            promptUI.SetActive(true);
    }
}
