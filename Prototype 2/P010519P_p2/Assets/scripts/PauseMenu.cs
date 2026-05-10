using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused = false;

    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;

    [Header("Settings UI Components")]
    public TMP_Dropdown resolutionDropdown;
    public UnityEngine.UI.Toggle postProcessToggle;

    private Resolution[] _resolutions;
    private UniversalAdditionalCameraData _cameraData;

    void Start()
    {
        if (Camera.main != null)
            _cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

        SetupResolutionDropdown();

        // re-apply settings every time the scene loads to prevent resolution resetting
        LoadAndApplySettings();

        Resume();
    }

    void Update()
    {
        if (StoryManager.Instance != null && !StoryManager.Instance.canPause) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                    CloseSettings();
                else
                    Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }

    private void LoadAndApplySettings()
    {
        // load and apply visuals
        bool ppEnabled = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;
        if (_cameraData != null) _cameraData.renderPostProcessing = ppEnabled;
        if (postProcessToggle != null) postProcessToggle.isOn = ppEnabled;

        // force the saved resolution. This stops the game from reverting to a windowed 
        // default when moving from the menu to the level.
        if (PlayerPrefs.HasKey("ResWidth") && PlayerPrefs.HasKey("ResHeight"))
        {
            int savedW = PlayerPrefs.GetInt("ResWidth");
            int savedH = PlayerPrefs.GetInt("ResHeight");

            // if for some reason the screen changed, force it back to fullscreen
            if (Screen.width != savedW || Screen.height != savedH || Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                Screen.SetResolution(savedW, savedH, FullScreenMode.FullScreenWindow);
            }
            UpdateDropdownUI(savedW, savedH);
        }
    }

    void SetupResolutionDropdown()
    {
        _resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add(_resolutions[i].width + " x " + _resolutions[i].height);
        }

        resolutionDropdown.AddOptions(options);
    }

    private void UpdateDropdownUI(int width, int height)
    {
        if (resolutionDropdown == null || _resolutions == null) return;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].width == width && _resolutions[i].height == height)
            {
                resolutionDropdown.value = i;
                resolutionDropdown.RefreshShownValue();
                break;
            }
        }
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= _resolutions.Length) return;
        Resolution res = _resolutions[index];
        Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);

        PlayerPrefs.SetInt("ResWidth", res.width);
        PlayerPrefs.SetInt("ResHeight", res.height);
        PlayerPrefs.Save();
    }

    public void TogglePostProcessing(bool isOn)
    {
        if (_cameraData != null) _cameraData.renderPostProcessing = isOn;
        PlayerPrefs.SetInt("PostProcessEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}