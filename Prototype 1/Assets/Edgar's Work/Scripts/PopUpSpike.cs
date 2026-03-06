using UnityEngine;
using System.Collections;

public class PopUpSpike : MonoBehaviour
{
    [Header("Spike Object")]
    public GameObject spikeObject; // the spike that appears

    [Header("Trigger Requirement")]
    public bool requirePreviousTrigger = false;
    public TriggerZone requiredTrigger;

    [Header("Spawn Delay")]
    [Tooltip("Delay in seconds before the spike appears after triggering.")]
    public float spawnDelay = 0f; // NEW: delay time

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // If a previous trigger is required but hasn't activated yet
        if (requirePreviousTrigger)
        {
            if (requiredTrigger == null) return;
            if (!requiredTrigger.activated) return;
        }

        StartCoroutine(ActivateSpikeWithDelay());
    }

    private IEnumerator ActivateSpikeWithDelay()
    {
        hasTriggered = true;

        // Wait for the specified delay
        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        // Activate the spike
        if (spikeObject != null)
            spikeObject.SetActive(true);
    }
}