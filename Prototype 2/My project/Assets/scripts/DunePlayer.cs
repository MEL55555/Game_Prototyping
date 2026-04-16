using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using System.Collections;
using TMPro;

public class DunePlayer : MonoBehaviour
{
  public Rigidbody2D rb;

    [Header("UI Feedback")]
    public TextMeshProUGUI speedUpText;
    public float textDisplayDuration = 1.5f;

    [Header("Audio")]
    public AudioSource slideSfx;
    public AudioSource deathSfx;
    public float maxSfxVolume = 0.8f;
    public float sfxLerpSpeed = 5f;

    [Header("Visuals & Particles")]
    public Transform directionArrow;
    public float arrowOffset = 2.0f;
    public float arrowShowThreshold = 1.0f;
    public ParticleSystem fireTrail;
    public float fireStartSpeed = 25f;
    public float fireIntensityMultiplier = 5f;

    [Header("Game State")]
    private bool _gameStarted;
    private bool _isDead;

    [Header("Movement Settings")]
    public float floatGravity = 2.5f;      
    public float baseDiveGravity = 8f;   
    public float maxDiveGravity = 22f;    
    public float chargeSpeed = 8f;       
    public float slidePushForce = 35f;   
    public float passiveSlideBoost = 15f; 

    [Header("Diving Acceleration")]
    public float diveAcceleration = 10f;

    [Header("Speed & Momentum")]
    public float initialPush = 15f;
    public float maxSpeed = 60f;
    public float deathSpeedThreshold = 2.0f;    

    [Header("Score Scaling")]
    public int pointsToUpgrade = 1000;
    public float speedIncreaseAmount = 5f;
    public float absoluteMaxSpeed = 150f;
    [Space]
    public float pushForceIncrease = 5f;
    public float maxPushForce = 100f;
    [Space]
    public float diveAccelIncrease = 3f;
    public float maxDiveAccel = 50f;
    [Space]
    public float gravityIncrease = 1f; 
    public float absoluteMaxGravity = 40f;
    
    private int _lastUpgradeMilestone = 0;

    [Header("Death Settings")]
    public float backwardsDeathDelay = 0.3f; 
    public float respawnDelay = 1.2f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.5f;

    private float _backwardsTimer; 
    private float _currentDiveGravity;
    private bool _isDiving;
    private bool _isTouchingGround;
    
    void Start()
    {
        // gets the physics body and stops it until start
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false; 
        _currentDiveGravity = baseDiveGravity;

        // make sure there is no friction so we slide smooth
        if (rb.sharedMaterial != null) rb.sharedMaterial.friction = 0;

        if (slideSfx != null)
        {
            slideSfx.volume = 0;
            slideSfx.Play();
        }

        if (fireTrail != null)
        {
            var em = fireTrail.emission;
          em.rateOverTime = 0;
        }

        if (speedUpText != null) speedUpText.gameObject.SetActive(false);
    }

    void Update()
    {
        // stops the player from starting if the intro is playing
        if (StoryManager.Instance != null && !StoryManager.Instance.canStartGame) 
            return;

        bool startRequested = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) startRequested = true;
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) startRequested = true;

        if (!_gameStarted)
        {
            if (startRequested) StartGame();
            return;
        }

        if (!_isDead)
        {
            bool spaceHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool mouseHeld = Pointer.current != null && Pointer.current.press.isPressed;
            
            _isDiving = spaceHeld || mouseHeld;
            
            // ramps up the gravity when diving
            _currentDiveGravity = _isDiving 
                ? Mathf.MoveTowards(_currentDiveGravity, maxDiveGravity, chargeSpeed * Time.deltaTime) 
                : baseDiveGravity;

          UpdateDirectionArrow();
            CheckDeath();
            HandleSpeedScaling();
        }

        UpdateSfx();
        UpdateFireTrail();
    }

    private void UpdateFireTrail()
    {
        if (fireTrail == null) return;

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            // makes the fire point the same way we move
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            fireTrail.transform.rotation = Quaternion.Euler(0, 0, angle - 180f);
        }

        var em = fireTrail.emission;
        float currentSpeed = rb.linearVelocity.magnitude;

        // only show fire when moving fast
        if (!_isDead && currentSpeed > fireStartSpeed)
        {
            float excessSpeed = currentSpeed - fireStartSpeed;
            em.rateOverTime = excessSpeed * fireIntensityMultiplier;
        }
        else
        {
          em.rateOverTime = 0;
        }
    }

    private void UpdateSfx()
    {
        if (slideSfx == null) return;
        float targetVolume = 0f;

        // slide sound gets louder and higher as we go fast
        if (_isTouchingGround && rb.linearVelocity.magnitude > 0.5f)
        {
            float speedRatio = rb.linearVelocity.magnitude / maxSpeed;
            targetVolume = speedRatio * maxSfxVolume;
            slideSfx.pitch = Mathf.Lerp(0.7f, 1.1f, speedRatio);
        }

        slideSfx.volume = Mathf.MoveTowards(slideSfx.volume, targetVolume, sfxLerpSpeed * Time.deltaTime);
    }

    private void UpdateDirectionArrow()
    {
        if (directionArrow == null || _isDead) 
        {
            if(directionArrow != null) directionArrow.gameObject.SetActive(false);
            return;
        }
        Vector2 velocity = rb.linearVelocity;
        if (velocity.magnitude > arrowShowThreshold)
        {
            directionArrow.gameObject.SetActive(true);
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
          directionArrow.rotation = Quaternion.Euler(0, 0, angle);
            directionArrow.position = transform.position + (Vector3)velocity.normalized * arrowOffset;
        }
        else directionArrow.gameObject.SetActive(false);
    }

    private void HandleSpeedScaling()
    {
        if (ScoreManager.Instance != null && !_isDead)
        {
            float currentScore = ScoreManager.Instance.GetScore();
            int scoreInt = Mathf.FloorToInt(currentScore);

            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.CheckProgress(currentScore);
            }
            
            // levels up stats every thousand points
            if (scoreInt >= _lastUpgradeMilestone + pointsToUpgrade)
            {
                if (maxSpeed < absoluteMaxSpeed) maxSpeed += speedIncreaseAmount;
                if (slidePushForce < maxPushForce) slidePushForce += pushForceIncrease;
                if (diveAcceleration < maxDiveAccel) diveAcceleration += diveAccelIncrease;
                if (maxDiveGravity < absoluteMaxGravity) maxDiveGravity += gravityIncrease;

                _lastUpgradeMilestone += pointsToUpgrade;
                StartCoroutine(ShowSpeedUpText());
            }
        }
    }

    IEnumerator ShowSpeedUpText()
    {
        if (speedUpText == null) yield break;
        speedUpText.gameObject.SetActive(true);
        yield return new WaitForSeconds(textDisplayDuration);
        speedUpText.gameObject.SetActive(false);
    }

    private void CheckDeath()
    {
        if (_isDead) return;
        float vx = rb.linearVelocity.x;
        // die if moving too slow on ground or rolling back
        if (_isTouchingGround && vx < deathSpeedThreshold) { StartCoroutine(DeathSequence("Too Slow!")); return; }
        if (vx < -0.1f) 
        {
            _backwardsTimer += Time.deltaTime;
            if (_backwardsTimer >= backwardsDeathDelay) StartCoroutine(DeathSequence("Rolled Backwards!"));
        }
        else _backwardsTimer = 0f;
    }

    void FixedUpdate()
    {
        if (!_gameStarted) return;
        
        if (!_isDead)
            rb.gravityScale = _isDiving ? _currentDiveGravity : floatGravity;

        // cap the top speed
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        
        _isTouchingGround = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _isTouchingGround = true;
        if (_isDead) return;
        
        // math to push the ball along the curves
        Vector2 normal = collision.GetContact(0).normal;
        Vector2 slopeDirection = new Vector2(normal.y, -normal.x);
        if (slopeDirection.x < 0) slopeDirection *= -1;
        
        rb.AddForce(slopeDirection * passiveSlideBoost, ForceMode2D.Force);
        if (_isDiving) 
        {
            rb.AddForce(slopeDirection * slidePushForce, ForceMode2D.Force);
            rb.AddForce(slopeDirection * diveAcceleration, ForceMode2D.Force);
        }
    }

    IEnumerator DeathSequence(string reason)
    {
        _isDead = true;
        if (directionArrow != null) directionArrow.gameObject.SetActive(false);
        
        if (StoryManager.Instance != null) {
            StoryManager.Instance.SaveHighScore();
        }
        
        if (deathSfx != null) deathSfx.Play();

        CameraShake.Instance?.Shake(shakeDuration, shakeIntensity);

        // physics effect for dieing
        rb.gravityScale = 4f; 
        rb.angularVelocity = 720f; 
        yield return new WaitForSeconds(respawnDelay);

        // reload the scene but skip the intro
        if (StoryManager.Instance != null)
            StoryManager.Instance.RestartGame();
        else
          SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void StartGame()
    {
        _gameStarted = true;
        rb.simulated = true;
        rb.linearVelocity = new Vector2(initialPush, -2f);

        if (StoryManager.Instance != null)
        {
            if (StoryManager.Instance.startPanel != null)
            {
                StoryManager.Instance.startPanel.SetActive(false);
            }
          StoryManager.Instance.StartCinematicDescent();
        }
    }
}