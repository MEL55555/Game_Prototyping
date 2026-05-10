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

        // force ortho mode on start so it doesn't look wonky
        if (!cam.orthographic) cam.orthographic = true;

        cam.orthographicSize = minZoom;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // update camera x to follow target with a set offset
            Vector3 targetPosition = transform.position;
            targetPosition.x = target.position.x + offset.x;

            // track height but cap the lowest point it can go
            float targetY = Mathf.Max(target.position.y + offset.y, offset.y);
            targetPosition.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f);

            transform.position = targetPosition;

            // calc zoom based on player altitude
            float height = target.position.y;
            float targetZoom = minZoom;

            // only kick in extra zoom once they pass the threshold height
            if (height > zoomThreshold)
            {
                targetZoom = minZoom + ((height - zoomThreshold) * zoomMultiplier);
            }

            // clamp values so it stays within bounds
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            // smooth interpolation for the lens zoom
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }
}