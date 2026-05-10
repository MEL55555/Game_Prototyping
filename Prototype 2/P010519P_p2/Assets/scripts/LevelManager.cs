using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public Transform player;

    [Header("Chunk Prefabs")]
    public GameObject startChunkPrefab;
    public GameObject duneBasePrefab;

    [Header("Sizing")]
    public float chunkWidth = 120f;

    [Header("Stream Settings")]
    public int chunksToKeepAhead = 6;
    public float deleteDistanceBehind = 300f;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float nextSpawnX = 0f;

    void Start()
    {
        // reset spawning pos and clear out the list for a fresh run
        nextSpawnX = 0f;
        activeChunks.Clear();

        if (startChunkPrefab != null)
        {
            // spawn the static tutorial piece to get the player moving
            GameObject tutorial = Instantiate(startChunkPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
            tutorial.transform.SetParent(this.transform);
            activeChunks.Add(tutorial);

            nextSpawnX += 120f;
        }

        // prime the level by spawning several chunks in front of the player
        for (int i = 0; i < chunksToKeepAhead; i++)
        {
            SpawnProceduralChunk(false);
        }
    }

    void Update()
    {
        // check if player is approaching the end of the current generated path
        if (player.position.x > nextSpawnX - (chunkWidth * chunksToKeepAhead))
        {
            SpawnProceduralChunk(false);
        }

        // tidy up old chunks that are well behind the player to keep memory usage low
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

        // add a microscopic offset to prevent any weird Z-fighting or overlap glitches
        float safeX = nextSpawnX + (activeChunks.Count * 0.001f);
        GameObject chunk = Instantiate(prefabToSpawn, new Vector3(safeX, 0, 0), Quaternion.identity);
        chunk.transform.SetParent(this.transform);

        // trigger the procedural generation on the new chunk if it's not a tutorial piece
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