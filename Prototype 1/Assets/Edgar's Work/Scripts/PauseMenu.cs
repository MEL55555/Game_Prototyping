using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuWithSettings : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    [Header("Buttons")]
    public Button firstPauseButton;
    public Button firstSettingsButton;

    [Header("Audio")]
    public AudioSource gameMusic;
    public AudioSource pauseMusic;
    public Slider musicVolumeSlider;

    [Header("Post Processing")]
    public BassPostProcessPulse bassVisualizer;

    [Header("Toggles")]
    public Toggle bloomToggle;
    public Toggle chromaticToggle;
    public Toggle lensToggle;
    public Toggle vignetteToggle;
    public Toggle filmGrainToggle;

    [Header("Disable When Paused")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("Custom Cursor")]
    public CustomCursor customCursor;

    bool isPaused = false;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (bassVisualizer != null)
        {
            bloomToggle.isOn = bassVisualizer.bloomEnabled;
            chromaticToggle.isOn = bassVisualizer.chromaticEnabled;
            lensToggle.isOn = bassVisualizer.lensDistortionEnabled;
            vignetteToggle.isOn = bassVisualizer.vignetteEnabled;
            filmGrainToggle.isOn = bassVisualizer.filmGrainEnabled;
        }

        if (musicVolumeSlider != null && gameMusic != null)
            musicVolumeSlider.value = gameMusic.volume;

        if (bloomToggle != null)
            bloomToggle.onValueChanged.AddListener(v => bassVisualizer.bloomEnabled = v);

        if (chromaticToggle != null)
            chromaticToggle.onValueChanged.AddListener(v => bassVisualizer.chromaticEnabled = v);

        if (lensToggle != null)
            lensToggle.onValueChanged.AddListener(v => bassVisualizer.lensDistortionEnabled = v);

        if (vignetteToggle != null)
            vignetteToggle.onValueChanged.AddListener(v => bassVisualizer.vignetteEnabled = v);

        if (filmGrainToggle != null)
            filmGrainToggle.onValueChanged.AddListener(v => bassVisualizer.filmGrainEnabled = v);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(v => gameMusic.volume = v);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        foreach (var script in scriptsToDisable)
            script.enabled = true;

        if (gameMusic != null) gameMusic.UnPause();
        if (pauseMusic != null) pauseMusic.Stop();

        if (bassVisualizer != null)
            bassVisualizer.freezeEffects = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (customCursor != null)
            customCursor.HideCursor();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        settingsMenuUI.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        foreach (var script in scriptsToDisable)
            script.enabled = false;

        if (gameMusic != null) gameMusic.Pause();
        if (pauseMusic != null) pauseMusic.Play();

        if (bassVisualizer != null)
            bassVisualizer.freezeEffects = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (customCursor != null)
            customCursor.ShowCursor();

        if (firstPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
        }
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);

        if (firstSettingsButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton.gameObject);
        }
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        if (firstPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}