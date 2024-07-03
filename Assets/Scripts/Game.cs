using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static PositionCounter;
using static MoveGenerator;
using TMPro;

public class Game : MonoBehaviour {
    public static Game Instance { get; private set; }
    public PromotionManager promotionManager;
    public GameObject[,] currentPieces;
    public List<GameObject> blackPieces, whitePieces;
    public List<GameObject> highlightedSquares, hintSquares;
    public GameObject chessPiecePrefab, highlightedEmptySquarePrefab,
        highlightedOccupiedSquarePrefab, hintSquarePrefab;
    public TMP_Text prompt;
    public string promptText = "";
    public bool activeTutorial;
    public bool tutorialMoving;
    public Move tutorialMove;
    public Move hintMove = null;
    public bool soundActive;
    public char currentPlayer;
    public string playerPerspective;
    public int AItoUse;
    public char AIPlayer;
    public int gameTreeMaxDepth;
    public float timeToMove;
    public bool timeNotExpired;
    Coroutine moveTimerCoroutine;
    public bool salvageMove;
    public Hashtable gameStates;

    public void Start() {
        currentPieces = new GameObject[8, 8];
        blackPieces = new();
        whitePieces = new();
        gameStates = new();
        promotionManager = GetComponent<PromotionManager>();
        tutorialMoving = false;
        soundActive = true;
    }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public async void GeneratePosition() {
        activeTutorial = false;
        Color betterWhite = Color.white;
        betterWhite.a = 150f / 255f;
        prompt.color = betterWhite;
        if (AItoUse > 0) {
            promptText = "You're up against AIv" + AItoUse + "!";
            prompt.text = promptText;
        }
        GetComponent<SquareCoordinatesUI>().GenerateFilesAndRanks(playerPerspective);
        GameState globalGameState = GameStateManager.Instance.globalGameState;

        // add the gameState in the hashtable
        gameStates.Add(globalGameState, 1);

        char[,] boardConfiguration = globalGameState.boardConfiguration;
        currentPlayer = globalGameState.whoMoves;
        hintMove = null;

        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
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

        if (currentPlayer == AIPlayer && currentPlayer != '-') {
            GameStateManager.Instance.IsEngineRunning = true;
            MoveEval bestMove = new();
            // start the timer to move
            StartCoroutine(MoveTimerCoroutine(timeToMove));
            await Task.Run(() => bestMove = SolvePosition());
            MovePiece(new Move(bestMove.move));
            GameStateManager.Instance.IsEngineRunning = false;
            await Task.Yield();
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

    public async void MovePiece(Move move) {
        if (currentPlayer == '-') { // game is over or tutorial is finished
            return;
        }
        bool localTutorialMoving = tutorialMoving;
        bool isCapture, isPromoting, isCastling;
        isCapture = isPromoting = isCastling = false;
        DestroyHintSquares();
        // not correct tutorial move
        if (activeTutorial && tutorialMove != null && 
            ((tutorialMove.promotesInto == '-' && !move.ToString().Equals(tutorialMove.ToString())) || !shallowCompareMoves(tutorialMove, move))) {
            IndexMove indexMove2 = new(move);
            currentPieces[indexMove2.oldRow, indexMove2.oldColumn].GetComponent<PiecePlacer>().SetFile(move.oldFile);
            currentPieces[indexMove2.oldRow, indexMove2.oldColumn].GetComponent<PiecePlacer>().SetRank(move.oldRank);
            currentPieces[indexMove2.oldRow, indexMove2.oldColumn].GetComponent<PiecePlacer>().SetGlobalCoords(playerPerspective);
            return;
        }
        GameState gameState = GameStateManager.Instance.globalGameState;
        IndexMove indexMove = new(move);
        string movedPieceName = currentPieces[indexMove.oldRow, indexMove.oldColumn].name;
        // check if a pawn moved
        if (movedPieceName.Contains("pawn")) {
            // check if the pawn reached the last rank
            if (indexMove.newRow == 0 || indexMove.newRow == 7) {
                isPromoting = true;
            } else {
                char enPassantFile = gameState.enPassantFile;
                int enPassantRank = gameState.enPassantRank;
                // pawn captured the en-passant target
                if (move.newFile == enPassantFile && move.newRank == enPassantRank) {
                    isCapture = true;
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
                isCastling = true;
                currentPieces[indexMove.newRow, 7].GetComponent<PieceMover>().isDragging = false;
                rookPlacer = currentPieces[indexMove.newRow, 7].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(move.newFile - 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[indexMove.newRow, 5] = currentPieces[indexMove.newRow, 7];
                currentPieces[indexMove.newRow, 7] = null;
            } else { // long castle
                isCastling = true;
                currentPieces[indexMove.newRow, 0].GetComponent<PieceMover>().isDragging = false;
                rookPlacer = currentPieces[indexMove.newRow, 0].GetComponent<PiecePlacer>();
                rookPlacer.SetFile((char)(move.newFile + 1));
                rookPlacer.SetGlobalCoords(playerPerspective);
                currentPieces[indexMove.newRow, 3] = currentPieces[indexMove.newRow, 0];
                currentPieces[indexMove.newRow, 0] = null;
            }
        }

        if (isPromoting) {
            string new_name;
            if (move.promotesInto == '-') {
                // make the promoted pawn temporarily invisible
                currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = false;
                GameStateManager.Instance.isPromotionMenuDisplayed = true;
                new_name = await promotionManager.GeneratePromotionMenu(playerPerspective, currentPlayer, move.newFile);
                // promotion is cancelled due to reset position/swap perspectives button
                if (new_name == null) {
                    GameStateManager.Instance.isPromotionMenuDisplayed = false;
                    currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = true;
                    return;
                }
                GameStateManager.Instance.isPromotionMenuDisplayed = false;
                // promotion was cancelled, but game does not reset
                if (new_name.Equals("") || (tutorialMove != null && new_name[0] != 'k')) {
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
            }

            // get the name for the new sprite
            new_name = GetPieceName(indexMove.promotesInto);
            currentPieces[indexMove.oldRow, indexMove.oldColumn].name = new_name;
            currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().sprite =
                GetComponent<SpriteFactory>().GetSprite(new_name);
            // make the pawn visible again
            currentPieces[indexMove.oldRow, indexMove.oldColumn].GetComponent<SpriteRenderer>().enabled = true;
        }

        // Destroy old piece (if the square was occupied), update internal GameObject array, and reposition the moved piece
        if (!isCapture) {
            isCapture = DestroyPieceAt(indexMove.newRow, indexMove.newColumn);
        } else {
            DestroyPieceAt(indexMove.newRow, indexMove.newColumn);
        }
        currentPieces[indexMove.newRow, indexMove.newColumn] = currentPieces[indexMove.oldRow, indexMove.oldColumn];
        currentPieces[indexMove.oldRow, indexMove.oldColumn] = null;
        PiecePlacer placer = currentPieces[indexMove.newRow, indexMove.newColumn].GetComponent<PiecePlacer>();
        currentPieces[indexMove.newRow, indexMove.newColumn].GetComponent<PieceMover>().isDragging = false;
        placer.SetFile(move.newFile);
        placer.SetRank(move.newRank);
        placer.SetGlobalCoords(playerPerspective);

        SwapPlayer();
        // make the move to update the gameState
        gameState.MakeMoveNoHashtable(indexMove);
        hintMove = null;

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

        if (currentPlayer != '-') {
            if (GameStateManager.Instance.IsKingInCheck(gameState)) {
                GetComponent<AudioManager>().PlaySound("check");
            } else if (isPromoting) {
                GetComponent<AudioManager>().PlaySound("promote");
            } else if (isCastling) {
                GetComponent<AudioManager>().PlaySound("castle");
            } else if (isCapture) {
                GetComponent<AudioManager>().PlaySound("capture");
            } else {
                GetComponent<AudioManager>().PlaySound("move");
            }
        }

        await Task.Yield();

        // get the move for the AI
        if (currentPlayer == AIPlayer && currentPlayer != '-') {
            GameStateManager.Instance.IsEngineRunning = true;
            MoveEval bestMove = new();
            // start the timer to move
            StartCoroutine(MoveTimerCoroutine(timeToMove));
            await Task.Run(() => bestMove = SolvePosition());
            MovePiece(new Move(bestMove.move));
            GameStateManager.Instance.IsEngineRunning = false;
        }
        if (activeTutorial && !localTutorialMoving) {
            GetComponent<TutorialManager>().ContinueTutorial();
        }
    }

    public void CancelMovePiece() {
        promotionManager.CancelPromotionMenu();
    }

    public void GetStaticPositionEval() {
        GameState gameState = GameStateManager.Instance.globalGameState;
        float eval = AIv6.PositionEvaluator(gameState, 0, GetLegalMoves(gameState));
        UnityEngine.Debug.Log("Static position evaluation:" + eval);
        promptText = "Static position evaluation:\n" + eval;
        prompt.text = promptText;
    }

    public async void GetPositionEval() {
        if (currentPlayer == '-') {
            promptText += "\nGame is over";
            prompt.text = promptText;
            UnityEngine.Debug.Log("Game is over");
            return;
        }
        GameStateManager.Instance.IsEngineRunning = true;
        MoveEval bestMove;
        if (moveTimerCoroutine != null) {
            StopCoroutine(moveTimerCoroutine);
        }
        moveTimerCoroutine = StartCoroutine(MoveTimerCoroutine(timeToMove));
        prompt.text = "Thinking...";
        await Task.Run(() => bestMove = SolvePosition());
        GameStateManager.Instance.IsEngineRunning = false;
        prompt.text = promptText;
    }

    public async void GetSizeOfGameTree() {
        prompt.text = "Exploring tree...";
        await Task.Run(() => RunPerft(GameStateManager.Instance.globalGameState));
        prompt.text = promptText;
    }

    public async void ShowHint() {
        if (currentPlayer == '-') {
            UnityEngine.Debug.Log("game is over");
            return;
        }
        if (hintMove != null) {
            UnityEngine.Debug.Log("hint already " + hintMove);
            return;
        }
        if (GameStateManager.Instance.IsEngineRunning) {
            UnityEngine.Debug.Log("not your turn or already getting hint");
            return;
        }
        if (moveTimerCoroutine != null) {
            StopCoroutine(moveTimerCoroutine);
        }
        promptText = "Thinking...";
        prompt.text = promptText;
        UnityEngine.Debug.Log("Thinking...");
        moveTimerCoroutine = StartCoroutine(MoveTimerCoroutine(timeToMove));
        GameStateManager.Instance.IsEngineRunning = true;
        hintMove = await Task.Run(() => GetHint());
        GameStateManager.Instance.IsEngineRunning = false;
        promptText += "\nTry moving there!";
        prompt.text = promptText;
        UnityEngine.Debug.Log("got hint " + hintMove);

        GameObject obj1 = Instantiate(hintSquarePrefab, new Vector3(0, 0, -0.011f), Quaternion.identity);
        GameObject obj2 = Instantiate(hintSquarePrefab, new Vector3(0, 0, -0.011f), Quaternion.identity);
        SetHintSquarePosition(obj1, hintMove.oldFile, hintMove.oldRank);
        SetHintSquarePosition(obj2, hintMove.newFile, hintMove.newRank);
        hintSquares.Add(obj1);
        hintSquares.Add(obj2);
    }

    public Move GetHint() {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        timeNotExpired = true;
        MoveEval moveToMakeFound = null;
        MoveEval mandatoryMove = null;
        int searchDepth = 1;
        int maxSearchDepth = 10;
        while (searchDepth <= maxSearchDepth) {
            MoveEval moveToMake = AIv6.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates);
            if (timeNotExpired || (salvageMove && Math.Abs(moveToMake.score) != 10000)) {
                moveToMakeFound = moveToMake;
                UnityEngine.Debug.Log("best move at depth " + searchDepth + " " + new Move(moveToMakeFound.move) +
                " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));
                if (!timeNotExpired) {
                    UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", but we salvaged the best move");
                    break;
                }
            } else {
                UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", and can't salvage the best move");
                break;
            }
            // now search deeper
            ++searchDepth;
        }
        stopwatch.Stop();
        if (searchDepth <= maxSearchDepth) {
            UnityEngine.Debug.Log("best move found in " + timeToMove + "s " + new Move(moveToMakeFound.move) +
                " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));
        } else {
            UnityEngine.Debug.Log("move found at depth " + maxSearchDepth + " " + new Move(moveToMakeFound.move) +
            " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
            (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score) +
            " in " + stopwatch.ElapsedMilliseconds + "ms");
        }
        return new Move(moveToMakeFound.move);
    }

    public MoveEval SolvePosition() {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        timeNotExpired = true;
        MoveEval moveToMakeFound = null;
        MoveEval mandatoryMove = null;
        int searchDepth = 1;
        int maxSearchDepth = 10;
        promptText = "";
        while (searchDepth <= maxSearchDepth) {
            MoveEval moveToMake = new();
            switch (AItoUse) {
                case 1: moveToMake = AIv2.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove); break;
                case 2: moveToMake = AIv3.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove); break;
                case 3: moveToMake = AIv4.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates); break;
                case 4: moveToMake = AIv5.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates); break;
                case 5: moveToMake = AIv6.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates); break;
                case 6: moveToMake = AIv7.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates); break;
                case 7: moveToMake = AIv8.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, gameStates); break;
                default: break;
            }
            if (timeNotExpired || salvageMove && Math.Abs(moveToMake.score) != 10000) {
                moveToMakeFound = moveToMake;
                string information = "best move at depth " + searchDepth + " " + new Move(moveToMakeFound.move) +
                " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score);
                UnityEngine.Debug.Log(information);
                promptText += information + "\n";
                if (!timeNotExpired) {
                    information = "time expired while searching at depth" + searchDepth + ", but we salvaged the best move";
                    promptText += information + "\n";
                    UnityEngine.Debug.Log(information);
                    break;
                }
            } else {
                string information = "time expired while searching at depth" + searchDepth + ", and can't salvage the best move";
                promptText += information + "\n";
                UnityEngine.Debug.Log(information);
                break;
            }
            // now search deeper
            ++searchDepth;
        }
        stopwatch.Stop();
        if (searchDepth <= maxSearchDepth) {
            string information = "best move found in " + timeToMove + "s " + new Move(moveToMakeFound.move) +
                " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score);
            UnityEngine.Debug.Log(information);
            promptText += information;
        } else {
            string information = "move found at depth " + maxSearchDepth + " " + new Move(moveToMakeFound.move) +
            " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
            (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score) +
            " in " + stopwatch.ElapsedMilliseconds + "ms";
            UnityEngine.Debug.Log(information);
            promptText += information;
        }
        return moveToMakeFound;
    }

    public IEnumerator MoveTimerCoroutine(float duration) {
        yield return new WaitForSeconds(duration);
        timeNotExpired = false;
    }

    public bool DestroyPieceAt(int row, int column) {
        if (currentPieces[row, column] != null) {
            if (currentPlayer == 'w') {
                blackPieces.Remove(currentPieces[row, column]);
            } else {
                whitePieces.Remove(currentPieces[row, column]);
            }
            Destroy(currentPieces[row, column]);
            return true;
        }
        return false;
    }

    public void RunPerft(GameState gameState) {
        GameStateManager.Instance.IsEngineRunning = true;
        string outPath = Path.Combine(Application.streamingAssetsPath, "mine.txt");
        using StreamWriter writer = File.CreateText(outPath);
        promptText = "";
        for (int depth = 1; depth <= gameTreeMaxDepth; ++depth) {
            maxDepth = depth;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            long posNumber = SearchPositions(gameState, 0, writer);
            UnityEngine.Debug.Log("Number of possible positions for " + maxDepth + " moves: " + posNumber);
            promptText += "\nNumber of possible positions for " + maxDepth + " moves: " + posNumber + hintMove;
            stopwatch.Stop();
            UnityEngine.Debug.Log("For depth " + depth + " time is " + stopwatch.ElapsedMilliseconds + "ms");
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks1 / Stopwatch.Frequency * 1000 + "ms spent getting legal moves");
            GameStateManager.Instance.numberOfTicks1 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks2 / Stopwatch.Frequency * 1000 + "ms spent getting legal pawn moves");
            GameStateManager.Instance.numberOfTicks2 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks3 / Stopwatch.Frequency * 1000 + "ms spent getting legal bishop moves");
            GameStateManager.Instance.numberOfTicks3 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks4 / Stopwatch.Frequency * 1000 + "ms spent getting legal knight moves");
            GameStateManager.Instance.numberOfTicks4 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks5 / Stopwatch.Frequency * 1000 + "ms spent getting legal rook moves");
            GameStateManager.Instance.numberOfTicks5 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks7 / Stopwatch.Frequency * 1000 + "ms spent getting legal king moves");
            GameStateManager.Instance.numberOfTicks7 = 0;
            UnityEngine.Debug.Log(1.0f * GameStateManager.Instance.numberOfTicks6 / Stopwatch.Frequency * 1000 + "ms spent in isKingSafeAt");
            GameStateManager.Instance.numberOfTicks6 = 0;
        }
        GameStateManager.Instance.IsEngineRunning = false;
    }

    public void HandleGameState(GameState gameState, Hashtable gameStates) {
        switch (GameStateManager.Instance.GetDrawConclusion(gameState, gameStates)) {
            case GameConclusion.DrawBy50MoveRule: {
                    currentPlayer = '-';
                    promptText = prompt.text + "\n\nDraw by 50 move rule";
                    prompt.text = promptText;
                    Color betterYellow = Color.yellow;
                    betterYellow.a = 150f / 255f;
                    prompt.color = betterYellow;
                    UnityEngine.Debug.Log("Draw by 50 move rule");
                    GetComponent<AudioManager>().PlaySound("end");
                    break;
                }
            case GameConclusion.DrawByRepetition: {
                    currentPlayer = '-';
                    promptText = prompt.text + "\n\nDraw by 3-fold repetition";
                    prompt.text = promptText;
                    Color betterYellow = Color.yellow;
                    betterYellow.a = 150f / 255f;
                    prompt.color = betterYellow;
                    UnityEngine.Debug.Log("Draw by 3-fold repetition");
                    GetComponent<AudioManager>().PlaySound("end");
                    break;
                }
            case GameConclusion.DrawByInsufficientMaterial: {
                    currentPlayer = '-';
                    promptText = prompt.text + "\n\nDraw by insufficient material";
                    prompt.text = promptText;
                    Color betterYellow = Color.yellow;
                    betterYellow.a = 150f / 255f;
                    prompt.color = betterYellow;
                    UnityEngine.Debug.Log("Draw by insufficient material");
                    GetComponent<AudioManager>().PlaySound("end");
                    break;
                }
            default:
                GameStateManager.Instance.globalLegalMoves = GetLegalMoves(gameState);
                switch (GameStateManager.Instance.GetMateConclusion(gameState, GameStateManager.Instance.globalLegalMoves)) {
                    case GameConclusion.Checkmate: {
                            promptText = prompt.text + "\n\nCheckmate! " + (currentPlayer == 'b' ? "White" : "Black") + " wins!";
                            prompt.text = promptText;
                            if (currentPlayer == AIPlayer || AIPlayer == '-') {
                                Color betterGreen = Color.green;
                                betterGreen.a = 150f / 255f;
                                prompt.color = betterGreen;
                            } else {
                                Color betterRed = Color.red;
                                betterRed.g = betterRed.b = 80f / 255f;
                                betterRed.a = 150f / 255f;
                                prompt.color = betterRed;
                            }
                            UnityEngine.Debug.Log("Checkmate! " + (currentPlayer == 'b' ? "White" : "Black") + " wins!");
                            currentPlayer = '-';
                            GetComponent<AudioManager>().PlaySound("end");
                            break;
                        }
                    case GameConclusion.Stalemate: {
                            promptText = prompt.text + "\n\nStalemate! Game is a draw since " + (currentPlayer == 'b' ? "Black" : "White") + " has no moves.";
                            prompt.text = promptText;
                            Color betterYellow = Color.yellow;
                            betterYellow.a = 150f / 255f;
                            prompt.color = betterYellow;
                            UnityEngine.Debug.Log("Stalemate! Game is a draw since " + (currentPlayer == 'b' ? "Black" : "White") + " has no moves.");
                            currentPlayer = '-';
                            GetComponent<AudioManager>().PlaySound("end");
                            break;
                        }
                }
                break;
        }
    }

    public void SwapPlayer() {
        if (currentPlayer == 'w') {
            currentPlayer = 'b';
        } else if (currentPlayer == 'b') {
            currentPlayer = 'w';
        }
    }

    public void SwapPerspectives() {
        promotionManager.CancelPromotionMenu();
        if (playerPerspective.Equals("white")) {
            playerPerspective = "black";
        } else {
            playerPerspective = "white";
        }
        for (int i = 0; i < blackPieces.Count; ++i) {
            blackPieces[i].GetComponent<PiecePlacer>().SetGlobalCoords(playerPerspective);
        }
        for (int i = 0; i < whitePieces.Count; ++i) {
            whitePieces[i].GetComponent<PiecePlacer>().SetGlobalCoords(playerPerspective);
        }
        GetComponent<SquareCoordinatesUI>().SwapPerspectivesForPieceCoordinates();
        if (hintMove != null) {
            SetHintSquarePosition(hintSquares[0], hintMove.oldFile, hintMove.oldRank);
            SetHintSquarePosition(hintSquares[1], hintMove.newFile, hintMove.newRank);
        }
    }

    public void SetHintSquarePosition(GameObject hintSquare, char file, int rank) {
        float global_x = file - 'a';
        float global_y = rank - 1;
        if (playerPerspective.Equals("black")) {
            global_x = 7 - global_x;
            global_y = 7 - global_y;
        }
        global_x -= 3.5f;
        global_y -= 3.5f;
        hintSquare.transform.position = new Vector3(global_x, global_y, -0.011f);
    }

    public void DestroyPosition() {
        if (!activeTutorial) {
            promptText = "";
            prompt.text = promptText;
        }
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
        // Destroy the notation for files and ranks
        GetComponent<SquareCoordinatesUI>().DestroyFilesAndRanks();
        DestroyHintSquares();
    }

    public void StartTutorial(int tutorial) {
        GetComponent<TutorialManager>().StartTutorial(tutorial);
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
    public void CreateHighlightedSquares(char file, int rank) {
        if (currentPlayer == AIPlayer) {
            return;
        }
        foreach (IndexMove move in GameStateManager.Instance.globalLegalMoves) {
            Move move2 = new(move);
            if (move2.oldFile == file && move2.oldRank == rank) {
                GameObject obj;
                if (currentPieces[move.newRow, move.newColumn] != null ||
                    EnPassant(move.newRow, move.newColumn)) { // square has a piece in it
                    obj = Instantiate(highlightedOccupiedSquarePrefab, new Vector3(0, 0, -0.019f), Quaternion.identity);
                } else { // square is empty
                    obj = Instantiate(highlightedEmptySquarePrefab, new Vector3(0, 0, -0.019f), Quaternion.identity);
                }
                float global_x = move2.newFile - 'a';
                float global_y = move2.newRank - 1;
                if (playerPerspective.Equals("black")) {
                    global_x = 7 - global_x;
                    global_y = 7 - global_y;
                }
                global_x -= 3.5f;
                global_y -= 3.5f;
                obj.transform.position = new Vector3(global_x, global_y, -0.019f);
                highlightedSquares.Add(obj);
            }
        }
    }
    public void DestroyHighLightedSquares() {
        foreach (GameObject obj in highlightedSquares) {
            Destroy(obj);
        }
        highlightedSquares.Clear();
    }
    public void DestroyHintSquares() {
        foreach (GameObject obj in hintSquares) {
            Destroy(obj);
        }
        hintSquares.Clear();
    }
    public bool EnPassant(int row, int column) {
        GameState gameState = GameStateManager.Instance.globalGameState;
        if (ColumnToFile(column) == gameState.enPassantFile && RowToRank(row) == gameState.enPassantRank) {
            return true;
        }
        return false;
    }
    public bool shallowCompareMoves(Move move1, Move move2) {
        if (move1.oldFile != move2.oldFile) {
            return false;
        }
        if (move1.oldRank != move2.oldRank) {
            return false;
        }
        if (move1.newFile != move2.newFile) {
            return false;
        }
        if (move1.newRank != move2.newRank) {
            return false;
        }
        return true;
    }
}
