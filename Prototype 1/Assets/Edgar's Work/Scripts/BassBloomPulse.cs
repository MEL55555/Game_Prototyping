using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BassPostProcessPulse : MonoBehaviour
{
    [Header("Audio & Effects")]
    public AudioSource music;   // The music audio source
    public Volume volume;       // Post-processing volume reference

    [Header("Bloom Settings")]
    public bool bloomEnabled = true;
    public float bloomMultiplier = 40f;
    public float bloomThreshold = 0.09f;
    public float bloomExponent = 1.2f;

    [Header("Chromatic Aberration Settings")]
    public bool chromaticEnabled = true;
    public float chromaticMultiplier = 1.4f;
    public float chromaticThreshold = 0.01f;
    public float chromaticExponent = 0.9f;

    [Header("Lens Distortion Settings")]
    public bool lensDistortionEnabled = true;
    public float lensDistortionMultiplier = 0.3f;
    public float lensThreshold = 0.07f;
    public float lensExponent = 1f;

    [Header("Camera Shake Settings")]
    public bool shakeEnabled = true;
    public Camera targetCamera;
    public float shakeMultiplier = 0.02f;
    public float shakeThreshold = 0.02f;
    public float shakeExponent = 1.2f;
    public float shakeSpeed = 12f;

    [Header("Frequency Bands Settings")]
    public int deepBassCount = 3;
    public int midBassCount = 4;
    public int highMidCount = 5;

    [Header("Global Settings")]
    public float audioMultiplier = 0.5f;  // Scales the FFT spectrum
    public float smoothSpeed = 8f;        // Smooth factor for pulsing

    private float[] spectrum = new float[64];  // Audio spectrum buffer
    private float pulse;                       // Smoothed pulse value

    // Post-processing references
    private Bloom bloom;
    private ChromaticAberration chromatic;
    private LensDistortion lensDistortion;

    // Default intensities to reset effects when no music is playing
    private float bloomDefault;
    private float chromaticDefault;
    private float lensDefault;

    void Start()
    {
        // Get post-processing effects from volume
        if (volume != null)
        {
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out chromatic);
            volume.profile.TryGet(out lensDistortion);
        }

        // Store default values
        if (bloom != null) bloomDefault = bloom.intensity.value;
        if (chromatic != null) chromaticDefault = chromatic.intensity.value;
        if (lensDistortion != null) lensDefault = lensDistortion.intensity.value;
    }

    void Update()
    {
        if (music == null || !music.isPlaying)
        {
            ResetEffects(); // Reset if no music
            return;
        }

        // --- Get audio spectrum ---
        music.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);

        // --- Calculate frequency band pulses ---
        float deepBass = SumRange(spectrum, 0, deepBassCount);
        float midBass = SumRange(spectrum, deepBassCount, midBassCount);
        float highMid = SumRange(spectrum, deepBassCount + midBassCount, highMidCount);

        // Weighted combined pulse
        float combinedPulse = (deepBass * 1f + midBass * 0.7f + highMid * 0.5f) * audioMultiplier;

        // Smooth pulse to avoid jitter
        pulse = Mathf.Lerp(pulse, combinedPulse, Time.deltaTime * smoothSpeed);

        // Apply effects
        ApplyEffect(bloom, bloomEnabled, pulse, bloomThreshold, bloomExponent, bloomMultiplier, val => bloom.intensity.value = val, bloomDefault);
        ApplyChromatic(chromatic, chromaticEnabled, pulse, chromaticThreshold, chromaticExponent, chromaticMultiplier, chromaticDefault);
        ApplyEffect(lensDistortion, lensDistortionEnabled, pulse, lensThreshold, lensExponent, lensDistortionMultiplier, val => lensDistortion.intensity.value = val, lensDefault);

        // Camera shake based on audio
        if (targetCamera != null && shakeEnabled)
        {
            ApplyCameraShake(pulse);
        }
    }

    // --- Helper: Sum a range of the spectrum ---
    private float SumRange(float[] data, int start, int count)
    {
        float sum = 0f;
        for (int i = start; i < start + count && i < data.Length; i++)
            sum += data[i];
        return sum;
    }

    // --- Apply generic effect (Bloom or Lens) ---
    private void ApplyEffect<T>(T effect, bool enabled, float pulse, float threshold, float exponent, float multiplier, System.Action<float> applyValue, float defaultValue)
    {
        if (effect != null && enabled)
        {
            float value = Mathf.Max(pulse - threshold, 0f);
            value = Mathf.Pow(value, exponent);
            applyValue(defaultValue + value * multiplier);
        }
    }

    // --- Special handler for Chromatic Aberration ---
    private void ApplyChromatic(ChromaticAberration effect, bool enabled, float pulse, float threshold, float exponent, float multiplier, float defaultValue)
    {
        if (effect != null && enabled)
        {
            float value = Mathf.Max(pulse - threshold, 0f);
            value = Mathf.Pow(value, exponent);
            float finalValue = Mathf.Max(value * multiplier, 0.01f); // minimal movement
            effect.intensity.value = defaultValue + Mathf.PingPong(finalValue, 1f);
        }
    }

    // --- Camera shake logic ---
    private void ApplyCameraShake(float pulse)
    {
        float shakePulse = Mathf.Max(pulse - shakeThreshold, 0f);
        if (shakePulse <= 0f) return;

        float shakeAmount = Mathf.Pow(shakePulse, shakeExponent) * shakeMultiplier;
        Vector3 shakeOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * 2f,
            0f
        ) * shakeAmount;

        targetCamera.transform.localPosition += shakeOffset;
    }

    // --- Reset all effects to their defaults ---
    void ResetEffects()
    {
        if (bloom != null) bloom.intensity.value = bloomDefault;
        if (chromatic != null) chromatic.intensity.value = chromaticDefault;
        if (lensDistortion != null) lensDistortion.intensity.value = lensDefault;
    }
}