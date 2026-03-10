using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class DeathTaunt
{
    public AudioClip voiceLine;
    [TextArea(2,4)]
    public string screenText;
}

public class DeathTauntSystem : MonoBehaviour
{
    [Header("Taunts")]
    public DeathTaunt[] taunts;

    [Header("Audio Sources")]
    public AudioSource tauntAudioSource;      // For voice lines
    public AudioSource explosionAudioSource;  // For death explosion
    public AudioClip deathExplosionSound;     // Explosion clip

    [Header("UI")]
    public TextMeshProUGUI tauntText;

    [Header("Typing Settings")]
    [Range(0.01f,0.2f)]
    public float typingSpeed = 0.05f;
    [Range(1f,10f)]
    public float displayDuration = 3f;

    [Header("Player")]
    public PlayerController player;

    void Start()
    {
        ClearText();
        if (player != null)
            player.OnPlayerDeath += HandleDeath;
        else
            Debug.LogError("Player not assigned to DeathTauntSystem!");

        // Subscribe to the static respawn event via class
        PlayerController.OnPlayerRespawn += PlayRandomTaunt;
    }

    void OnDestroy()
    {
        if (player != null)
            player.OnPlayerDeath -= HandleDeath;

        // Unsubscribe from static event
        PlayerController.OnPlayerRespawn -= PlayRandomTaunt;
    }

    void HandleDeath()
    {
        if (explosionAudioSource != null && deathExplosionSound != null)
            explosionAudioSource.PlayOneShot(deathExplosionSound, 1f); // louder
    }

    public void PlayRandomTaunt()
    {
        if (taunts.Length == 0) return;

        int index = Random.Range(0, taunts.Length);
        DeathTaunt chosen = taunts[index];

        if (tauntAudioSource != null && chosen.voiceLine != null)
        {
            tauntAudioSource.pitch = Random.Range(0.95f, 1.05f);
            tauntAudioSource.PlayOneShot(chosen.voiceLine, 3f);
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(chosen.screenText));
    }

    IEnumerator TypeText(string message)
    {
        if (tauntText == null) yield break;

        tauntText.gameObject.SetActive(true);
        tauntText.text = "";

        foreach (char letter in message)
        {
            tauntText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);
        ClearText();
    }

    void ClearText()
    {
        if (tauntText != null)
        {
            tauntText.text = "";
            tauntText.gameObject.SetActive(false);
        }
    }
}