using UnityEngine;

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

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
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