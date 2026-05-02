using UnityEngine;
using TMPro;

public class DeathCounterUI : MonoBehaviour
{
    public TextMeshProUGUI deathText;       // Assign your TMP text object here
    private int deathCount;

    void Start()
    {
        // Load previous death count if it exists
        deathCount = PlayerPrefs.GetInt("DeathCount", 0);
        UpdateDeathText();
    }

    void Update()
    {
        // Continuously check the PlayerController's death count
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.deathCount != deathCount)
        {
            deathCount = player.deathCount;
            UpdateDeathText();

            // Save the updated death count
            PlayerPrefs.SetInt("DeathCount", deathCount);
        }
    }

    void UpdateDeathText()
    {
        if (deathText != null)
        {
            deathText.text = $"Deaths: {deathCount}";
        }
    }

    // Optional: reset death count (call this if you want a reset button)
    public void ResetDeathCount()
    {
        deathCount = 0;
        PlayerPrefs.SetInt("DeathCount", deathCount);
        UpdateDeathText();
    }
}