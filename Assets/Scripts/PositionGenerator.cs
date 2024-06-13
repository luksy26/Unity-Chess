using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static PositionCounter;

public class PositionGenerator : MonoBehaviour {
    public InputField inputField;
    public Button generateButton, runTests, swapPerspective, getPositionEval, getStaticPositionEval;

    void Start() {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
        runTests.onClick.AddListener(OnRunTestsButtonClicked);
        swapPerspective.onClick.AddListener(OnSwapPerspectiveClicked);
        getPositionEval.onClick.AddListener(OnGetPositionEvalClicked);
        getStaticPositionEval.onClick.AddListener(OnGetStaticPositionEvalClicked);
    }

    void OnGenerateButtonClicked() {
        string inputFEN = inputField.text;
        if (!GameStateManager.Instance.IsEngineRunning) {
            Game.Instance.AIPlayer = '-';
            Game.Instance.playerPerspective = "white";
            Game.Instance.movesAhead = 2;
            Game.Instance.timeToMove = 5f;
            Game.Instance.timeNotExpired = true;
            Game.Instance.CancelMovePiece();
            GameStateManager.Instance.GenerateGameState(inputFEN);
            Game.Instance.DestroyPosition();
            Game.Instance.GeneratePosition();
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
