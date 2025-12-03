using UnityEngine;
using System.Collections;

public class SignZoomInteraction : MonoBehaviour
{
    [Header("Zoom")]
    public Transform zoomPoint;          
    public float zoomDuration = 0.3f;
    public float zoomFOV = 30f;

    [Header("UI")]
    public GameObject promptUI;

    [Header("Scripts à désactiver")]
    public MonoBehaviour playerController;     // déplacement
    public MonoBehaviour playerCameraLook;     // rotation caméra !!!

    private bool playerInRange = false;
    private bool isZoomed = false;

    private Camera cam;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private float originalFOV;

    private void Start()
    {
        cam = Camera.main;
        if (promptUI != null) promptUI.SetActive(false);

        if (zoomPoint == null)
            Debug.LogWarning("ZoomPoint non assigné !");
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
            if (isZoomed) StartCoroutine(ZoomOut());
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomed) StartCoroutine(ZoomIn());
            else StartCoroutine(ZoomOut());
        }
    }

    private IEnumerator ZoomIn()
    {
        isZoomed = true;
        if (promptUI != null) promptUI.SetActive(false);

        // sauvegarde
        originalCamPos = cam.transform.position;
        originalCamRot = cam.transform.rotation;
        originalFOV = cam.fieldOfView;

        // désactiver scripts
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

        // réactivation
        if (playerController != null) playerController.enabled = true;
        if (playerCameraLook != null) playerCameraLook.enabled = true;

        if (playerInRange && promptUI != null)
            promptUI.SetActive(true);
    }
}
