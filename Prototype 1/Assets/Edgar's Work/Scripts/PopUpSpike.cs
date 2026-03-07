using UnityEngine;
using System.Collections;

public class PopUpSpike : MonoBehaviour
{
    [Header("Spike Object")]
    public GameObject spikeObject;

    [Header("Trigger Requirement")]
    public bool requirePreviousTrigger = false;
    public TriggerZone requiredTrigger;

    [Header("Spawn Delay")]
    [Tooltip("Delay in seconds before the spike appears after triggering.")]
    public float spawnDelay = 0f;

    private bool hasTriggered = false;
    Coroutine spawnCoroutine;

    void OnEnable()
    {
        PlayerController.OnPlayerRespawn += ResetTrap;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerRespawn -= ResetTrap;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (requirePreviousTrigger)
        {
            if (requiredTrigger == null) return;
            if (!requiredTrigger.activated) return;
        }

        spawnCoroutine = StartCoroutine(ActivateSpikeWithDelay());
    }

    IEnumerator ActivateSpikeWithDelay()
    {
        hasTriggered = true;

        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        if (spikeObject != null)
            spikeObject.SetActive(true);
    }

    void ResetTrap()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        hasTriggered = false;

        if (spikeObject != null)
            spikeObject.SetActive(false);
    }
}