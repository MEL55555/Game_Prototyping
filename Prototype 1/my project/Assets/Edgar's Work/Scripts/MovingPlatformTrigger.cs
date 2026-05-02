using UnityEngine;
using System.Collections;

public class MovingPlatformTrigger : MonoBehaviour
{
    [Header("Platform To Move")]
    public Transform platform;

    [Header("Movement Settings")]
    public Vector3 targetPosition;
    public float moveSpeed = 3f;

    [Header("Appearance Settings")]
    public bool appearBeforeMove = true;
    public bool disappearAfterMove = true;

    [Header("Delays (seconds)")]
    public float appearDelay = 0f;
    public float moveDelay = 0f;
    public float disappearDelay = 0f;

    private bool triggered = false;
    private bool isMoving = false;

    Vector3 startPosition;
    Coroutine platformCoroutine;

    void Start()
    {
        if (platform == null) return;

        startPosition = platform.position;

        if (appearBeforeMove)
            platform.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        PlayerController.OnPlayerRespawn += ResetPlatform;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerRespawn -= ResetPlatform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            platformCoroutine = StartCoroutine(PlatformSequence());
        }
    }

    private IEnumerator PlatformSequence()
    {
        if (platform == null) yield break;

        // --- Appear ---
        if (appearBeforeMove)
        {
            yield return new WaitForSeconds(appearDelay);
            platform.gameObject.SetActive(true);
        }

        // --- Move delay ---
        if (moveDelay > 0f)
            yield return new WaitForSeconds(moveDelay);

        // --- Move ---
        isMoving = true;

        while (Vector3.Distance(platform.position, targetPosition) > 0.01f)
        {
            platform.position = Vector3.MoveTowards(
                platform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        platform.position = targetPosition;
        isMoving = false;

        // --- Disappear delay ---
        if (disappearAfterMove)
            yield return new WaitForSeconds(disappearDelay);

        // --- Disappear ---
        if (disappearAfterMove)
            platform.gameObject.SetActive(false);
    }

    void ResetPlatform()
    {
        if (platformCoroutine != null)
            StopCoroutine(platformCoroutine);

        triggered = false;
        isMoving = false;

        if (platform != null)
        {
            platform.position = startPosition;

            if (appearBeforeMove)
                platform.gameObject.SetActive(false);
            else
                platform.gameObject.SetActive(true);
        }
    }
}