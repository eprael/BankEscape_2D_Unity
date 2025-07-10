using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public void SetHandCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        // Or use a custom hand texture:
        // Cursor.SetCursor(handCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void SetDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}