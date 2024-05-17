using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{   
    public GameObject chessPiece;

    private GameObject[,] piecePositions = new GameObject[8, 8];
    private GameObject[] blackPieces = new GameObject[16];
    private GameObject[] whitePieces = new GameObject[16];

    private string currentPlayer = "white";

    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        whitePieces = new GameObject[] {
            CreatePieceSprite("white_rook", 'a', 1),
            CreatePieceSprite("white_knight", 'b', 1),
            CreatePieceSprite("white_bishop", 'c', 1),
            CreatePieceSprite("white_queen", 'd', 1),
            CreatePieceSprite("white_king", 'e', 1),
            CreatePieceSprite("white_bishop", 'f', 1),
            CreatePieceSprite("white_knight", 'g', 1),
            CreatePieceSprite("white_rook", 'h', 1),
            CreatePieceSprite("white_pawn", 'a', 2),
            CreatePieceSprite("white_pawn", 'b', 2),
            CreatePieceSprite("white_pawn", 'c', 2),
            CreatePieceSprite("white_pawn", 'd', 2),
            CreatePieceSprite("white_pawn", 'e', 2),
            CreatePieceSprite("white_pawn", 'f', 2),
            CreatePieceSprite("white_pawn", 'g', 2),
            CreatePieceSprite("white_pawn", 'h', 2)
        };
        blackPieces = new GameObject[] {
            CreatePieceSprite("black_rook", 'a', 8),
            CreatePieceSprite("black_knight", 'b', 8),
            CreatePieceSprite("black_bishop", 'c', 8),
            CreatePieceSprite("black_queen", 'd', 8),
            CreatePieceSprite("black_king", 'e', 8),
            CreatePieceSprite("black_bishop", 'f', 8),
            CreatePieceSprite("black_knight", 'g', 8),
            CreatePieceSprite("black_rook", 'h', 8),
            CreatePieceSprite("black_pawn", 'a', 7),
            CreatePieceSprite("black_pawn", 'b', 7),
            CreatePieceSprite("black_pawn", 'c', 7),
            CreatePieceSprite("black_pawn", 'd', 7),
            CreatePieceSprite("black_pawn", 'e', 7),
            CreatePieceSprite("black_pawn", 'f', 7),
            CreatePieceSprite("black_pawn", 'g', 7),
            CreatePieceSprite("black_pawn", 'h', 7)
        };

        for (int i = 0; i < blackPieces.Length; ++i) {
            SetPosition(blackPieces[i]);
        }
        for (int i = 0; i < whitePieces.Length; ++i) {
            SetPosition(whitePieces[i]);
        }
    }

    public GameObject CreatePieceSprite(string name, char file, int rank) {
        GameObject obj = Instantiate(chessPiece, new Vector3(0, 0, -0.01f), Quaternion.identity);
        PiecePlacer placer = obj.GetComponent<PiecePlacer>();
        placer.name = name;
        placer.SetFile(file);
        placer.SetRank(rank);
        placer.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj) {
        PiecePlacer placer = obj.GetComponent<PiecePlacer>();

        piecePositions[placer.GetFile(), placer.GetRank()] = obj;
    }
}
