using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture; // Assign your cursor texture in the Inspector
    public Vector2 hotSpot = Vector2.zero; // The offset of the cursor's "click point" from the top-left corner of the texture

    private void Start()
    {
        // Set the custom cursor with the specified texture and hot spot
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }

    private void OnDestroy()
    {
        // Reset the cursor to the default when the script is destroyed
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
