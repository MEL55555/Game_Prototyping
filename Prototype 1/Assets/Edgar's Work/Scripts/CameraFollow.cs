using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // The player
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.5f;
    public float dampingSpeed = 1.5f;

    private Vector3 initialPosition;
    private float currentShakeDuration = 0f;

    void Start()
    {
        if (target == null) return;
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Smooth follow
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Apply camera shake if active
        if (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0; // keep camera in correct Z
            smoothedPosition += shakeOffset;

            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }

        transform.position = smoothedPosition;
    }

    // Call this method to trigger a shake
    public void ShakeCamera(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}