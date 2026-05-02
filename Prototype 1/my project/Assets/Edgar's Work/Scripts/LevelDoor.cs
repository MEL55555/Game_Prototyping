using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    [Header("Level Exit")]
    public bool loadNextScene = true;
    public string nextSceneName;

    [Header("Door Teleport Trick")]
    public bool doorMoves = false;
    public Transform[] teleportLocations;
    public float detectionRadius = 2f;
    public int maxTeleports = 1;

    private int teleportCount = 0;
    private Transform player;

    Vector3 startPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
    }

    void OnEnable()
    {
        PlayerController.OnPlayerRespawn += ResetDoor;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerRespawn -= ResetDoor;
    }

    void Update()
    {
        if (!doorMoves || player == null) return;
        if (teleportCount >= maxTeleports) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            TeleportDoor();
        }
    }

    void TeleportDoor()
    {
        if (teleportLocations.Length == 0) return;

        int randomIndex = Random.Range(0, teleportLocations.Length);

        transform.position = teleportLocations[randomIndex].position;

        teleportCount++;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!loadNextScene) return;

        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void ResetDoor()
    {
        teleportCount = 0;
        transform.position = startPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}