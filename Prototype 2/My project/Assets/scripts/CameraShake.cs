using UnityEngine;
using System.Collections; 

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 _originalPos;

    void Awake() => Instance = this;

    public void Shake(float duration, float intensity)
    {
        // save where the camera was before shaking
        _originalPos = transform.localPosition;
        StopAllCoroutines();
        StartCoroutine(ProcessShake(duration, intensity));
    }

    private IEnumerator ProcessShake(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // pick a random spot nearby
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        // put the camera back exactly where it started
        transform.localPosition = _originalPos;
    }
}