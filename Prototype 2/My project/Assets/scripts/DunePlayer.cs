using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Added for UI text support

public class DunePlayer : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("UI Feedback")]
    [Tooltip("Drag your 'SpeedUpText' (TextMeshProUGUI) object here.")]
    public TextMeshProUGUI speedUpText;
    public float textDisplayDuration = 1.5f;

    [Header("Audio")]
    public AudioSource slideSfx;
    [Tooltip("Drag the second AudioSource (for the death sound) here.")]
    public AudioSource deathSfx;
    public float maxSfxVolume = 0.8f;
    public float sfxLerpSpeed = 5f;

    [Header("Visuals & Particles")]
    public Transform directionArrow;
    public float arrowOffset = 2.0f;
    public float arrowShowThreshold = 1.0f;
    [Tooltip("Drag the FireTrail Particle System here.")]
    public ParticleSystem fireTrail;
    [Tooltip("The speed at which the fire starts appearing.")]
    public float fireStartSpeed = 25f;
    [Tooltip("How many particles to emit per unit of speed over the threshold.")]
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

    [Header("Score Scaling (Difficulty & Power)")]
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
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false; 
        _currentDiveGravity = baseDiveGravity;

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

        // Hide the popup text at the start
        if (speedUpText != null) speedUpText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) StartGame();
            return;
        }

        if (!_isDead)
        {
            _isDiving = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            
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

        var em = fireTrail.emission;
        float currentSpeed = rb.linearVelocity.magnitude;

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
            directionArrow.localPosition = velocity.normalized * arrowOffset;
        }
        else directionArrow.gameObject.SetActive(false);
    }

    private void HandleSpeedScaling()
    {
        if (ScoreManager.Instance != null && !_isDead)
        {
            int score = Mathf.FloorToInt(ScoreManager.Instance.GetScore());
            
            if (score >= _lastUpgradeMilestone + pointsToUpgrade)
            {
                // Increase Top Speed
                if (maxSpeed < absoluteMaxSpeed) maxSpeed += speedIncreaseAmount;
                
                // Increase Push Force
                if (slidePushForce < maxPushForce) slidePushForce += pushForceIncrease;

                // Increase Constant Acceleration
                if (diveAcceleration < maxDiveAccel) diveAcceleration += diveAccelIncrease;

                // Increase Gravity (Small steps)
                if (maxDiveGravity < absoluteMaxGravity) maxDiveGravity += gravityIncrease;

                _lastUpgradeMilestone += pointsToUpgrade;

                // TRIGGER THE UI NOTIFICATION
                StartCoroutine(ShowSpeedUpText());

                Debug.Log($"Stats Up! Max Speed: {maxSpeed}, Push: {slidePushForce}, Accel: {diveAcceleration}, Gravity: {maxDiveGravity}");
            }
        }
    }

    // Coroutine to show and hide text briefly
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

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        
        _isTouchingGround = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _isTouchingGround = true;
        if (_isDead) return;
        
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
        
        if (deathSfx != null) deathSfx.Play();

        CameraShake.Instance?.Shake(shakeDuration, shakeIntensity);

        rb.gravityScale = 4f; 
        rb.angularVelocity = 720f; 
        yield return new WaitForSeconds(respawnDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void StartGame()
    {
        _gameStarted = true;
        rb.simulated = true;
        rb.linearVelocity = new Vector2(initialPush, -2f);
    }
}