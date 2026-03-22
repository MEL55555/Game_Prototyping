using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuWithSettings : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainMenuUI;
    public GameObject settingsMenuUI;

    [Header("Buttons")]
    public Button firstMainButton;
    public Button firstSettingsButton;

    [Header("Audio")]
    public AudioSource menuMusic;
    public Slider musicVolumeSlider;

    [Header("Post Processing")]
    public BassPostProcessPulse bassVisualizer;

    [Header("Toggles")]
    public Toggle bloomToggle;
    public Toggle chromaticToggle;
    public Toggle lensToggle;
    public Toggle vignetteToggle;
    public Toggle filmGrainToggle;

    [Header("Custom Cursor")]
    public CustomCursor customCursor;

    void Start()
    {
        Time.timeScale = 1f;

        // ✅ CURSOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (customCursor != null)
        {
            customCursor.gameObject.SetActive(true);
            customCursor.ShowCursor();
        }

        // ✅ MENU STATE
        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);

        // ✅ LOAD SETTINGS FIRST
        LoadSettings();

        // ✅ APPLY SETTINGS TO SYSTEMS
        ApplySettings();

        // ✅ LISTENERS (SAVE ON CHANGE)
        if (musicVolumeSlider != null && menuMusic != null)
            musicVolumeSlider.onValueChanged.AddListener(v =>
            {
                menuMusic.volume = v;
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

        // ✅ SELECT BUTTON
        if (EventSystem.current != null && firstMainButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton.gameObject);
        }
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
        if (menuMusic != null && musicVolumeSlider != null)
            menuMusic.volume = musicVolumeSlider.value;

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

    // -------- MENU --------

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OpenSettings()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(true);

        if (EventSystem.current != null && firstSettingsButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSettingsButton.gameObject);
        }
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null)
            settingsMenuUI.SetActive(false);

        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (EventSystem.current != null && firstMainButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMainButton.gameObject);
        }
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