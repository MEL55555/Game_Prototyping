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
        // makes the mouse show up so you can click the menu
       Cursor.visible = true;
       Cursor.lockState = CursorLockMode.None;

        if (Camera.main != null)
            _cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

        SetupResolutionDropdown();

        // gets your saved settings like volume or graphics
        LoadAndApplySettings();

        OpenMainMenu();
    }

    private void LoadAndApplySettings()
    {
        // checks if post processing was left on or off
        bool ppEnabled = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;
        if (_cameraData != null) _cameraData.renderPostProcessing = ppEnabled;
        if (postProcessToggle != null) postProcessToggle.isOn = ppEnabled;

        // tries to set the screen size to what you used last time
        if (PlayerPrefs.HasKey("ResWidth") && PlayerPrefs.HasKey("ResHeight"))
        {
            int savedW = PlayerPrefs.GetInt("ResWidth");
            int savedH = PlayerPrefs.GetInt("ResHeight");
          Screen.SetResolution(savedW, savedH, FullScreenMode.Windowed);
            
            UpdateDropdownUI(savedW, savedH);
        }
        else
        {
            // uses the default screen size if it is the first run
            int nativeW = Screen.currentResolution.width;
            int nativeH = Screen.currentResolution.height;
            Screen.SetResolution(nativeW, nativeH, FullScreenMode.Windowed);
            UpdateDropdownUI(nativeW, nativeH);
        }
    }

    private void UpdateDropdownUI(int width, int height)
    {
        if (resolutionDropdown == null || _resolutions == null) return;

        // makes the list show the right resolution as selected
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

    public void PlayGame() { 
        // goes to the next scene in your build list
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }
    
    public void OpenSettings() { 
        mainMenuPanel.SetActive(false); 
        settingsPanel.SetActive(true); 
    }
    
    public void OpenMainMenu() { 
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
        // finds all possible screen sizes for your monitor
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
       Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
        
        // saves the choice so it stays when you restart
        PlayerPrefs.SetInt("ResWidth", res.width);
        PlayerPrefs.SetInt("ResHeight", res.height);
        PlayerPrefs.Save(); 
    }

    public void TogglePostProcessing(bool isOn)
    {
        // turns the fancy camera effects on or off
        if (_cameraData != null) _cameraData.renderPostProcessing = isOn;
        
        PlayerPrefs.SetInt("PostProcessEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save(); 
    }
}