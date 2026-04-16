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
        // starts the world at zero
        nextSpawnX = 0f;
        activeChunks.Clear();

        if (startChunkPrefab != null)
        {
            // places the first tutorial piece
          GameObject tutorial = Instantiate(startChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
            tutorial.transform.SetParent(this.transform);
            activeChunks.Add(tutorial);

            nextSpawnX += 120f;
        }

        // fills out the path in front of the player
        for (int i = 0; i < chunksToKeepAhead; i++)
        {
            SpawnProceduralChunk(false);
        }
    }

    void Update()
    {
        // adds new ground as the player moves forward
        if (player.position.x > nextSpawnX - (chunkWidth * chunksToKeepAhead))
        {
            SpawnProceduralChunk(false);
        }

        // removes old ground that is far behind to save memory
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

    void SpawnProceduralChunk(bool isTutorial)
    {
        GameObject prefabToSpawn = isTutorial ? startChunkPrefab : duneBasePrefab;

        if (prefabToSpawn == null) return;

        // uses a tiny offset to keep pieces from perfectly overlapping
        float safeX = nextSpawnX + (activeChunks.Count * 0.001f);
        GameObject chunk = Instantiate(prefabToSpawn, new Vector3(safeX, 0, 0), Quaternion.identity);
        chunk.transform.SetParent(this.transform);

        // tells the chunk to build its own random hills
        if (!isTutorial)
        {
            AutoBuildChunk gen = chunk.GetComponent<AutoBuildChunk>();
            if (gen != null)
            {
                gen.Generate();
            }
        }

        activeChunks.Add(chunk);
        nextSpawnX += chunkWidth; 
    }
}