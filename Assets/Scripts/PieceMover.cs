using UnityEngine;

public class PieceMover : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    
    void Start()
    {
        // Ensure BoxCollider2D matches the size of the SpriteRenderer
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (boxCollider != null && spriteRenderer != null)
        {
            boxCollider.size = spriteRenderer.size;
        }
    }

    void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        offset.z = 0;  // Ensure z component remains unchanged
        isDragging = true;
        Debug.Log("Object clicked, initial position is" + transform.position);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition + offset;
            transform.position = new Vector3(transform.position.x, transform.position.y, -0.01f);
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            Debug.Log("Object dropped at position: " + transform.position);
        }
    }
}
