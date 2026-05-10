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
        // access the spline data for shape manipulation
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        _currentNoiseScale = baseNoiseScale;
        _currentMaxHeight = baseDuneHeight;

        // scaling difficulty based on player progression
        if (ScoreManager.Instance != null)
        {
            float score = ScoreManager.Instance.GetScore();
            float difficultyMultiplier = score / 1000f;

            // bump up frequency and height but cap them at max values
            _currentNoiseScale += difficultyMultiplier * noiseIncreaseRate;
            _currentNoiseScale = Mathf.Min(_currentNoiseScale, maxNoiseScale);

            _currentMaxHeight += difficultyMultiplier * heightIncreaseRate;
            _currentMaxHeight = Mathf.Min(_currentMaxHeight, absoluteMaxHeight);
        }

        // grab a random seed if one hasn't been specified
        float currentSeed = (seed == 0) ? Random.Range(-10000f, 10000f) : seed;

        // main loop for plotting surface points
        for (int i = 0; i <= smoothness; i++)
        {
            float xPos = (chunkWidth / smoothness) * i;

            // sampling perlin noise for organic variance
            float sampleX = (xPos + currentSeed) * _currentNoiseScale;
            float noiseValue = Mathf.PerlinNoise(sampleX, 0f);

            // apply cosine curve to smooth out the transitions between chunks
            float yPos = (-Mathf.Cos((xPos / chunkWidth) * Mathf.PI * 2) * _currentMaxHeight) + _currentMaxHeight;
            yPos *= noiseValue;

            // plot point and set to continuous for smooth bezier curves
            spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            // ensure tangents at start/end points are horizontal for seamless tiling
            if (i == 0 || i == smoothness)
            {
                float tangentLength = (chunkWidth / smoothness) * 0.5f;
                spline.SetLeftTangent(i, new Vector3(-tangentLength, 0, 0));
                spline.SetRightTangent(i, new Vector3(tangentLength, 0, 0));
            }
        }

        // close off the shape at the bottom corners
        int nextIndex = smoothness + 1;
        spline.InsertPointAt(nextIndex, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex, ShapeTangentMode.Linear);
        spline.InsertPointAt(nextIndex + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(nextIndex + 1, ShapeTangentMode.Linear);

        // bake the changes into the visual mesh
        shape.RefreshSpriteShape();

        return chunkWidth;
    }
}