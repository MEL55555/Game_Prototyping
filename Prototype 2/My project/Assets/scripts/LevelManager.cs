using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public Transform player;

    [Header("Prefabs")]
    public GameObject startChunkPrefab;
    public GameObject duneBasePrefab;

    [Header("Chunk Settings")]
    public float chunkWidth = 120f;

    [Header("World Persistence")]
    public int chunksToKeepAhead = 6;
    public float deleteDistanceBehind = 300f;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float nextSpawnX = 0f;

    void Start()
    {
        nextSpawnX = 0f;
        activeChunks.Clear();

        // 1. Spawn the Tutorial (Must be exactly 120 units)
        if (startChunkPrefab != null)
        {
            // TESSELLATION FIX: Added +0.001f offset
            GameObject tutorial = Instantiate(startChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
            tutorial.transform.SetParent(this.transform);
            activeChunks.Add(tutorial);

            // FORCE the next spawn to be exactly at 120
            nextSpawnX += 120f;
        }

        // 2. Spawn initial dunes ahead
        for (int i = 0; i < chunksToKeepAhead; i++)
        {
            SpawnProceduralChunk(false);
        }
    }

    void Update()
    {
        // Check if we need to spawn a new chunk
        if (player.position.x > nextSpawnX - (chunkWidth * chunksToKeepAhead))
        {
            SpawnProceduralChunk(false);
        }

        // Cleanup old chunks
        if (activeChunks.Count > 0)
        {
            float chunkEndX = activeChunks[0].transform.position.x + chunkWidth;
            if (player.position.x > chunkEndX + deleteDistanceBehind)
            {
                GameObject old = activeChunks[0];
                activeChunks.RemoveAt(0);
                Destroy(old);
            }
        }
    }

    // This is the function the error was looking for!
    void SpawnProceduralChunk(bool isTutorial)
    {
        GameObject prefabToSpawn = isTutorial ? startChunkPrefab : duneBasePrefab;

        if (prefabToSpawn == null) return;

        // TESSELLATION FIX: Added a microscopic offset based on total count to prevent overlapping points
        float safeX = nextSpawnX + (activeChunks.Count * 0.001f);
        GameObject chunk = Instantiate(prefabToSpawn, new Vector3(safeX, 0, 0), Quaternion.identity);
        chunk.transform.SetParent(this.transform);

        // If it's the random dune, run the generator
        if (!isTutorial)
        {
            AutoBuildChunk gen = chunk.GetComponent<AutoBuildChunk>();
            if (gen != null)
            {
                gen.Generate();
            }
        }

        activeChunks.Add(chunk);
        nextSpawnX += chunkWidth; // Always moves by exactly 120
    }
}