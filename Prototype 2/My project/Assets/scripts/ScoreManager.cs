using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;

    [Header("Dialogue UI")]
    public TextMeshProUGUI dialogueText;

    [System.Serializable]
    public class DialogueEntry
    {
        public float scoreTrigger;

        [TextArea(2, 4)]
        public string message;
    }

    public List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.03f;
    public float displayTime = 2.5f;
    public float fadeSpeed = 2f;

    [Header("Glitch")]
    public bool enableGlitch = true;
    public string glitchChars = "@#$%&*?!";

    [Header("Typing Sound")]
    public bool enableTypingSound = true;
    public AudioSource audioSource;
    public AudioClip typingClip;
    public float pitchVariation = 0.1f;

    [Header("Radio Audio")]
    public AudioSource radioSource;
    public AudioClip cleanRadio;
    public AudioClip staticRadio;

    [Range(0f, 1f)]
    public float distortionAmount;

    public float radioFadeSpeed = 2f; // NEW (controls fade smoothness)

    [Header("Altitude Thresholds")]
    public float heightStep = 10f; 
    public float startMultiplierY = 5f; 
    
    [Header("Scoring Math")]
    public float pointsPerUnit = 1f; 
    public int maxMultiplier = 10;

    private float currentScore = 0f;
    private Transform player;
    private int currentMultiplier = 1;

    private int dialogueIndex = 0;
    private Coroutine dialogueRoutine;
    private Coroutine radioFadeRoutine;

    void Awake() { Instance = this; }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (dialogueText != null)
        {
            Color c = dialogueText.color;
            c.a = 0;
            dialogueText.color = c;
        }

        if (radioSource != null)
        {
            radioSource.volume = 0f; // start silent
        }
    }

    void Update()
    {
        if (player == null) return;

        float scorePercent = currentScore / 50000f;
        distortionAmount = Mathf.Clamp01(scorePercent);

        // MULTIPLIER
        if (player.position.y > startMultiplierY)
        {
            float extraHeight = player.position.y - startMultiplierY;
            currentMultiplier = 1 + Mathf.FloorToInt(extraHeight / heightStep);
        }
        else currentMultiplier = 1;

        currentMultiplier = Mathf.Clamp(currentMultiplier, 1, maxMultiplier);

        // SCORE
        float speed = player.GetComponent<Rigidbody2D>().linearVelocity.x;
        if (speed > 0.5f) 
        {
            currentScore += speed * currentMultiplier * pointsPerUnit * Time.deltaTime;
        }

        scoreText.text = Mathf.FloorToInt(currentScore).ToString();

        if (currentMultiplier > 1)
        {
            multiplierText.text = "x" + currentMultiplier;
            UpdateMultiplierVisuals();
        }
        else multiplierText.text = "";

        CheckDialogue();
    }

    void CheckDialogue()
    {
        if (dialogueIndex >= dialogueList.Count) return;

        if (currentScore >= dialogueList[dialogueIndex].scoreTrigger)
        {
            if (dialogueRoutine != null)
                StopCoroutine(dialogueRoutine);

            dialogueRoutine = StartCoroutine(PlayDialogue(dialogueList[dialogueIndex].message));
            dialogueIndex++;
        }
    }

    IEnumerator PlayDialogue(string message)
    {
        dialogueText.text = "";
        SetAlpha(1f);

        StartRadio(); // 🔊 fade in radio

        foreach (char letter in message)
        {
            float dynamicGlitch = Mathf.Lerp(0.05f, 0.4f, distortionAmount);

            if (enableGlitch && Random.value < dynamicGlitch)
            {
                char randomChar = glitchChars[Random.Range(0, glitchChars.Length)];
                dialogueText.text += randomChar;

                yield return new WaitForSeconds(typingSpeed * 0.5f);

                dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - 1);
            }

            dialogueText.text += letter;

            if (enableTypingSound && audioSource != null && typingClip != null && letter != ' ')
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(typingClip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayTime);

        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        dialogueText.text = "";

        StopRadio(); // 🔊 fade out radio
    }

    void StartRadio()
    {
        if (radioSource == null) return;

        radioSource.clip = distortionAmount < 0.5f ? cleanRadio : staticRadio;
        radioSource.pitch = Random.Range(0.9f - distortionAmount, 1.1f + distortionAmount);

        if (!radioSource.isPlaying)
            radioSource.Play();

        if (radioFadeRoutine != null)
            StopCoroutine(radioFadeRoutine);

        radioFadeRoutine = StartCoroutine(FadeRadio(0.8f));
    }

    void StopRadio()
    {
        if (radioSource == null) return;

        if (radioFadeRoutine != null)
            StopCoroutine(radioFadeRoutine);

        radioFadeRoutine = StartCoroutine(FadeRadio(0f));
    }

    IEnumerator FadeRadio(float targetVolume)
    {
        while (!Mathf.Approximately(radioSource.volume, targetVolume))
        {
            radioSource.volume = Mathf.MoveTowards(
                radioSource.volume,
                targetVolume,
                radioFadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (targetVolume == 0f)
            radioSource.Stop();
    }

    void SetAlpha(float a)
    {
        if (dialogueText == null) return;

        Color c = dialogueText.color;
        c.a = Mathf.Clamp01(a);
        dialogueText.color = c;
    }

    void UpdateMultiplierVisuals()
    {
        if (currentMultiplier >= 8) multiplierText.color = Color.cyan; 
        else if (currentMultiplier >= 5) multiplierText.color = Color.yellow; 
        else multiplierText.color = Color.white; 
    }

    public void AddBonusPoints(int amount, int streak)
    {
        currentScore += (amount * streak) * currentMultiplier;
    }

    public float GetScore()
    {
        return currentScore;
    }
}