using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static Action OnPlayerRespawn;
    public event Action OnPlayerDeath; // Event for explosion

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 12f;
    public float airControlSpeed = 3f;

    [Header("Spin")]
    public float jumpSpinForce = 700f;
    public float doubleJumpSpinForce = 1100f;
    public float spinDrag = 6f;

    [Header("Jelly Squash & Stretch")]
    public float jumpStretch = 1.2f;
    public float jumpSquash = 0.8f;
    public float landSquash = 0.7f;
    public float squashSpeed = 12f;
    public float squashMultiplier = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    public float groundCheckOffset = 0.55f;

    [Header("Camera Shake")]
    public CameraFollow camFollow;
    public float maxShakeMagnitude = 0.8f;
    public float shakeDurationMultiplier = 0.02f;
    public float shakeDelay = 0.05f;

    [Header("Death Counter")]
    public int deathCount = 0;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    [Header("Death Effect")]
    public GameObject deathEffectPrefab;

    [Header("Death Timing")]
    public float respawnDelay = 0.9f;

    [Header("Trail Settings")]
    public TrailRenderer trail;
    
    [Header("Death Taunts")]
    public DeathTauntSystem tauntSystem;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    bool isGrounded;
    bool hasUsedDoubleJump;
    bool isDead = false;

    Vector3 originalScale;
    Vector3 targetScale;
    float spinVelocity;
    float previousYVelocity;
    float pendingShakeMagnitude = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        targetScale = originalScale;
        previousYVelocity = rb.linearVelocity.y;

        if (trail != null)
            trail.emitting = true;
    }

    void Update()
    {
        if (isDead) return;

        ForceGroundCheckPosition();
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleSpin();
        HandleJellySquash();

        previousYVelocity = rb.linearVelocity.y;
    }

    void ForceGroundCheckPosition()
    {
        if (groundCheck == null) return;
        groundCheck.position = transform.position + Vector3.down * groundCheckOffset;
        groundCheck.rotation = Quaternion.identity;
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, moveInput * moveSpeed, airControlSpeed * Time.deltaTime),
                rb.linearVelocity.y
            );

        if (trail != null)
            trail.emitting = Mathf.Abs(rb.linearVelocity.x) > 0.1f || Mathf.Abs(rb.linearVelocity.y) > 0.1f;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                PerformJump(jumpForce);
                hasUsedDoubleJump = false;
                spinVelocity += jumpSpinForce;
                targetScale = new Vector3(originalScale.x * jumpSquash, originalScale.y * jumpStretch, 1);
            }
            else if (!hasUsedDoubleJump)
            {
                PerformJump(doubleJumpForce);
                hasUsedDoubleJump = true;
                spinVelocity += doubleJumpSpinForce;
                targetScale = new Vector3(originalScale.x * 0.75f, originalScale.y * 1.3f, 1);
            }
        }
    }

    void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    void HandleSpin()
    {
        if (!isGrounded)
        {
            transform.Rotate(0, 0, spinVelocity * Time.deltaTime);
            spinVelocity = Mathf.Lerp(spinVelocity, 0, spinDrag * Time.deltaTime);
        }
    }

    void HandleJellySquash()
    {
        float yDiff = targetScale.y - transform.localScale.y;
        float xDiff = -yDiff * squashMultiplier;
        Vector3 desiredScale = new Vector3(originalScale.x + xDiff, transform.localScale.y + yDiff, 1);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, squashSpeed * Time.deltaTime);
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            hasUsedDoubleJump = false;
            float snapped = Mathf.Round(transform.eulerAngles.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(0, 0, snapped);
            spinVelocity = 0;
            targetScale = new Vector3(originalScale.x * 1.2f, originalScale.y * landSquash, 1);
            Invoke(nameof(ResetScale), 0.08f);

            if (camFollow != null)
            {
                float landingSpeed = Mathf.Abs(previousYVelocity);
                pendingShakeMagnitude = Mathf.Clamp(landingSpeed * shakeDurationMultiplier, 0f, maxShakeMagnitude);
                Invoke(nameof(TriggerCameraShake), shakeDelay);
            }
        }
    }

    void TriggerCameraShake()
    {
        if (camFollow != null && pendingShakeMagnitude > 0f)
        {
            camFollow.ShakeCamera(0.25f, pendingShakeMagnitude);
            pendingShakeMagnitude = 0f;
        }
    }

    void ResetScale() => targetScale = originalScale;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Death") && !isDead)
            StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        deathCount++;

        // Trigger death event for explosion
        OnPlayerDeath?.Invoke();

        // Spawn particle effect
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Camera shake
        if (camFollow != null)
            camFollow.ShakeCamera(0.25f, 0.5f);

        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (trail != null) trail.emitting = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = respawnPoint.position;
        targetScale = originalScale;
        spinVelocity = 0;

        if (trail != null) trail.Clear();
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        // Trigger taunt voice + typing
        OnPlayerRespawn?.Invoke();
        isDead = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}