using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioDistortionFilter))]
public class DynamicMusicManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicZone
    {
        public float minScore;
        public float maxScore;
        public AudioClip musicClip;
    }

    [Header("Music Zones")]
    public List<MusicZone> musicZones = new List<MusicZone>();

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Fade Settings")]
    public float fadeDuration = 2.5f;
    public float maxVolume = 0.6f;

    [Header("Distortion Settings")]
    public float maxDistortion = 0.7f;
    public float pitchVariance = 0.2f;
    public float minLowPass = 800f;
    public float maxLowPass = 22000f;

    [Header("Score Settings")]
    public float maxScore = 50000f;

    private AudioSource currentSource;
    private AudioSource nextSource;

    private MusicZone currentZone;
    private bool isTransitioning = false;

    private AudioLowPassFilter lowPass;
    private AudioDistortionFilter distortion;

    void Start()
    {
        // init the two-source system for seamless crossfading
        currentSource = sourceA;
        nextSource = sourceB;

        currentSource.volume = 0f;
        nextSource.volume = 0f;

        // grab filter components for the dynamic distortion
        lowPass = GetComponent<AudioLowPassFilter>();
        distortion = GetComponent<AudioDistortionFilter>();
    }

    void Update()
    {
        if (ScoreManager.Instance == null) return;

        float score = ScoreManager.Instance.GetScore();
        float scorePercent = Mathf.Clamp01(score / maxScore);

        // check if player has entered a new score bracket for music
        MusicZone targetZone = GetZoneForScore(score);
        if (targetZone != null && targetZone != currentZone && !isTransitioning)
        {
            StartCoroutine(CrossfadeTo(targetZone));
        }

        // update filters based on current progression
        ApplyAudioEffects(scorePercent);
    }

    MusicZone GetZoneForScore(float score)
    {
        // find which clip belongs to the current score range
        foreach (var zone in musicZones)
        {
            if (score >= zone.minScore && score < zone.maxScore)
                return zone;
        }
        return null;
    }

    IEnumerator CrossfadeTo(MusicZone newZone)
    {
        isTransitioning = true;
        currentZone = newZone;

        // prep the standby source with the new clip
        nextSource.clip = newZone.musicClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // lerp volumes in opposite directions for the fade
            currentSource.volume = Mathf.Lerp(maxVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, maxVolume, t);

            yield return null;
        }

        currentSource.Stop();

        // swap references so currentSource is always the audible one
        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        isTransitioning = false;
    }

    void ApplyAudioEffects(float scorePercent)
    {
        // scale distortion level based on game difficulty/score
        distortion.distortionLevel = Mathf.Lerp(0f, maxDistortion, scorePercent);

        // filter out high frequencies as the score climbs
        lowPass.cutoffFrequency = Mathf.Lerp(maxLowPass, minLowPass, scorePercent);

        // introduce random pitch flutter to make it sound a bit more grim near the end
        float pitchShift = Random.Range(-pitchVariance, pitchVariance) * scorePercent;
        currentSource.pitch = 1f + pitchShift;
    }
}