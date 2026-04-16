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
        // finds camera settings for post processing
        if (Camera.main != null)
            _cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

        SetupResolutionDropdown();
        LoadAndApplySettings();

        // start the game unpaused
        Resume();
    }

    void Update()
    {
        // wait until the intro is over before letting player pause
        if (StoryManager.Instance != null && !StoryManager.Instance.canPause)
        {
            return; 
        }

        // check for the escape key to open or close menu
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
        // turn off the menu and start time again
        pauseMenuUI.SetActive(false);
        if(settingsPanel != null) settingsPanel.SetActive(false);
        
        Time.timeScale = 1f;
        IsPaused = false;
        
        // hide mouse for playing
       Cursor.lockState = CursorLockMode.Locked; 
       Cursor.visible = false;
    }

    void Pause()
    {
        // show the menu and stop the game world
        pauseMenuUI.SetActive(true);
        if(settingsPanel != null) settingsPanel.SetActive(false);
        
        Time.timeScale = 0f;
        IsPaused = true;

        // free the mouse to click buttons
      Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        if(settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if(settingsPanel != null) settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void BackToMainMenu()
    {
        // reset time and clean up before leaving
        Time.timeScale = 1f; 
        IsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(0); 
    }

    private void LoadAndApplySettings()
    {
        // load graphics choices from player memory
        bool ppEnabled = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;
        if (_cameraData != null) _cameraData.renderPostProcessing = ppEnabled;
        if (postProcessToggle != null) postProcessToggle.isOn = ppEnabled;

        if (PlayerPrefs.HasKey("ResWidth") && PlayerPrefs.HasKey("ResHeight"))
        {
            int savedW = PlayerPrefs.GetInt("ResWidth");
            int savedH = PlayerPrefs.GetInt("ResHeight");
          Screen.SetResolution(savedW, savedH, FullScreenMode.Windowed);
            UpdateDropdownUI(savedW, savedH);
        }
    }

    void SetupResolutionDropdown()
    {
        // list all screen sizes the computer can do
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
        
        // sync the dropdown with current screen size
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
       Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
        
        // save resolution so it sticks
        PlayerPrefs.SetInt("ResWidth", res.width);
        PlayerPrefs.SetInt("ResHeight", res.height);
        PlayerPrefs.Save();
    }

    public void TogglePostProcessing(bool isOn)
    {
        // turn visuals on or off instantly
        if (_cameraData != null) _cameraData.renderPostProcessing = isOn;
        PlayerPrefs.SetInt("PostProcessEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}