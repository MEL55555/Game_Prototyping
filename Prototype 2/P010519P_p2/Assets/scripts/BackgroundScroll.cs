using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    public DunePlayer player;
    public float scrollSpeedMultiplier = 0.05f;

    private Material _mat;
    private Vector2 _offset;
    private string _texturePropertyName = "_BaseMap";

    void Start()
    {
        // grab material from the renderer component
        _mat = GetComponent<Renderer>().material;

        // check shader property names - urp usually uses basemap
        if (!_mat.HasProperty(_texturePropertyName))
        {
            _texturePropertyName = "_MainTex";
        }
    }

    void Update()
    {
        // skip if player or rb reference is missing
        if (player == null || player.rb == null) return;

        float speed = player.rb.linearVelocity.x;

        // calc offset based on player velocity 
        // use unscaled delta so it still scrolls during slow-mo bits
        _offset.x += (speed * scrollSpeedMultiplier) * Time.unscaledDeltaTime;

        // apply the new offset to the shader
        _mat.SetTextureOffset(_texturePropertyName, _offset);
    }

    private void OnDestroy()
    {
        // tidy up the instantiated material to stop memory leaks
        if (_mat != null)
        {
            Destroy(_mat);
        }
    }
}