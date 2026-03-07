using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public Transform player;
    public float detectionRadius = 3f;

    [Header("First Movement")]
    public Vector3 targetPosition;
    public float moveSpeed = 5f;

    [Header("Optional External Trigger")]
    public bool requireTriggerZone = false;
    public TriggerZone triggerZone;

    [Header("Second Movement (Optional)")]
    public bool enableSecondMovement = false;
    public Vector3 secondTargetPosition;
    public float secondMoveDelay = 1f;
    public float secondMoveSpeed = 5f;

    private bool hasTriggered = false;
    private int movementStage = 0;

    Vector3 startPosition;
    Coroutine secondMoveCoroutine;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnEnable()
    {
        PlayerController.OnPlayerRespawn += ResetTrap;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerRespawn -= ResetTrap;
    }

    void Update()
    {
        if (player == null) return;

        if (requireTriggerZone)
        {
            if (triggerZone == null) return;
            if (!triggerZone.activated) return;
        }

        if (!hasTriggered)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRadius)
            {
                hasTriggered = true;
            }

            return;
        }

        // FIRST MOVEMENT
        if (movementStage == 0)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;

                if (enableSecondMovement)
                {
                    movementStage = 1;
                    secondMoveCoroutine = StartCoroutine(StartSecondMove());
                }
            }
        }

        // SECOND MOVEMENT
        else if (movementStage == 2)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                secondTargetPosition,
                secondMoveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, secondTargetPosition) < 0.01f)
            {
                transform.position = secondTargetPosition;
                movementStage = 3;
            }
        }
    }

    IEnumerator StartSecondMove()
    {
        yield return new WaitForSeconds(secondMoveDelay);
        movementStage = 2;
    }

    void ResetTrap()
    {
        // Stop coroutine if running
        if (secondMoveCoroutine != null)
        {
            StopCoroutine(secondMoveCoroutine);
        }

        // Reset position and state
        transform.position = startPosition;
        hasTriggered = false;
        movementStage = 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.15f);

        if (enableSecondMovement)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(secondTargetPosition, 0.15f);
        }
    }
}