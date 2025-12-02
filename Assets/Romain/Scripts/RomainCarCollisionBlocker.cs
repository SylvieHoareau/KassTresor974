using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RomainCarResetZone : MonoBehaviour
{
    [Header("Point de spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Layers interdits (TP instantanée)")]
    [SerializeField] private LayerMask instantResetLayers;

    [Header("Layers avec avertissement + timer")]
    [SerializeField] private LayerMask delayedResetLayers;
    [SerializeField] private float delayBeforeReset = 3f;

    [Header("UI d'avertissement")]
    [Tooltip("Un panel avec un texte 'Retourne dans la zone de jeu !'")]
    [SerializeField] private GameObject warningUI;

    private Rigidbody rb;
    private Coroutine resetRoutine;
    private bool isResetting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (warningUI != null)
            warningUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // Cas 1 : TP immédiate
        if (IsInLayerMask(otherLayer, instantResetLayers))
        {
            InstantReset();
            return;
        }

        // Cas 2 : avertissement + timer
        if (IsInLayerMask(otherLayer, delayedResetLayers))
        {
            StartDelayedReset();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // Si on sort d'une zone "delayed", on annule le reset
        if (IsInLayerMask(otherLayer, delayedResetLayers))
        {
            CancelDelayedReset();
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void InstantReset()
    {
        CancelDelayedReset(); // au cas où
        ResetCarToSpawn();
    }

    private void StartDelayedReset()
    {
        if (isResetting) return;

        isResetting = true;

        if (resetRoutine != null)
            StopCoroutine(resetRoutine);

        resetRoutine = StartCoroutine(DelayedResetCoroutine());
    }

    private void CancelDelayedReset()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        isResetting = false;

        if (warningUI != null)
            warningUI.SetActive(false);
    }

    private IEnumerator DelayedResetCoroutine()
    {
        if (warningUI != null)
            warningUI.SetActive(true);

        float timer = delayBeforeReset;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (warningUI != null)
            warningUI.SetActive(false);

        ResetCarToSpawn();

        isResetting = false;
        resetRoutine = null;
    }

    private void ResetCarToSpawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("RomainCarResetZone : Aucun spawnPoint assigné.");
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}
