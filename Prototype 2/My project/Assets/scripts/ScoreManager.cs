using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;

    [Header("Altitude Thresholds")]
    [Tooltip("How high must the player be to hit X2, X3, etc.")]
    public float heightStep = 10f; 
    public float startMultiplierY = 5f; // The altitude where X2 begins
    
    [Header("Scoring Math")]
    public float pointsPerUnit = 1f; // Base points for moving 1 unit right
    public int maxMultiplier = 10;

    private float currentScore = 0f;
    private Transform player;
    private int currentMultiplier = 1;

    void Awake() { Instance = this; }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. DYNAMIC MULTIPLIER CALCULATION
        // This math checks how many 'heightSteps' the player is above the start line
        if (player.position.y > startMultiplierY)
        {
            float extraHeight = player.position.y - startMultiplierY;
            // Adds 1 to the base for every 'heightStep' reached
            currentMultiplier = 1 + Mathf.FloorToInt(extraHeight / heightStep);
        }
        else
        {
            currentMultiplier = 1;
        }

        // Cap the multiplier
        currentMultiplier = Mathf.Clamp(currentMultiplier, 1, maxMultiplier);

        // 2. DISTANCE SCORING
        float speed = player.GetComponent<Rigidbody2D>().linearVelocity.x;
        if (speed > 0.5f) 
        {
            // Points = (Horizontal Speed) * (Altitude Multiplier)
            currentScore += speed * currentMultiplier * pointsPerUnit * Time.deltaTime;
        }

        // 3. UI UPDATES
        scoreText.text = Mathf.FloorToInt(currentScore).ToString();
        
        if (currentMultiplier > 1)
        {
            multiplierText.text = "x" + currentMultiplier;
            // Make the text "glow" or change color as it gets higher
            UpdateMultiplierVisuals();
        }
        else
        {
            multiplierText.text = "";
        }
    }

    void UpdateMultiplierVisuals()
    {
        // Cycles through colors based on multiplier
        if (currentMultiplier >= 8) multiplierText.color = Color.cyan; // Space
        else if (currentMultiplier >= 5) multiplierText.color = Color.yellow; // High Atmosphere
        else multiplierText.color = Color.white; // Low altitude
    }

    public void AddBonusPoints(int amount, int streak)
    {
        // Bonus points (from smooth landings) are ALSO multiplied by altitude!
        currentScore += (amount * streak) * currentMultiplier;
    }
    
    // Add this to the bottom of ScoreManager.cs
    public float GetScore()
    {
        return currentScore;
    }
}