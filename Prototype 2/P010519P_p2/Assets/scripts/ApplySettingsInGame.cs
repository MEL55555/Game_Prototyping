using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ApplySettingsInGame : MonoBehaviour
{
    void Start()
    {
        // Grab the URP extra data from the cam
        var cameraData = GetComponent<UniversalAdditionalCameraData>();

        if (cameraData != null)
        {
            // Load saved setting from prefs - defaults to on if nothing is there
            bool isEnabled = PlayerPrefs.GetInt("PostProcessEnabled", 1) == 1;

            // Toggle the post-processing bit on the camera
            cameraData.renderPostProcessing = isEnabled;
        }
        else
        {
            // Just in case we've forgotten to stick this on a camera
            Debug.LogWarning("No UniversalAdditionalCameraData found on this object, mate.");
        }
    }
}