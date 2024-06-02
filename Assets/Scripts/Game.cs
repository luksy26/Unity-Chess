using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static PositionCounter;

public class Game : MonoBehaviour {
    public static Game Instance { get; private set; }
    public PromotionManager promotionManager;
    public GameObject[,] currentPieces;
    public List<GameObject> blackPieces, whitePieces;
    public GameObject chessPiecePrefab;
    public char currentPlayer;
    public string playerPerspective;
    Hashtable gameStates;

    public void Start() {
        currentPieces = new GameObject[8, 8];
        blackPieces = new();
        whitePieces = new();
        gameStates = new();
        promotionManager = GetComponent<PromotionManager>();
    }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public async void GeneratePosition() {
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

        HandleGameState(globalGameState, gameStates);

        await Task.Yield();
        await Task.Run(() => RunEngine(globalGameState));
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

    public async void MovePiece(Move move) {
        GameState gameState = GameStateManager.Instance.globalGameState;
        IndexMove indexMove = new(move);
        bool promoting = false;
        string movedPieceName = currentPieces[indexMove.oldRow, indexMove.oldColumn].name;

        // check if a pawn moved
        if (movedPieceName.Contains("pawn")) {
            // check if the pawn reached the last rank
            if (indexMove.newRow == 0 || indexMove.newRow == 7) {
                promoting = true;
            } else {
                char enPassantFile = gameState.enPassantFile;
                int enPassantRank = gameState.enPassantRank;
                // pawn captured the en-passant target
                if (move.newFile == enPassantFile && move.newRank == enPassantRank) {
                    if (currentPlayer == 'w') {
                        blackPieces.Remove(currentPieces[indexMove.newRow + 1, indexMove.newColumn]);
                        Destroy(currentPieces[indexMove.newRow + 1, indexMove.newColumn]);
                        currentPieces[indexMove.newRow + 1, indexMove.newColumn] = null;
                    } else {
                        whitePieces.Remove(currentPieces[indexMove.newRow - 1, indexMove.newColumn]);
                        Destroy(currentPieces[indexMove.newRow - 1, indexMove.newColumn]);
                        currentPieces[indexMove.newRow - 1, indexMove.newColumn] = null;
                    }
                }
            }
        } else if (movedPieceName.Contains("king") && Math.Abs(indexMove.newColumn - indexMove.oldColumn) == 2) { // the king has just castled
            // we need to change the rook's placement as well
            PiecePlacer rookPlacer;
            // short castle
            if (indexMove.newColumn > indexMove.oldColumn) {
                rookPlacer = currentPieces[indexMove.newRow, 7].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(move.newFile - 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[indexMove.newRow, 5] = currentPieces[indexMove.newRow, 7];
                currentPieces[indexMove.newRow, 7] = null;
            } else { // long castle
                rookPlacer = currentPieces[indexMove.newRow, 0].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(move.newFile + 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[indexMove.newRow, 3] = currentPieces[indexMove.newRow, 0];
                currentPieces[indexMove.newRow, 0] = null;
            }
        }

        if (promoting) {
            // make the promoted pawn temporarily invisible
            currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = false;
            GameStateManager.Instance.isPromotionMenuDisplayed = true;
            string new_name = await promotionManager.GeneratePromotionMenu(playerPerspective, currentPlayer, move.newFile);
            // promotion is cancelled due to reset position button
            if (new_name == null) {
                GameStateManager.Instance.isPromotionMenuDisplayed = false;
                return;
            }
            GameStateManager.Instance.isPromotionMenuDisplayed = false;
            // promotion was cancelled, but game does not reset
            if (new_name.Equals("")) {
                // make the piece visible and put it back
                currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = true;
                currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<PiecePlacer>().SetGlobalCoords(playerPerspective);
                return;
            }
            // set the piece selected for promotion
            move.promotesInto = new_name[0];
            if (move.promotesInto == 'k') {
                move.promotesInto = 'n';
            }
            if (currentPlayer == 'w') {
                move.promotesInto = char.ToUpper(move.promotesInto);
            }
            indexMove.promotesInto = move.promotesInto;
            // get the name for the new sprite
            new_name = (currentPlayer == 'w' ? "white" : "black") + "_" + new_name;
            currentPieces[indexMove.oldRow, indexMove.oldColumn].name = new_name;
            currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().sprite =
                GetComponent<SpriteFactory>().GetSprite(new_name);
            // make the pawn visible again
            currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = true;
        }

        // Destroy old piece (if the square was occupied), update internal GameObject array, and reposition the moved piece
        DestroyPieceAt(indexMove.newRow, indexMove.newColumn);
        currentPieces[indexMove.newRow, indexMove.newColumn] = currentPieces[indexMove.oldRow, indexMove.oldColumn];
        currentPieces[indexMove.oldRow, indexMove.oldColumn] = null;
        PiecePlacer placer = currentPieces[indexMove.newRow, indexMove.newColumn].GetComponent<PiecePlacer>();
        placer.SetFile(move.newFile);
        placer.SetRank(move.newRank);
        placer.SetGlobalCoords(playerPerspective);

        SwapPlayer();
        // make the move to update the gameState
        gameState.MovePiece(indexMove);
        UnityEngine.Debug.Log("GameState changed:");
        UnityEngine.Debug.Log(GameStateManager.Instance.globalGameState);

        // adding the gameState to the hashtable
        if (gameStates.ContainsKey(gameState)) {
            int noOccurences = (int)gameStates[gameState];
            ++noOccurences;
            gameStates[gameState] = noOccurences;
        } else {
            gameStates.Add(gameState, 1);
        }

        HandleGameState(gameState, gameStates);

        // wait for next frame
        await Task.Yield();
        await Task.Run(() => RunEngine(gameState));
    }

    public void CancelMovePiece() {
        promotionManager.CancelPromotionMenu();
    }

    public void DestroyPieceAt(int row, int column) {
        if (currentPieces[row, column] != null) {
            if (currentPlayer == 'w') {
                blackPieces.Remove(currentPieces[row, column]);
            } else {
                whitePieces.Remove(currentPieces[row, column]);
            }
            Destroy(currentPieces[row, column]);
        }
    }

    public void RunEngine(GameState gameState) {
        GameStateManager.Instance.IsEngineRunning = true;
        string outPath = Path.Combine(Application.streamingAssetsPath, "mine.txt");
        using StreamWriter writer = new(outPath, false);
        for (int depth = 1; depth < 6; ++depth) {
            maxDepth = depth;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            UnityEngine.Debug.Log("Number of possible positions for " + maxDepth + " moves: " + SearchPositions(gameState, 0, writer));
            stopwatch.Stop();
            UnityEngine.Debug.Log("For depth " + depth + " time is " + stopwatch.ElapsedMilliseconds + "ms");
        }
        GameStateManager.Instance.IsEngineRunning = false;
    }

    public void HandleGameState(GameState gameState, Hashtable gameStates) {
        switch (GameStateManager.Instance.GetGameConclusion(gameState, gameStates)) {
            case GameConclusion.DrawBy50MoveRule: {
                    currentPlayer = '-';
                    UnityEngine.Debug.Log("Draw by 50 move rule");
                    break;
                }
            case GameConclusion.DrawByRepetition: {
                    currentPlayer = '-';
                    UnityEngine.Debug.Log("Draw by 3-fold repetition");
                    break;
                }
            case GameConclusion.DrawByInsufficientMaterial: {
                    currentPlayer = '-';
                    UnityEngine.Debug.Log("Draw by insufficient material");
                    break;
                }
            case GameConclusion.Checkmate: {
                    UnityEngine.Debug.Log("Checkmate! " + (currentPlayer == 'b' ? "White" : "Black") + " wins!");
                    currentPlayer = '-';
                    break;
                }
            case GameConclusion.Stalemate: {
                    UnityEngine.Debug.Log("Stalemate! Game is a draw since " + (currentPlayer == 'b' ? "Black" : "White") + " has no moves.");
                    currentPlayer = '-';
                    break;
                }
            default: break;
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
