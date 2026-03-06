using UnityEngine;

public class Teleporter : MonoBehaviour
{
    // The portal we teleport to
    public Teleporter exitTeleporter;

    // Where the player appears
    public Transform exitPoint;

    private bool playerInside = false;
    private bool canTeleport = true;

    void Update()
    {
        // If player is inside portal and presses jump
        if (playerInside && canTeleport && Input.GetKeyDown(KeyCode.Space))
        {
            Teleport();
        }
    }

    void Teleport()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        // Disable both portals briefly to stop looping
        canTeleport = false;
        exitTeleporter.canTeleport = false;

        // Move player
        player.transform.position = exitPoint.position;

        // Re-enable portals after delay
        Invoke(nameof(ResetTeleport), 0.3f);
        exitTeleporter.Invoke(nameof(ResetTeleport), 0.3f);
    }

    void ResetTeleport()
    {
        canTeleport = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}