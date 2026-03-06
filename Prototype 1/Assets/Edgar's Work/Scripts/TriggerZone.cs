using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    // Whether the player has activated this trigger
    public bool activated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
        }
    }
}