using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    // INPUT SYSTEM
    InputAction pauseAction;
    InputAction backAction;

    void OnEnable()
    {
        // PAUSE (Esc, P, Start)
        pauseAction = new InputAction(type: InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Keyboard>/p");
        pauseAction.AddBinding("<Gamepad>/start");

        pauseAction.performed += ctx =>
        {
            if (isPaused)
                Resume();
            else
                Pause();
        };

        // BACK (B / Circle)
        backAction = new InputAction(type: InputActionType.Button);
        backAction.AddBinding("<Gamepad>/buttonEast");

        backAction.performed += ctx =>
        {
            if (settingsMenuUI.activeSelf)
            {
                CloseSettings();
            }
        };

        pauseAction.Enable();
        backAction.Enable();
    }

    void OnDisable()
    {
        pauseAction.Disable();
        backAction.Disable();
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        LoadSettings();
        ApplySettings();

        if (musicVolumeSlider != null && gameMusic != null)
            musicVolumeSlider.onValueChanged.AddListener(v =>
            {
                gameMusic.volume = v;
                SaveSettings();
            });

        if (bloomToggle != null)
            bloomToggle.onValueChanged.AddListener(v =>
            {
                if (bassVisualizer != null) bassVisualizer.bloomEnabled = v;
                SaveSettings();
            });

        if (chromaticToggle != null)
            chromaticToggle.onValueChanged.AddListener(v =>
            {
                if (bassVisualizer != null) bassVisualizer.chromaticEnabled = v;
                SaveSettings();
            });

        if (lensToggle != null)
            lensToggle.onValueChanged.AddListener(v =>
            {
                if (bassVisualizer != null) bassVisualizer.lensDistortionEnabled = v;
                SaveSettings();
            });

        if (vignetteToggle != null)
            vignetteToggle.onValueChanged.AddListener(v =>
            {
                if (bassVisualizer != null) bassVisualizer.vignetteEnabled = v;
                SaveSettings();
            });

        if (filmGrainToggle != null)
            filmGrainToggle.onValueChanged.AddListener(v =>
            {
                if (bassVisualizer != null) bassVisualizer.filmGrainEnabled = v;
                SaveSettings();
            });
    }

    // -------- SAVE --------
    void SaveSettings()
    {
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("Volume", musicVolumeSlider.value);

        if (bloomToggle != null)
            PlayerPrefs.SetInt("Bloom", bloomToggle.isOn ? 1 : 0);

        if (chromaticToggle != null)
            PlayerPrefs.SetInt("Chromatic", chromaticToggle.isOn ? 1 : 0);

        if (lensToggle != null)
            PlayerPrefs.SetInt("Lens", lensToggle.isOn ? 1 : 0);

        if (vignetteToggle != null)
            PlayerPrefs.SetInt("Vignette", vignetteToggle.isOn ? 1 : 0);

        if (filmGrainToggle != null)
            PlayerPrefs.SetInt("FilmGrain", filmGrainToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    // -------- LOAD --------
    void LoadSettings()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);

        if (bloomToggle != null)
            bloomToggle.isOn = PlayerPrefs.GetInt("Bloom", 1) == 1;

        if (chromaticToggle != null)
            chromaticToggle.isOn = PlayerPrefs.GetInt("Chromatic", 1) == 1;

        if (lensToggle != null)
            lensToggle.isOn = PlayerPrefs.GetInt("Lens", 1) == 1;

        if (vignetteToggle != null)
            vignetteToggle.isOn = PlayerPrefs.GetInt("Vignette", 1) == 1;

        if (filmGrainToggle != null)
            filmGrainToggle.isOn = PlayerPrefs.GetInt("FilmGrain", 1) == 1;
    }

    // -------- APPLY --------
    void ApplySettings()
    {
        if (gameMusic != null && musicVolumeSlider != null)
            gameMusic.volume = musicVolumeSlider.value;

        if (bassVisualizer != null)
        {
            if (bloomToggle != null)
                bassVisualizer.bloomEnabled = bloomToggle.isOn;

            if (chromaticToggle != null)
                bassVisualizer.chromaticEnabled = chromaticToggle.isOn;

            if (lensToggle != null)
                bassVisualizer.lensDistortionEnabled = lensToggle.isOn;

            if (vignetteToggle != null)
                bassVisualizer.vignetteEnabled = vignetteToggle.isOn;

            if (filmGrainToggle != null)
                bassVisualizer.filmGrainEnabled = filmGrainToggle.isOn;
        }
    }

    // -------- GAME FLOW --------

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

        if (EventSystem.current != null && firstPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseButton.gameObject);
        }
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);

        if (EventSystem.current != null && firstSettingsButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton.gameObject);
        }
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        if (EventSystem.current != null && firstPauseButton != null)
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