using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [Header("Destroy Settings")]
    public float destroyTime = 0.6f; // You can edit this in Inspector

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}