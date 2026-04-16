using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
  public static ScoreManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;

    [Header("Altitude Thresholds")]
    public float heightStep = 10f; 
    public float startMultiplierY = 5f; 
    
    [Header("Scoring Math")]
    public float pointsPerUnit = 1f; 
    public int maxMultiplier = 10;

    private float currentScore = 0f;
    private Transform player;
    private int currentMultiplier = 1;

    void Awake() { Instance = this; }

    void Start()
    {
        // finds the player object in the scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // calculates how high the player is to give a score boost
        if (player.position.y > startMultiplierY)
        {
            float extraHeight = player.position.y - startMultiplierY;
            currentMultiplier = 1 + Mathf.FloorToInt(extraHeight / heightStep);
        }
        else
        {
            currentMultiplier = 1;
        }

        // stops the multiplier from going too high
        currentMultiplier = Mathf.Clamp(currentMultiplier, 1, maxMultiplier);

        // adds points based on how fast the player moves right
        float speed = player.GetComponent<Rigidbody2D>().linearVelocity.x;
        if (speed > 0.5f) 
        {
          currentScore += speed * currentMultiplier * pointsPerUnit * Time.deltaTime;
        }

        // shows the current score on the screen
        scoreText.text = Mathf.FloorToInt(currentScore).ToString();
        
        if (currentMultiplier > 1)
        {
            multiplierText.text = "x" + currentMultiplier;
            UpdateMultiplierVisuals();
        }
        else
        {
            multiplierText.text = "";
        }
    }

    void UpdateMultiplierVisuals()
    {
        // changes the color of the text as the player goes higher
        if (currentMultiplier >= 8) multiplierText.color = Color.cyan; 
        else if (currentMultiplier >= 5) multiplierText.color = Color.yellow; 
        else multiplierText.color = Color.white; 
    }

    public void AddBonusPoints(int amount, int streak)
    {
        // gives extra points for doing cool things
      currentScore += (amount * streak) * currentMultiplier;
    }
    
    public float GetScore()
    {
        return currentScore;
    }
}