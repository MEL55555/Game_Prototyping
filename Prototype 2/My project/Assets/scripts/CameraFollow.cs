using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Positioning")]
    // Increasing Y-offset pushes the player lower on the screen, 
    // revealing the bottom of the valleys.
    public Vector3 offset = new Vector3(12f, 5f, -10f); 

    [Header("Dynamic Zoom Settings")]
    public Camera cam;
    public float minZoom = 22f;         // MUCH wider starting view
    public float maxZoom = 55f;         // Massive zoom for high altitudes
    public float zoomThreshold = 12f;   // Won't zoom out more until you pass Y=12
    public float zoomMultiplier = 1.0f; 
    public float zoomSpeed = 1.5f;      // Slower speed makes the wide view feel stable

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic) cam.orthographic = true;
        
        // Snap immediately to the wide view on start
        cam.orthographicSize = minZoom;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Follow X
            Vector3 targetPosition = transform.position;
            targetPosition.x = target.position.x + offset.x; 

            // 2. Vertical Tracking
            // We follow the player upward, but the camera "floor" 
            // is offset.y to keep the ground visible.
            float targetY = Mathf.Max(target.position.y + offset.y, offset.y);
            targetPosition.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f); 
            
            transform.position = targetPosition;

            // 3. Wide-to-Ultra Zoom Logic
            float height = target.position.y;
            float targetZoom = minZoom;

            if (height > zoomThreshold)
            {
                // Only starts increasing zoom once you are high in the air
                targetZoom = minZoom + ((height - zoomThreshold) * zoomMultiplier);
            }
            
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom); 

            // Apply zoom smoothly
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }
}