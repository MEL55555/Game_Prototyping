using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursor : MonoBehaviour
{
    public RectTransform cursorImage;
    public Canvas canvas;
    public Camera uiCamera;

    void Start()
    {
        Cursor.visible = false;

        if (cursorImage != null)
            cursorImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (cursorImage == null || canvas == null || uiCamera == null) return;

        
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            uiCamera,
            out pos
        );

        cursorImage.localPosition = pos;
    }

    public void ShowCursor()
    {
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(true);
    }

    public void HideCursor()
    {
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(false);
    }
}