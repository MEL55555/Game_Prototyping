using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 12f; // Stronger double jump
    public float airControlSpeed = 3f;   // How fast you can adjust in air

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasUsedDoubleJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            // Full horizontal control on ground
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Air control: smoothly adjust horizontal velocity
            float targetVelocityX = moveInput * moveSpeed;
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, airControlSpeed * Time.deltaTime),
                rb.linearVelocity.y
            );
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                PerformJump(jumpForce);
                hasUsedDoubleJump = false;
            }
            else if (!hasUsedDoubleJump)
            {
                PerformJump(doubleJumpForce);
                hasUsedDoubleJump = true;
            }
        }
    }

    void PerformJump(float force)
    {
        // Only reset vertical velocity, keep horizontal momentum
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded && !wasGrounded)
        {
            hasUsedDoubleJump = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Death"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}