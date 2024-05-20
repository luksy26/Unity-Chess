using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Game : MonoBehaviour {
    public static Game Instance { get; private set; }
    public GameObject[,] currentPieces;
    public List<GameObject> blackPieces, whitePieces;
    public GameObject chessPiecePrefab;
    public char currentPlayer;
    public string playerPerspective;

    public void Start() {
        //playerPerspective = "white";
        currentPieces = new GameObject[8, 8];
        blackPieces = new();
        whitePieces = new();
    }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public void GeneratePieces() {
        GameState gameState = GameStateManager.Instance.gameState;
        char[,] boardConfiguration = gameState.boardConfiguration;
        currentPlayer = gameState.whoMoves;

        for (int i = 0; i < boardConfiguration.GetLength(0); ++i) {
            for (int j = 0; j < boardConfiguration.GetLength(1); ++j) {
                char potentialPiece = boardConfiguration[i, j];
                char file = (char)(j + 'a');
                int rank = 8 - i;
                string pieceName = GetPieceName(potentialPiece);
                if (pieceName != "") {
                    currentPieces[i, j] = CreatePieceSprite(pieceName, file, rank);
                    if (char.IsUpper(potentialPiece)) {
                        whitePieces.Add(currentPieces[i, j]);
                    } else {
                        blackPieces.Add(currentPieces[i, j]);
                    }
                }
            }
        }
    }

    public GameObject CreatePieceSprite(string name, char file, int rank) {

        GameObject obj = Instantiate(chessPiecePrefab, new Vector3(0, 0, -0.01f), Quaternion.identity);

        obj.name = name;
        obj.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteFactory>().GetSprite(name);

        PiecePlacer placer = obj.GetComponent<PiecePlacer>();
        placer.SetFile(file);
        placer.SetRank(rank);
        placer.SetGlobalCoords(playerPerspective);
        return obj;
    }
    public void DestroyPieces() {
        for (int i = 0; i < currentPieces.GetLength(0); ++i) {
            for (int j = 0; j < currentPieces.GetLength(1); ++j) {
                if (currentPieces[i, j] != null) {
                    blackPieces.Remove(currentPieces[i, j]);
                    whitePieces.Remove(currentPieces[i, j]);
                    Destroy(currentPieces[i, j]);
                }
            }
        }
    }
    public void MovePiece(char old_file, int old_rank, char new_file, int new_rank) {
        GameStateManager manager = GameStateManager.Instance;
        int old_i = 8 - old_rank;
        int old_j = old_file - 'a';
        int new_i = 8 - new_rank;
        int new_j = new_file - 'a';
        bool promoting = false;
        string movedPieceName = currentPieces[old_i, old_j].name;
        if (currentPieces[new_i, new_j] != null) {
            Debug.Log("destroying " + currentPieces[new_i, new_j].name);
            if (currentPlayer == 'w') {
                blackPieces.Remove(currentPieces[new_i, new_j]);
            } else {
                whitePieces.Remove(currentPieces[new_i, new_j]);
            }
            Destroy(currentPieces[new_i, new_j]);
        }
        if (movedPieceName.Contains("pawn")) {
            if (new_i == 0 || new_i == 7) {
                promoting = true;
            } else {
                char enPassantFile = manager.gameState.enPassantFile;
                int enPassantRank = manager.gameState.enPassantRank;
                // pawn captured the en-passant target
                if (new_file == enPassantFile && new_rank == enPassantRank) {
                    if (currentPlayer == 'w') {
                        Debug.Log("destroying " + currentPieces[new_i + 1, new_j].name);
                        blackPieces.Remove(currentPieces[new_i + 1, new_j]);
                        Destroy(currentPieces[new_i + 1, new_j]);
                    } else {
                        Debug.Log("destroying " + currentPieces[new_i - 1, new_j].name);
                        whitePieces.Remove(currentPieces[new_i - 1, new_j]);
                        Destroy(currentPieces[new_i - 1, new_j]);
                    }
                }
            }
        }
        currentPieces[new_i, new_j] = currentPieces[old_i, old_j];
        currentPieces[old_i, old_j] = null;

        PiecePlacer placer = currentPieces[new_i, new_j].GetComponent<PiecePlacer>();
        placer.SetFile(new_file);
        placer.SetRank(new_rank);
        placer.SetGlobalCoords(playerPerspective);
        if (promoting) {
            string new_name = currentPlayer == 'w' ? "white_queen" : "black_queen";
            currentPieces[new_i, new_j].name = new_name;
            currentPieces[new_i, new_j].GetComponent<SpriteRenderer>().sprite =
                currentPieces[new_i, new_j].GetComponent<SpriteFactory>()
                    .GetSprite(new_name);

        }

        manager.MovePiece(old_i, old_j, new_i, new_j);

    }
    public void SwapPlayer() {
        if (currentPlayer == 'w') {
            currentPlayer = 'b';
        } else {
            currentPlayer = 'w';
        }
    }
    private string GetPieceName(char x) {
        return x switch {
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
}
