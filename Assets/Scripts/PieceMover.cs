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
                char piece_owner = GetPieceOwner(name);
                if (currentGame.currentPlayer.Equals(piece_owner)) {
                    PiecePlacer placer = GetComponent<PiecePlacer>();
                    char oldFile = placer.GetFile();
                    int oldRank = placer.GetRank();
                    char newFile = GetFile(mousePosition.x, currentGame.playerPerspective);
                    int newRank = GetRank(mousePosition.y, currentGame.playerPerspective);
                    Move move = new(oldFile, oldRank, newFile, newRank);
                    if (validator.IsLegalMove(move, GameStateManager.Instance.globalGameState)) {
                        backToStart = false;
                        currentGame.MovePiece(move);
                    }
                }
            }
            if (backToStart) {
                transform.position = initialPosition;
            } else {
                currentGame.SwapPlayer();
            }
        }
    }
    public bool IsInBounds(Vector3 pos) {
        if (pos.x < xMin || pos.x >= xMax || pos.y < yMin || pos.y >= yMax) {
            return false;
        }
        return true;
    }
    private char GetFile(float x, string playerPerspective) {
        char file = (char)('a' + (int)(x + 4));
        if (playerPerspective.Equals("black")) {
            file = (char)('a' + ('h' - file));
        }
        return file;
    }
    private int GetRank(float y, string playerPerspective) {
        int rank = (int)(y + 4) + 1;
        if (playerPerspective.Equals("black")) {
            rank = 9 - rank;
        }
        return rank;
    }
    private char GetPieceOwner(string piece_name) {
        if (piece_name.Contains("white")) {
            return 'w';
        }
        return 'b';
    }
}
