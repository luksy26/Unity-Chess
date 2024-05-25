using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    public static Game Instance { get; private set; }
    public GameObject[,] currentPieces;
    public List<GameObject> blackPieces, whitePieces;
    public GameObject chessPiecePrefab;
    public char currentPlayer;
    public string playerPerspective;

    Hashtable gameStates;

    public void Start() {
        //playerPerspective = "white";
        currentPieces = new GameObject[8, 8];
        blackPieces = new();
        whitePieces = new();
        gameStates = new();
    }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public void GeneratePosition() {
        GameState globalGameState = GameStateManager.Instance.globalGameState;

        // add the gameState in the hashtable
        gameStates.Add(globalGameState, 1);

        char[,] boardConfiguration = globalGameState.boardConfiguration;
        currentPlayer = globalGameState.whoMoves;

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
        obj.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteFactory>().GetSprite(name);

        PiecePlacer placer = obj.GetComponent<PiecePlacer>();
        placer.SetFile(file);
        placer.SetRank(rank);
        placer.SetGlobalCoords(playerPerspective);
        return obj;
    }

    public void MovePiece(char old_file, int old_rank, char new_file, int new_rank) {
        GameState gameState = GameStateManager.Instance.globalGameState;
        int old_i = 8 - old_rank;
        int old_j = old_file - 'a';
        int new_i = 8 - new_rank;
        int new_j = new_file - 'a';
        bool promoting = false;
        string movedPieceName = currentPieces[old_i, old_j].name;

        // check if we a piece was captured
        if (currentPieces[new_i, new_j] != null) {
            Debug.Log("destroying " + currentPieces[new_i, new_j].name);
            if (currentPlayer == 'w') {
                blackPieces.Remove(currentPieces[new_i, new_j]);
            } else {
                whitePieces.Remove(currentPieces[new_i, new_j]);
            }
            Destroy(currentPieces[new_i, new_j]);
        }
        // check if a pawn moved
        if (movedPieceName.Contains("pawn")) {
            // check if the pawn reached the last rank
            if (new_i == 0 || new_i == 7) {
                promoting = true;
            } else {
                char enPassantFile = gameState.enPassantFile;
                int enPassantRank = gameState.enPassantRank;
                // pawn captured the en-passant target
                if (new_file == enPassantFile && new_rank == enPassantRank) {
                    if (currentPlayer == 'w') {
                        blackPieces.Remove(currentPieces[new_i + 1, new_j]);
                        Destroy(currentPieces[new_i + 1, new_j]);
                        currentPieces[new_i + 1, new_j] = null;
                    } else {
                        whitePieces.Remove(currentPieces[new_i - 1, new_j]);
                        Destroy(currentPieces[new_i - 1, new_j]);
                        currentPieces[new_i - 1, new_j] = null;
                    }
                }
            }
        } else if (movedPieceName.Contains("king") && Math.Abs(new_j - old_j) == 2) { // the king has just castled
            // we need to change the rook's placement as well
            PiecePlacer rookPlacer;
            // short castle
            if (new_j > old_j) {
                rookPlacer = currentPieces[new_i, 7].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(new_file - 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[new_i, 5] = currentPieces[new_i, 7];
                currentPieces[new_i, 7] = null;
            } else { // long castle
                rookPlacer = currentPieces[new_i, 0].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(new_file + 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[new_i, 3] = currentPieces[new_i, 0];
                currentPieces[new_i, 0] = null;
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
        gameState.MovePiece(old_i, old_j, new_i, new_j);
        Debug.Log("GameState changed:");
        Debug.Log(gameState);
        if (gameStates.ContainsKey(gameState)) {
            int noOccurences = (int)gameStates[gameState];
            ++noOccurences;
            if (noOccurences == 3) {
                currentPlayer = '-';
                Debug.Log("Draw by 3-fold repetition");
            }
            gameStates[gameState] = noOccurences;
        } else {
            gameStates.Add(gameState, 1);
        }
    }
    public void SwapPlayer() {
        if (currentPlayer == 'w') {
            currentPlayer = 'b';
        } else if (currentPlayer == 'b') {
            currentPlayer = 'w';
        }
    }

    public void DestroyPosition() {
        for (int i = 0; i < currentPieces.GetLength(0); ++i) {
            for (int j = 0; j < currentPieces.GetLength(1); ++j) {
                if (currentPieces[i, j] != null) {
                    // Remove the GameObjects from the matrixx
                    blackPieces.Remove(currentPieces[i, j]);
                    whitePieces.Remove(currentPieces[i, j]);
                    // Destroy the objects
                    Destroy(currentPieces[i, j]);
                }
            }
        }
        // Reset the positions hashtable
        gameStates.Clear();
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
