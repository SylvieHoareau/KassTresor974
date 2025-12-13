using UnityEngine;
using TMPro;

public class RomainCreditTrigger : MonoBehaviour
{
    public RomainCreditsFader manager;
    public TMP_Text creditText;

    [Tooltip("Empêche de retrigger 50 fois si tu restes dedans.")]
    public bool triggerOnce = true;

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        manager?.TriggerCredit(creditText);
    }
}
