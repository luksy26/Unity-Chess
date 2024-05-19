using UnityEngine;

public class PieceMover : MonoBehaviour
{
    private bool isDragging = false;
    
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
        isDragging = true;
        Debug.Log("Object clicked, initial position is" + transform.position);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, -0.02f);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, -0.02f);
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, -0.01f);
            Debug.Log("Object dropped at position: " + transform.position);
        }
    }
}
