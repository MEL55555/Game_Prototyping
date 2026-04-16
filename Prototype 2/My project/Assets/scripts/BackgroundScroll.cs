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
        // gets the material from the renderer
        _mat = GetComponent<Renderer>().material;

        // check if we use base map or main tex
        if (!_mat.HasProperty(_texturePropertyName))
        {
            _texturePropertyName = "_MainTex";
        }
    }

    void Update()
    {
        // dont do anything if player isnt there
        if (player == null || player.rb == null) return;

        float speed = player.rb.linearVelocity.x;
        
        // background keeps moving even during slow motion
        _offset.x += (speed * scrollSpeedMultiplier) * Time.unscaledDeltaTime;

        // moves the texture
        _mat.SetTextureOffset(_texturePropertyName, _offset);
    }
    
    private void OnDestroy()
    {
        // keeps things clean
        if (_mat != null)
        {
            Destroy(_mat);
        }
    }
}