using UnityEngine;

public class BassPulse : MonoBehaviour
{
    public AudioSource music;
    public float pulseMultiplier = 30f;
    public float scaleAmount = 0.3f;
    public float smoothSpeed = 8f;

    private float[] spectrum = new float[64];
    private Vector3 originalScale;
    private float currentPulse;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Get audio spectrum data
        music.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);

        // Bass is in the first few frequencies
        float bass = 0f;
        for (int i = 0; i < 5; i++)
        {
            bass += spectrum[i];
        }

        bass *= pulseMultiplier;

        // Smooth the pulse
        currentPulse = Mathf.Lerp(currentPulse, bass, Time.deltaTime * smoothSpeed);

        // Apply scale
        transform.localScale = originalScale * (1 + currentPulse * scaleAmount);
    }
}