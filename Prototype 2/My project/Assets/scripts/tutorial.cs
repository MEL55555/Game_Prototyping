using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class StartChunkBuilder : MonoBehaviour
{
    [Header("Chunk Dimensions")]
    public float chunkWidth = 120f;
    public float chunkDepth = 40f; 
    
    [Header("Tutorial Slope Settings")]
    public float startLowDepth = -14f; // How deep the ball starts
    [Range(0f, 0.5f)]
    public float flatStartPercent = 0.2f; // How long it stays low before rising

    [Header("Resolution")]
    [Range(20, 100)]
    public int smoothness = 60; 

    [ContextMenu("Build Low-To-High Start")]
    public void BuildStart()
    {
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        for (int i = 0; i <= smoothness; i++)
        {
            float t = (float)i / smoothness;
            float xPos = t * chunkWidth;
            float yPos;

            if (t < flatStartPercent)
            {
                // Part 1: Stay at the low depth
                yPos = startLowDepth;
            }
            else
            {
                // Part 2: Rise back to 0
                // Remap 't' so it starts at 0 when we leave the flat part
                float riseT = (t - flatStartPercent) / (1f - flatStartPercent);
                
                // Using a Cosine lerp (Smooth Step) for a silky ramp
                // This starts at startLowDepth and ends exactly at 0
                float smoothRise = (1f - Mathf.Cos(riseT * Mathf.PI)) / 2f;
                yPos = Mathf.Lerp(startLowDepth, 0, smoothRise);
            }

            spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // Close the bottom shape
        int last = smoothness + 1;
        spline.InsertPointAt(last, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(last, ShapeTangentMode.Linear);
        
        spline.InsertPointAt(last + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(last + 1, ShapeTangentMode.Linear);
        
        shape.RefreshSpriteShape();
        Debug.Log($"Low-to-High Tutorial Built. Starting Depth: {startLowDepth}");
    }
}