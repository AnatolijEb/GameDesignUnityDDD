using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonCursorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D handCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (handCursor != null)
        {
            Cursor.SetCursor(handCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            // If no texture is assigned, we could potentially use a system cursor if supported,
            // but for now we just log it as a placeholder.
            Debug.Log("[ButtonCursorHover] Hovering over " + gameObject.name + ". (No handCursor texture assigned)");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        // Ensure cursor is restored if button is disabled or hidden while hovering
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
