using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class AutoBuildChunk : MonoBehaviour
{
    [Header("Dune Geometry")]
    public float chunkWidth = 120f;      
    public float chunkDepth = 25f;      
    
    [Header("Base Variety Settings")]
    [Tooltip("Starting noise scale (0.03 is smooth).")]
    public float baseNoiseScale = 0.03f; 
    [Tooltip("Starting dune height.")]
    public float baseDuneHeight = 14f;
    
    [Header("Score Scaling")]
    [Tooltip("How much noise scale increases per 1000 points.")]
    public float noiseIncreaseRate = 0.005f;
    public float maxNoiseScale = 0.08f;
    [Space]
    [Tooltip("How much height increases per 1000 points.")]
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
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        // 1. DYNAMIC DIFFICULTY CALCULATION
        _currentNoiseScale = baseNoiseScale;
        _currentMaxHeight = baseDuneHeight;

        if (ScoreManager.Instance != null)
        {
            float score = ScoreManager.Instance.GetScore();
            float difficultyMultiplier = score / 1000f;

            // Scale Noise (How frequent hills are)
            _currentNoiseScale += difficultyMultiplier * noiseIncreaseRate;
            _currentNoiseScale = Mathf.Min(_currentNoiseScale, maxNoiseScale);

            // Scale Height (How tall hills are)
            _currentMaxHeight += difficultyMultiplier * heightIncreaseRate;
            _currentMaxHeight = Mathf.Min(_currentMaxHeight, absoluteMaxHeight);
        }

        // 2. RANDOM SEED
        float currentSeed = (seed == 0) ? Random.Range(-10000f, 10000f) : seed;

        for (int i = 0; i <= smoothness; i++)
        {
            float xPos = (chunkWidth / smoothness) * i;

            // 3. GENERATE NOISE
            float sampleX = (xPos + currentSeed) * _currentNoiseScale;
            float noiseValue = Mathf.PerlinNoise(sampleX, 0f);
            
            // 4. APPLY "DUNE" SHAPING WITH DYNAMIC HEIGHT
            // Uses _currentMaxHeight to determine the peak-to-valley distance
            float yPos = (-Mathf.Cos((xPos / chunkWidth) * Mathf.PI * 2) * _currentMaxHeight) + _currentMaxHeight;
            yPos *= noiseValue; 

            spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            if (i == 0 || i == smoothness)
            {
                float tangentLength = (chunkWidth / smoothness) * 0.5f;
                spline.SetLeftTangent(i, new Vector3(-tangentLength, 0, 0));
                spline.SetRightTangent(i, new Vector3(tangentLength, 0, 0));
            }
        }

        int nextIndex = smoothness + 1;
        spline.InsertPointAt(nextIndex, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex, ShapeTangentMode.Linear);
        spline.InsertPointAt(nextIndex + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex + 1, ShapeTangentMode.Linear);
        
        shape.RefreshSpriteShape();
        
        return chunkWidth;
    }
}