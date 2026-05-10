using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class StartChunkBuilder : MonoBehaviour
{
    [Header("Dimensions")]
    public float chunkWidth = 120f;
    public float chunkDepth = 40f;

    [Header("Initial Slope")]
    public float startLowDepth = -14f;
    [Range(0f, 0.5f)]
    public float flatStartPercent = 0.2f;

    [Header("Resolution")]
    [Range(20, 100)]
    public int smoothness = 60;

    [ContextMenu("Build Low-To-High Start")]
    public void BuildStart()
    {
        // access the spline API for the SpriteShape
        SpriteShapeController shape = GetComponent<SpriteShapeController>();
        Spline spline = shape.spline;
        spline.Clear();

        for (int i = 0; i <= smoothness; i++)
        {
            float t = (float)i / smoothness;
            float xPos = t * chunkWidth;
            float yPos;

            // determine if we are in the flat entry zone or the rising ramp
            if (t < flatStartPercent)
            {
                // hold at the initial low depth for the player to gain control
                yPos = startLowDepth;
            }
            else
            {
                // calculate normalized progress through the ramp section
                float riseT = (t - flatStartPercent) / (1f - flatStartPercent);

                // use Cosine interpolation for a silky smooth entry and exit curve
                float smoothRise = (1f - Mathf.Cos(riseT * Mathf.PI)) / 2f;
                yPos = Mathf.Lerp(startLowDepth, 0, smoothRise);
            }

            // plot the top edge points of the terrain
            spline.InsertPointAt(i, new Vector3(xPos, yPos, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // close the shape by adding bottom-right and bottom-left anchors
        int last = smoothness + 1;
        spline.InsertPointAt(last, new Vector3(chunkWidth, -chunkDepth, 0));
        spline.SetTangentMode(last, ShapeTangentMode.Linear);

        spline.InsertPointAt(last + 1, new Vector3(0, -chunkDepth, 0));
        spline.SetTangentMode(last + 1, ShapeTangentMode.Linear);

        // force the mesh and collider to update based on the new spline data
        shape.RefreshSpriteShape();
    }
}