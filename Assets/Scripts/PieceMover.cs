using UnityEngine;

public class PieceMover : MonoBehaviour {
    private bool isDragging = false;
    private Vector3 initialPosition;
    private GameObject gameController;
    private Game currentGame;
    private MoveValidator validator;
    private static readonly float xMax = 4;
    private static readonly float yMax = 4;
    private static readonly float xMin = -4;
    private static readonly float yMin = -4;

    public void Start() {
        // Ensure BoxCollider2D matches the size of the SpriteRenderer
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameController = GameObject.FindGameObjectWithTag("GameController");
        currentGame = gameController.GetComponent<Game>();
        validator = gameController.GetComponent<MoveValidator>();

        if (boxCollider != null && spriteRenderer != null) {
            boxCollider.size = spriteRenderer.size;
        }
    }

    public void OnMouseDown() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
        initialPosition = transform.position;
        Debug.Log(name + " clicked, initial position is" + initialPosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, -0.02f);
    }

    public void OnMouseDrag() {
        if (isDragging) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float newX = mousePosition.x;
            float newY = mousePosition.y;

            if (newX > xMax || newX < xMin) {
                newX = transform.position.x;
            }
            if (newY > yMax || newY < yMin) {
                newY = transform.position.y;
            }
            transform.position = new Vector3(newX, newY, -0.02f);
        }
    }

    public void OnMouseUp() {
        if (isDragging) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            isDragging = false;
            bool backToStart = true;

            if (IsInBounds(mousePosition)) {
                string piece_owner = GetPieceOwner(name);
                if (currentGame.currentPlayer.Equals(piece_owner)) {
                    PiecePlacer placer = GetComponent<PiecePlacer>();
                    char old_file = placer.GetFile();
                    int old_rank = placer.GetRank();
                    char new_file = GetFile(mousePosition.x);
                    int new_rank = GetRank(mousePosition.y);
                    if (validator.IsLegalMove(old_file, old_rank, new_file, new_rank)) {
                        backToStart = false;
                        Debug.Log("new file is " + new_file + " new rank is " + new_rank);
                        currentGame.MovePiece(old_file, old_rank, new_file, new_rank);
                        placer.SetFile(new_file);
                        placer.SetRank(new_rank);
                        placer.SetGlobalCoords();
                    }
                }
            }
            if (backToStart) {
                transform.position = initialPosition;
            } else {
                currentGame.SwapPlayer();

            }
            Debug.Log(name + " dropped at position: " + transform.position);
        }
    }
    public bool IsInBounds(Vector3 pos) {
        if (pos.x < xMin || pos.x > xMax || pos.y < yMin || pos.y > yMax) {
            return false;
        }
        return true;
    }
    private char GetFile(float x) {
        return (char)('a' + (int)(x + 4));
    }
    private int GetRank(float y) {
        return (int)(y + 4) + 1;
    }
    private string GetPieceOwner(string piece_name) {
        if (piece_name.ToLower().Contains("white")) {
            return "white";
        }
        return "black";
    }
}
