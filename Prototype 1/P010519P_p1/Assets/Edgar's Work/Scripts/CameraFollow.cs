using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Stuff")]
    public Transform target; // the player we are following
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.5f;
    public float dampingSpeed = 1.5f;

    private Vector3 initialPosition;
    private float currentShakeDuration = 0f;

    void Start()
    {
        // make sure we actually have a player to look at
        if (target == null) return;
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // make the camera follow the player but not too fast
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // if something exploded or we hit something, shake the screen
        if (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0; // dont mess with the Z axis or we might go inside the level
            smoothedPosition += shakeOffset;

            // timer for the shake
            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }

        transform.position = smoothedPosition;
    }

    // trigger this from other scripts when something cool happens
    public void ShakeCamera(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}