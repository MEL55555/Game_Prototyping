using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class AutoBuildChunk : MonoBehaviour
{
    [Header("Dune Geometry")]
    public float chunkWidth = 120f;      
    public float chunkDepth = 25f;      
    
    [Header("Base Variety Settings")]
    public float baseNoiseScale = 0.03f; 
    public float baseDuneHeight = 14f;
    
    [Header("Score Scaling")]
    public float noiseIncreaseRate = 0.005f;
    public float maxNoiseScale = 0.08f;
    public float heightIncreaseRate = 2f;
    public float absoluteMaxHeight = 40f;
    
    [Header("Generation")]
    public float seed = 0f;             

    [Header("Resolution")]
    [Range(30, 100)] 
    public int smoothness = 60; 

    private float _currentNoiseScale;
    private float _currentMaxHeight;

    public float Generate() 
    {
        return BuildDune();
    }

    [ContextMenu("Build Rolling Dunes")]
    public float BuildDune() 
    {
        // gets the spline component to start drawing the ground
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        _currentNoiseScale = baseNoiseScale;
        _currentMaxHeight = baseDuneHeight;

        // makes the game harder based on the player score
        if (ScoreManager.Instance != null)
        {
            float score = ScoreManager.Instance.GetScore();
            float difficultyMultiplier = score / 1000f;

            // hills get more frequent as score goes up
            _currentNoiseScale += difficultyMultiplier * noiseIncreaseRate;
            _currentNoiseScale = Mathf.Min(_currentNoiseScale, maxNoiseScale);

            // hills get taller as score goes up
            _currentMaxHeight += difficultyMultiplier * heightIncreaseRate;
            _currentMaxHeight = Mathf.Min(_currentMaxHeight, absoluteMaxHeight);
        }

        // uses a random number so every chunk looks different
        float currentSeed = (seed == 0) ? Random.Range(-10000f, 10000f) : seed;

        // creates points along the top of the dune
        for (int i = 0; i <= smoothness; i++)
        {
          float xPos = (chunkWidth / smoothness) * i;

            // use perlin noise to get smooth random waves
            float sampleX = (xPos + currentSeed) * _currentNoiseScale;
            float noiseValue = Mathf.PerlinNoise(sampleX, 0f);
            
            // math to make the ground curve up and down like a real dune
          float yPos = (-Mathf.Cos((xPos / chunkWidth) * Mathf.PI * 2) * _currentMaxHeight) + _currentMaxHeight;
            yPos *= noiseValue; 

            // adds the point and makes it curvy
            spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            // handles the very start and end of the chunk
            if (i == 0 || i == smoothness)
            {
                float tangentLength = (chunkWidth / smoothness) * 0.5f;
                spline.SetLeftTangent(i, new Vector3(-tangentLength, 0, 0));
                spline.SetRightTangent(i, new Vector3(tangentLength, 0, 0));
            }
        }

        // adds points at the bottom to fill in the shape
        int nextIndex = smoothness + 1;
        spline.InsertPointAt(nextIndex, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex, ShapeTangentMode.Linear);
        spline.InsertPointAt(nextIndex + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex + 1, ShapeTangentMode.Linear);
        
        // updates the visual mesh
        shape.RefreshSpriteShape();
        
        return chunkWidth;
    }
}