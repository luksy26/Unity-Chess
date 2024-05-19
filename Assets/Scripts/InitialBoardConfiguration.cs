using UnityEngine;
using System.Text;

public class InitialBoardConfiguration : MonoBehaviour {
    public static InitialBoardConfiguration Instance { get; private set; }

    // very important structure, will be used across multiple game components
    public char[,] boardConfiguration;
    public int rows = 8;
    public int cols = 8;

    // default starting position
    public string defaultFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public void GenerateboardConfiguration(string inputFEN) {
        if (string.IsNullOrEmpty(inputFEN)) {
            inputFEN = defaultFEN;
        }

        boardConfiguration = new char[rows, cols];

        StringBuilder inputFEN_sb = new(inputFEN);
        int index = 0;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                if (char.IsDigit(inputFEN_sb[index])) {
                    int emptySpaces = inputFEN_sb[index] - '0';
                    if (emptySpaces > 0) {
                        boardConfiguration[i, j] = '-';
                        inputFEN_sb[index] = (char)('0' + emptySpaces - 1);
                    } else {
                        ++index;
                        --j;
                    }
                } else if (inputFEN_sb[index] != '/') {
                    boardConfiguration[i, j] = inputFEN_sb[index];
                    ++index;
                } else {
                    ++index;
                    --j;
                }
            }
        }

        // Example of logging the array for debugging purposes
        Debug.Log("Board Configuration Generated:");
        for (int i = 0; i < rows; i++) {
            string row = "";
            for (int j = 0; j < cols; j++) {
                row += boardConfiguration[i, j];
            }
            Debug.Log(row);
        }
    }
}
