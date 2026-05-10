using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 _originalPos;

    void Awake() => Instance = this;

    public void Shake(float duration, float intensity)
    {
        // cache original position to ensure we can snap back after
        _originalPos = transform.localPosition;

        // kill any active shakes to avoid offset stacking
        StopAllCoroutines();
        StartCoroutine(ProcessShake(duration, intensity));
    }

    private IEnumerator ProcessShake(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // generate random offsets based on intensity strength
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            // apply displacement to local space
            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // reset transform to prevent permanent camera drift
        transform.localPosition = _originalPos;
    }
}