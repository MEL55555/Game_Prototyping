using UnityEngine;

public class MovingPlatformTrigger : MonoBehaviour
{
    [Header("Platform To Move")]
    public Transform platform;

    [Header("Movement Settings")]
    public Vector3 targetPosition;
    public float moveSpeed = 3f;

    private bool triggered = false;

    void Update()
    {
        // If triggered, move the platform
        if (triggered && platform != null)
        {
            platform.position = Vector3.MoveTowards(
                platform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
        }
    }
}