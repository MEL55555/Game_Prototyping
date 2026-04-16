using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class StartChunkBuilder : MonoBehaviour
{
    [Header("Chunk Dimensions")]
    public float chunkWidth = 120f;
    public float chunkDepth = 40f; 
    
    [Header("Tutorial Slope Settings")]
    public float startLowDepth = -14f; 
    [Range(0f, 0.5f)]
    public float flatStartPercent = 0.2f; 

    [Header("Resolution")]
    [Range(20, 100)]
    public int smoothness = 60; 

    [ContextMenu("Build Low-To-High Start")]
    public void BuildStart()
    {
        // gets the tool to draw the ground shape
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        for (int i = 0; i <= smoothness; i++)
        {
            float t = (float)i / smoothness;
            float xPos = t * chunkWidth;
            float yPos;

            // decides if the ground stays flat or starts rising
            if (t < flatStartPercent)
            {
                // keeps the ground low at the very beginning
                yPos = startLowDepth;
            }
            else
            {
                // math to make a smooth curve back up to the top
                float riseT = (t - flatStartPercent) / (1f - flatStartPercent);
                
                // smooth step math so the ramp feels silky
                float smoothRise = (1f - Mathf.Cos(riseT * Mathf.PI)) / 2f;
              yPos = Mathf.Lerp(startLowDepth, 0, smoothRise);
            }

            // adds the point to the spline
          spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // adds the bottom points to fill the shape in
        int last = smoothness + 1;
        spline.InsertPointAt(last, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(last, ShapeTangentMode.Linear);
        
        spline.InsertPointAt(last + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(last + 1, ShapeTangentMode.Linear);
        
        // updates the sprite to show the new ground
        shape.RefreshSpriteShape();
    }
}