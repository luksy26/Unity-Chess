using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;
using static PositionCounter;

public class PositionGenerator : MonoBehaviour {
    public InputField inputField;
    public Button generateButton;

    void Start() {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
    }

    void OnGenerateButtonClicked() {
        string inputFEN = inputField.text;

        // string filePath = Path.Combine(Application.streamingAssetsPath, "perft.txt");
        // string outPath = Path.Combine(Application.streamingAssetsPath, "outputChess.txt");
        // if (File.Exists(filePath)) {
        //     using StreamReader reader = new(filePath);
        //     using StreamWriter writer = new(outPath, false);
        //     string line;
        //     while ((line = reader.ReadLine()) != null) {
        //         string[] parts = line.Split(',');
        //         string fen = parts[0];
        //         int legalMovesDepth1 = int.Parse(parts[1]);
        //         int legalMovesDepth2 = int.Parse(parts[2]);
        //         GameStateManager.Instance.GenerateGameState(fen);

        //         bool wip1 = false, wip2 = false;

        //         for (int j = 0; j < 8; ++j) {
        //             if (GameStateManager.Instance.globalGameState.boardConfiguration[1, j] == 'P' ||
        //             GameStateManager.Instance.globalGameState.boardConfiguration[6, j] == 'p') {
        //                 wip1 = true;
        //                 wip2 = true;
        //             }
        //             if (!wip2 && (GameStateManager.Instance.globalGameState.boardConfiguration[2, j] == 'P' ||
        //                 GameStateManager.Instance.globalGameState.boardConfiguration[5, j] == 'p')) {
        //                 wip2 = true;
        //             }
        //         }
        //         if (!wip1) {
        //             maxDepth = 1;
        //             int result = SearchPositions(GameStateManager.Instance.globalGameState, 0);
        //             if (result != legalMovesDepth1) {
        //                 writer.WriteLine("Incorrect results for depth 1: FEN: " + fen + " ; expected " + legalMovesDepth2 + " got " + result);
        //             }
        //         }
        //         if (!wip2) {
        //             maxDepth = 2;
        //             int result = SearchPositions(GameStateManager.Instance.globalGameState, 0);
        //             if (result != legalMovesDepth2) {
        //                 writer.WriteLine("Incorrect results for depth 2: FEN: " + fen + " ; expected " + legalMovesDepth2 + " got " + result);
        //             }
        //         }
        //     }
        // } else {
        //     Debug.Log("file not found");
        // }
        Game.Instance.CancelMovePiece();
        GameStateManager.Instance.GenerateGameState(inputFEN);
        Game.Instance.DestroyPosition();
        Game.Instance.GeneratePosition();
    }
}
