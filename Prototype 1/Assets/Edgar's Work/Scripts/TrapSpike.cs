using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public Transform player;
    public float detectionRadius = 3f;

    [Header("Movement")]
    public Vector3 targetPosition;
    public float moveSpeed = 5f;

    [Header("Optional External Trigger")]
    public bool requireTriggerZone = false;
    public TriggerZone triggerZone;

    private bool hasTriggered = false;

    void Update()
    {
        if (player == null) return;

        // STOP here if the spike needs a trigger zone and it hasn't activated yet
        if (requireTriggerZone)
        {
            if (triggerZone == null) return;
            if (!triggerZone.activated) return;
        }

        // Once triggered, move the spike
        if (hasTriggered)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            hasTriggered = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}