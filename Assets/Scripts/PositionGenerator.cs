using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static PositionCounter;
using static AIv2;
using System;

public class PositionGenerator : MonoBehaviour {
    public InputField inputField;
    public Button generateButton, runTests, swapPerspective, getPositionEval, getStaticPositionEval, evaluateEngine,
    getSizeOfGameTree;

    void Start() {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
        runTests.onClick.AddListener(OnRunTestsButtonClicked);
        swapPerspective.onClick.AddListener(OnSwapPerspectiveClicked);
        getPositionEval.onClick.AddListener(OnGetPositionEvalClicked);
        getStaticPositionEval.onClick.AddListener(OnGetStaticPositionEvalClicked);
        evaluateEngine.onClick.AddListener(OnEvaluateEngineButtonClicked);
        getSizeOfGameTree.onClick.AddListener(OnGetSizeOfGameTreeButtonClicked);
        Game.Instance.salvageMove = false;
    }

    void OnGenerateButtonClicked() {
        string inputFEN = inputField.text;
        if (!GameStateManager.Instance.IsEngineRunning) {
            Game.Instance.AIPlayer = '-';
            Game.Instance.playerPerspective = "white";
            Game.Instance.gameTreeMaxDepth = 4;
            Game.Instance.timeToMove = 5f;
            Game.Instance.timeNotExpired = true;
            Game.Instance.CancelMovePiece();
            GameStateManager.Instance.GenerateGameState(inputFEN);
            Game.Instance.DestroyPosition();
            Game.Instance.GeneratePosition();
        }
    }

    void OnGetSizeOfGameTreeButtonClicked() {
        Game.Instance.GetSizeOfGameTree();
    }

    async void OnEvaluateEngineButtonClicked() {
        Game.Instance.timeToMove = 5f;
        string filePath = Path.Combine(Application.streamingAssetsPath, "REF_Values.txt");
        string outPath = Path.Combine(Application.streamingAssetsPath, "engineEvaluation.txt");
        if (File.Exists(filePath)) {
            using StreamReader reader = new(filePath);
            using StreamWriter writer = new(outPath, false);
            string line;
            int idx = 0;
            line = reader.ReadLine(); // skip the header
            float sum = 0;
            int nr = 0;
            float maxDiff = 0;
            while ((line = reader.ReadLine()) != null) {
                if (idx % 1 != 0) {
                    ++idx;
                    continue; // only process one portion of the data
                }
                string[] parts = line.Split(',');
                string fen = parts[0];
                string bestMove = parts[1];
                string evaluation = parts[2];

                int mateValue = 0;
                float evaluationScore = 0;

                string[] partsEvaluation = evaluation.Split(" ");
                if (partsEvaluation.Length > 1) {
                    mateValue = int.Parse(partsEvaluation[1]);
                } else {
                    evaluationScore = int.Parse(evaluation);
                }

                UnityEngine.Debug.Log("processing FEN " + fen);
                GameStateManager.Instance.GenerateGameState(fen);
                GameStateManager.Instance.IsEngineRunning = true;
                StartCoroutine(Game.Instance.MoveTimerCoroutine(Game.Instance.timeToMove));
                Game.Instance.timeNotExpired = true;
                MoveEval moveToMakeFound = new();
                MoveEval mandatoryMoveFound = new() { move = new IndexMove(new Move(bestMove)) };
                int searchDepth = 1;
                while (true) {
                    MoveEval moveToMake = new();
                    MoveEval mandatoryMove = new() { move = new IndexMove(new Move(bestMove)) };
                    await Task.Run(() => moveToMake = GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove));
                    if (Game.Instance.timeNotExpired || (Game.Instance.salvageMove && Math.Abs(moveToMake.score) != 10000)) {
                        moveToMakeFound = moveToMake;
                        if (mandatoryMove.score != 10000) {
                            mandatoryMoveFound = mandatoryMove;
                        }
                        UnityEngine.Debug.Log("best move at depth " + searchDepth + " " + new Move(moveToMakeFound.move) +
                        " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                        (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));

                        if (!Game.Instance.timeNotExpired) {
                            UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", but we salvaged a move");
                            break;
                        }
                    } else {
                        UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", and can't salvage a move");
                        break;
                    }
                    // now search deeper
                    ++searchDepth;
                }
                UnityEngine.Debug.Log("best move found in " + Game.Instance.timeToMove + "s " + new Move(moveToMakeFound.move) +
                    " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                    (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));
                int foundMateValue = 0;
                float foundEvaluationScore = mandatoryMoveFound.score;
                if (Math.Abs(mandatoryMoveFound.score) > 950) {
                    foundMateValue = (int)((Math.Abs(Math.Abs(mandatoryMoveFound.score) - 1000) + Math.Abs(mandatoryMoveFound.score) % 2) / 2);
                }
                if (foundMateValue != 0) {
                    if (mandatoryMoveFound.score < 0) {
                        foundMateValue *= -1;
                    }
                }

                UnityEngine.Debug.Log("mandatory move found in " + Game.Instance.timeToMove + "s " + new Move(mandatoryMoveFound.move) +
                    " score: " + (Math.Abs(mandatoryMoveFound.score) > 950 ? "Mate in " +
                    (Math.Abs(Math.Abs(mandatoryMoveFound.score) - 1000) + Math.Abs(mandatoryMoveFound.score) % 2) / 2 : mandatoryMoveFound.score));
                GameStateManager.Instance.IsEngineRunning = false;
                if (GameStateManager.Instance.globalGameState.whoMoves == 'b') {
                    if (mateValue != 0) {
                        mateValue *= -1;
                    } else {
                        evaluationScore *= -1;
                    }
                }
                float trueDiff;
                if (mateValue != 0 && foundMateValue != 0) { // found the mate, it's fine if it's longer
                    trueDiff = 0;
                } else {
                    if (mateValue != 0) { // didn't find the mate, just assign a large enough advantage to diff with
                        if (mateValue < 0) {
                            evaluationScore = -15;
                        } else {
                            evaluationScore = 15;
                        }
                    } else {
                        evaluationScore /= 100f;
                    }
                    float diff = Math.Abs(foundEvaluationScore - evaluationScore);
                    if (Math.Abs(evaluationScore) <= 1 || Math.Abs(foundEvaluationScore) <= 1) {
                        trueDiff = diff;
                    } else if (evaluationScore * foundEvaluationScore > 0) { // same side, need smaller diff
                        trueDiff = diff * 2 / Math.Abs(evaluationScore + foundEvaluationScore);
                    } else { //opposite sides 
                        trueDiff = diff; // still just the difference
                    }
                }
                sum += trueDiff * trueDiff;
                ++nr;
                if (trueDiff > maxDiff) {
                    maxDiff = trueDiff;
                }
                writer.WriteLine("got " + new Move(mandatoryMoveFound.move) + " " + (foundMateValue != 0 ? "mate in " + foundMateValue : foundEvaluationScore) +
                    ", expected " + bestMove + " " + (mateValue != 0 ? "mate in " + mateValue : evaluationScore) +
                    " Diff is " + trueDiff);
                ++idx;
            }
            writer.WriteLine("Engine deviation: " + Math.Sqrt(sum / nr).ToString("F2"));
            writer.WriteLine("Max Diff " + maxDiff);
            UnityEngine.Debug.Log("Engine deviation: " + Math.Sqrt(sum / nr).ToString("F2"));
        } else {
            UnityEngine.Debug.Log("file not found");
        }
    }

    async void OnRunTestsButtonClicked() {
        string filePath = Path.Combine(Application.streamingAssetsPath, "perft.txt");
        string outPath = Path.Combine(Application.streamingAssetsPath, "outputChess.txt");
        if (File.Exists(filePath)) {
            using StreamReader reader = new(filePath);
            using StreamWriter writer = new(outPath, false);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            string line;
            int idx = 0;
            while ((line = reader.ReadLine()) != null) {
                if (idx % 40 != 0) {
                    ++idx;
                    continue; // only process one portion of the data
                }
                string[] parts = line.Split(',');
                string fen = parts[0];
                int legalMovesDepth1 = int.Parse(parts[1]);
                int legalMovesDepth2 = int.Parse(parts[2]);
                int legalMovesDepth3 = int.Parse(parts[3]);
                bool ok = true;
                GameStateManager.Instance.GenerateGameState(fen);
                maxDepth = 1;
                int result = 0;
                await Task.Run(() => result = SearchPositions(GameStateManager.Instance.globalGameState, 0));
                if (result != legalMovesDepth1) {
                    ok = false;
                    writer.WriteLine("Incorrect results for depth 1: FEN: " + fen + " ; expected " + legalMovesDepth1 + " got " + result);
                }
                maxDepth = 2;
                await Task.Run(() => result = SearchPositions(GameStateManager.Instance.globalGameState, 0));
                if (result != legalMovesDepth2) {
                    ok = false;
                    writer.WriteLine("Incorrect results for depth 2: FEN: " + fen + " ; expected " + legalMovesDepth2 + " got " + result);
                }
                maxDepth = 3;
                await Task.Run(() => result = SearchPositions(GameStateManager.Instance.globalGameState, 0));
                if (result != legalMovesDepth3) {
                    ok = false;
                    writer.WriteLine("Incorrect results for depth 3: FEN: " + fen + " ; expected " + legalMovesDepth3 + " got " + result);
                }
                UnityEngine.Debug.Log(fen + "was checked, it is" + (ok ? "" : " not") + " ok");
                ++idx;
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log("Ran tests in " + stopwatch.ElapsedMilliseconds + "ms");
        } else {
            UnityEngine.Debug.Log("file not found");
        }
    }

    void OnSwapPerspectiveClicked() {
        Game.Instance.SwapPerspectives();
    }

    void OnGetPositionEvalClicked() {
        Game.Instance.GetPositionEval();
    }

    void OnGetStaticPositionEvalClicked() {
        Game.Instance.GetStaticPositionEval();
    }
}
