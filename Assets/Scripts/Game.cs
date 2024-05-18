using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{      
    public static Game Instance { get; private set; }
    public GameObject[,] current_pieces;
    public GameObject chessPiecePrefab;

    private GameObject gameController;

    public string currentPlayer;

    public void Start() {
        gameController = GameObject.FindGameObjectWithTag("GameController");
        currentPlayer = "white";
        current_pieces = new GameObject[8, 8];
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GeneratePieces()
    {   
        char[,] boardConfiguration = InitialBoardConfiguration.Instance.boardConfiguration ;

        for (int i = 0; i < boardConfiguration.GetLength(0); ++i) {
            for (int j = 0; j < boardConfiguration.GetLength(1); ++j) {
                char potentialPiece = boardConfiguration[i, j];
                char file = (char)(j + 'a');
                int rank = 8 - i;
                string pieceName = GetPieceName(potentialPiece);
                if (pieceName != "") {
                    current_pieces[i, j] = CreatePieceSprite(pieceName, file, rank);
                }
            }
        }
    }

    public GameObject CreatePieceSprite(string name, char file, int rank) {
        Sprite piece_sprite = gameController.GetComponent<SpriteFactory>().GetSprite(name);
    
        GameObject obj = Instantiate(chessPiecePrefab, new Vector3(0, 0, -0.01f), Quaternion.identity);
        obj.GetComponent<SpriteRenderer>().sprite = piece_sprite;
    
        PiecePlacer placer = obj.GetComponent<PiecePlacer>();
        placer.SetFile(file);
        placer.SetRank(rank);
        placer.SetGlobalCoords();
        placer.name = name;
        obj.GetComponent<PieceMover>().name = name;
        return obj;
    }

    public string GetPieceName(char x) {
        return x switch
        {
            'r' => "black_rook",
            'n' => "black_knight",
            'b' => "black_bishop",
            'q' => "black_queen",
            'k' => "black_king",
            'p' => "black_pawn",
            'R' => "white_rook",
            'N' => "white_knight",
            'B' => "white_bishop",
            'Q' => "white_queen",
            'K' => "white_king",
            'P' => "white_pawn",
            _ => "",
        };
    }
    public void DestroyPieces() {
        for (int i = 0; i < current_pieces.GetLength(0); ++i) {
            for (int j = 0; j < current_pieces.GetLength(1); ++j) {
                if (current_pieces[i, j] != null) {
                    Destroy(current_pieces[i, j]);
                }
            }
        }
    }
}
