using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    // Whether the player has activated this trigger
    public bool activated = false;

    void OnEnable()
    {
        PlayerController.OnPlayerRespawn += ResetTrigger;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerRespawn -= ResetTrigger;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            activated = true;
        }
    }

    void ResetTrigger()
    {
        activated = false;
    }
}