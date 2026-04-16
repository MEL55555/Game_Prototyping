using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ApplySettingsInGame : MonoBehaviour
{
    void Start()
    {
        var cameraData = GetComponent<UniversalAdditionalCameraData>();
        if (cameraData != null)
        {
            // Load the saved setting 
            cameraData.renderPostProcessing = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;
        }
    }
}