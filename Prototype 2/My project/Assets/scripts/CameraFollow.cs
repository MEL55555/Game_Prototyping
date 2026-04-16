using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(12f, 5f, -10f); 

    public Camera cam;
    public float minZoom = 22f;         
    public float maxZoom = 55f;         
    public float zoomThreshold = 12f;   
    public float zoomMultiplier = 1.0f; 
    public float zoomSpeed = 1.5f;      

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // make sure we use ortho mode
        if (!cam.orthographic) cam.orthographic = true;
        
        cam.orthographicSize = minZoom;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // follow the player horizontally
            Vector3 targetPosition = transform.position;
            targetPosition.x = target.position.x + offset.x; 

            // keep the camera above a certain height
            float targetY = Mathf.Max(target.position.y + offset.y, offset.y);
            targetPosition.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f); 
            
            transform.position = targetPosition;

            // zoom out as the player goes higher
            float height = target.position.y;
            float targetZoom = minZoom;

            if (height > zoomThreshold)
            {
                targetZoom = minZoom + ((height - zoomThreshold) * zoomMultiplier);
            }
            
            // keep zoom within limits
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom); 

            // smooth zoom change
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }
}