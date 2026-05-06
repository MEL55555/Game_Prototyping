using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using UnityEngine.Video; 
using System.Collections;

public class StoryManager : MonoBehaviour
{
  public static StoryManager Instance;
    
    // this stays true while the game is open so death skips the intro
    // but it resets when you close the app
    private static bool _sessionMonologueSeen = false;

    [Header("Protection Flags")]
    public bool canPause = false; 
    public bool canStartGame = false;

    [Header("Start UI")]
    public GameObject startPanel;
    public TextMeshProUGUI monologueText;
    
    [Header("3D World UI")]
    public TextMeshPro highScoreText; 

    [Header("End UI")]
    public GameObject endPanel;
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endMonologueText; 
    public GameObject endButtonsContainer;

    [Header("Cutscene Settings")]
    public VideoPlayer endVideoPlayer; 
    public GameObject videoUIContainer; 

    [Header("Settings")]
    public float targetScore = 20000f;
    public float continueBoostForce = 50f;
    public float slowMoDuration = 2.5f; 
    
    private bool _storyFinished = false;

    private string[] openingMonologue = {
        "…signal restored…",
        "Pilot, if you can hear this, you survived the crash.",
        "Your vessel went down beyond the mapped dunes.",
        "Navigation is gone. The storm is closing in.",
        "All we can track is your core momentum… keep moving.",
        "There may be a way out past the outer ridge.",
        "Do not stop. The dunes are not stable.",
        "We will guide you as long as the signal holds."
    };

    private string[] closingMonologue = {
        "…you made it further than expected…",
        "the signal is degrading… we are losing you…",
        "those readings… that is not just terrain…",
        "something is moving beneath the surface…",
        "you need to turn back… do you hear me…",
        "…no… keep going… it is the only way out…",
        "we cannot see the end anymore… only you can…",
        "…signal lost…"
    };

    void Awake() 
    { 
      Instance = this; 
    }

    void Start()
    {
        canPause = false;
        canStartGame = false;

        if (endButtonsContainer != null) endButtonsContainer.SetActive(false);
        if (videoUIContainer != null) videoUIContainer.SetActive(false); 

        RefreshHighScoreDisplay();

        // only show the long text if we havent seen it yet
        if (!_sessionMonologueSeen) 
        {
          StartCoroutine(PlayMonologue());
        }
        else
        {
            SkipToPrompt();
        }
    }

    private void SkipToPrompt()
    {
        // just shows the press space message instantly
        if (startPanel != null) startPanel.SetActive(true);
        if (monologueText != null) monologueText.text = ">> INITIATE DESCENT [SPACE/CLICK]";
        
        canStartGame = true; 
      Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator PlayMonologue()
    {
        if (startPanel != null) startPanel.SetActive(true);
        
        // types out the story line by line
        foreach (string line in openingMonologue)
        {
            if (monologueText != null) monologueText.text = line;
            yield return new WaitForSeconds(2.8f);
        }
        
        if (monologueText != null) monologueText.text = ">> INITIATE DESCENT [SPACE/CLICK]";
        
        _sessionMonologueSeen = true; 
        canStartGame = true; 
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartCinematicDescent()
    {
        // dont let them start until the text is finished
        if (!canStartGame) return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(SlowMoTransition());
    }

    IEnumerator SlowMoTransition()
    {
        // creates the cool slow motion effect when falling
        Time.timeScale = 0.1f;
        float elapsed = 0f;
        while (elapsed < slowMoDuration)
        {
            elapsed += Time.unscaledDeltaTime; 
            Time.timeScale = Mathf.Lerp(0.1f, 1f, elapsed / slowMoDuration);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        canPause = true; 
    }

    public void RestartGame() 
    {
        // resets the scene for another try
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CheckProgress(float currentScore)
    {
        // checks if you reached the goal to end the game
        if (!_storyFinished && currentScore >= targetScore) TriggerEnding();
    }

    void TriggerEnding()
    {
        _storyFinished = true;
        canPause = false;
        SaveHighScore();
        Time.timeScale = 0f; 
        
        var player = Object.FindFirstObjectByType<DunePlayer>();
        if (player != null) player.rb.simulated = false;

        if (endPanel != null) endPanel.SetActive(true);
        if (endTitleText != null) endTitleText.text = "CORE SYNCHRONIZED";
      StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        // plays the final words after winning
        if (endButtonsContainer != null) endButtonsContainer.SetActive(false);
        foreach (string line in closingMonologue)
        {
            if (endMonologueText != null) endMonologueText.text = line;
            yield return new WaitForSecondsRealtime(3f); 
        }

        // shows the buttons and gives mouse control back
        if (endButtonsContainer != null) endButtonsContainer.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartReturnCutscene()
    {
        if (endVideoPlayer == null) { RestartGame(); return; }
        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // handles playing the video at the very end
        if (endPanel != null) endPanel.SetActive(false);
        if (videoUIContainer != null) videoUIContainer.SetActive(true);

        endVideoPlayer.Prepare();
        while (!endVideoPlayer.isPrepared) yield return null;

        endVideoPlayer.Play();
        yield return new WaitForSecondsRealtime(1.0f); 
        while (endVideoPlayer.isPlaying) yield return null; 

        Time.timeScale = 1f; 
        SceneManager.LoadScene(0);
    }

    public void ContinueForever() 
    {
        // lets you keep playing after the credits
        Time.timeScale = 1f;
        canPause = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (endPanel != null) endPanel.SetActive(false);
        var player = Object.FindFirstObjectByType<DunePlayer>();
        if (player != null)
        {
            player.rb.simulated = true;
          player.rb.AddForce(new Vector2(continueBoostForce, continueBoostForce / 2f), ForceMode2D.Impulse);
        }
    }

    public void SaveHighScore()
    {
        // saves the best distance to the computer
        if (ScoreManager.Instance != null)
        {
            float currentScore = ScoreManager.Instance.GetScore();
            float savedHighScore = PlayerPrefs.GetFloat("HighScore", 0);
            if (currentScore > savedHighScore)
            {
                PlayerPrefs.SetFloat("HighScore", currentScore);
                PlayerPrefs.Save();
            }
        }
    }

    public void RefreshHighScoreDisplay()
    {
        // updates the text showing the max distance
        float highscore = PlayerPrefs.GetFloat("HighScore", 0);
        if (highScoreText != null) 
            highScoreText.text = "LAST KNOWN POSITION\n" + Mathf.FloorToInt(highscore) + " UNITS";
    }
}