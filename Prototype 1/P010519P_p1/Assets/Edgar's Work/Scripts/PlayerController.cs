using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Events to tell other scripts (like the UI or Taunt system) what happened
    public static Action OnPlayerRespawn;
    public event Action OnPlayerDeath;

    [Header("Movement Tweaks")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 12f;
    public float airControlSpeed = 3f;

    [Header("Juicy Spin")]
    public float jumpSpinForce = 700f;
    public float doubleJumpSpinForce = 1100f;
    public float spinDrag = 6f;

    [Header("Squash and Stretch")]
    public float jumpStretch = 1.2f;
    public float jumpSquash = 0.8f;
    public float landSquash = 0.7f;
    public float squashSpeed = 12f;
    public float squashMultiplier = 0.5f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    public float groundCheckOffset = 0.55f;

    [Header("Camera Shake Link")]
    public CameraFollow camFollow;
    public float maxShakeMagnitude = 0.8f;

    [Header("Stats & Respawn")]
    public int deathCount = 0;
    public Transform respawnPoint;
    public GameObject deathEffectPrefab;
    public float respawnDelay = 0.9f;

    [Header("Visuals")]
    public TrailRenderer trail;

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

    // New Input System variables
    InputAction moveAction;
    InputAction jumpAction;
    float moveInput;
    bool jumpPressed;

    void OnEnable()
    {
        // Setup movement for Keyboard (AD) and Gamepad
        moveAction = new InputAction(type: InputActionType.Value);
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        moveAction.AddBinding("<Gamepad>/leftStick/x");

        moveAction.performed += ctx => moveInput = ctx.ReadValue<float>();
        moveAction.canceled += ctx => moveInput = 0f;

        // Setup jump for Space and Gamepad A/Cross
        jumpAction = new InputAction(type: InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        jumpAction.performed += ctx => jumpPressed = true;

        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        if (isDead) return;

        // Keep the ground check box where it belongs
        ForceGroundCheckPosition();
        CheckGround();

        HandleMovement();
        HandleJump();
        HandleSpin();
        HandleJellySquash();

        previousYVelocity = rb.linearVelocity.y;
    }

    void HandleMovement()
    {
        // Use Lerp in the air so it feels floaty, but snappy on the ground
        if (isGrounded)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, moveInput * moveSpeed, airControlSpeed * Time.deltaTime),
                rb.linearVelocity.y
            );
    }

    void HandleJump()
    {
        if (jumpPressed)
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
        jumpPressed = false;
    }

    void PerformJump(float force)
    {
        // Reset Y velocity before jumping so double jumps feel consistent
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
        // Simple math to make the jelly feel bouncy
        float yDiff = targetScale.y - transform.localScale.y;
        float xDiff = -yDiff * squashMultiplier;
        Vector3 desiredScale = new Vector3(originalScale.x + xDiff, transform.localScale.y + yDiff, 1);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, squashSpeed * Time.deltaTime);
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);

        // Landed this frame
        if (isGrounded && !wasGrounded)
        {
            hasUsedDoubleJump = false;
            // Snap to 90 degrees so we don't land sideways
            float snapped = Mathf.Round(transform.eulerAngles.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(0, 0, snapped);
            spinVelocity = 0;

            // Landing squash effect
            targetScale = new Vector3(originalScale.x * 1.2f, originalScale.y * landSquash, 1);
            Invoke(nameof(ResetScale), 0.08f);

            // Shake camera based on how hard we hit the ground
            if (camFollow != null)
            {
                float landingSpeed = Mathf.Abs(previousYVelocity);
                pendingShakeMagnitude = Mathf.Clamp(landingSpeed * 0.02f, 0f, maxShakeMagnitude);
                Invoke(nameof(TriggerCameraShake), 0.05f);
           
            }
        
        }
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        deathCount++;
        OnPlayerDeath?.Invoke(); // Tell the UI/Audio systems we died

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        rb.linearVelocity = Vector2.zero;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // Reset for next life
        transform.position = respawnPoint.position;
        spriteRenderer.enabled = true;
        OnPlayerRespawn?.Invoke();
        isDead = false;
    }

    // Helper functions and debugging
    void ResetScale() => targetScale = originalScale;
    void TriggerCameraShake() { if (camFollow != null) camFollow.ShakeCamera(0.25f, pendingShakeMagnitude); }
    void ForceGroundCheckPosition() { groundCheck.position = transform.position + Vector3.down * groundCheckOffset; }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Death") && !isDead)
            StartCoroutine(DeathSequence());
    }

    void OnDrawGizmosSelected()
    {
        // Visualization for the ground check in the editor
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        }
    }
}