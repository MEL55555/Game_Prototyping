using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public TMP_Dropdown resolutionDropdown;
    public UnityEngine.UI.Toggle postProcessToggle;

    private Resolution[] _resolutions;
    private UniversalAdditionalCameraData _cameraData;

    void Start()
    {
        // make sure the cursor isn't locked so the player can actually use the menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (Camera.main != null)
            _cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

        SetupResolutionDropdown();

        // fetch saved prefs or default to the monitor's native res
        LoadAndApplySettings();

        OpenMainMenu();
    }

    private void LoadAndApplySettings()
    {
        // sync post-processing with stored choice
        bool ppEnabled = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;
        if (_cameraData != null) _cameraData.renderPostProcessing = ppEnabled;
        if (postProcessToggle != null) postProcessToggle.isOn = ppEnabled;

        // check if we have a saved resolution; if not, grab the monitor's native res
        if (PlayerPrefs.HasKey("ResWidth") && PlayerPrefs.HasKey("ResHeight"))
        {
            int savedW = PlayerPrefs.GetInt("ResWidth");
            int savedH = PlayerPrefs.GetInt("ResHeight");

            // we use FullScreenWindow now to stop the game from shrinking into a tiny box
            Screen.SetResolution(savedW, savedH, FullScreenMode.FullScreenWindow);
            UpdateDropdownUI(savedW, savedH);
        }
        else
        {
            // first time launch: default to whatever the monitor is actually running
            Resolution nativeRes = Screen.currentResolution;
            Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);

            // save it immediately so the UI stays in sync
            PlayerPrefs.SetInt("ResWidth", nativeRes.width);
            PlayerPrefs.SetInt("ResHeight", nativeRes.height);
            UpdateDropdownUI(nativeRes.width, nativeRes.height);
        }
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

    public void PlayGame()
    {
        // load the first playable level (index 1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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

    public void SetResolution(int index)
    {
        if (index < 0 || index >= _resolutions.Length) return;

        Resolution res = _resolutions[index];
        // set resolution and keep it in fullscreen mode
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