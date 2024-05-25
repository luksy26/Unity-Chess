using UnityEngine;
using System.Text;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    // very important structure, will be used across multiple game components
    public GameState globalGameState;

    // default starting position
    public string defaultFEN;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist this object across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public void Start() {
        defaultFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        globalGameState = new();
    }

    public void GenerateGameState(string inputFEN) {
        if (string.IsNullOrEmpty(inputFEN)) {
            inputFEN = defaultFEN;
        }

        globalGameState.boardConfiguration = new char[8, 8];

        StringBuilder inputFEN_sb = new(inputFEN);
        int index = 0;
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (char.IsDigit(inputFEN_sb[index])) {
                    int emptySpaces = inputFEN_sb[index] - '0';
                    if (emptySpaces > 0) {
                        globalGameState.boardConfiguration[i, j] = '-';
                        inputFEN_sb[index] = (char)('0' + emptySpaces - 1);
                    } else {
                        ++index;
                        --j;
                    }
                } else if (inputFEN_sb[index] != '/') {
                    globalGameState.boardConfiguration[i, j] = inputFEN_sb[index];
                    if (inputFEN_sb[index] == 'k') {
                        globalGameState.blackKingRow = i;
                        globalGameState.blackKingColumn = j;
                    } else if (inputFEN_sb[index] == 'K') {
                        globalGameState.whiteKingRow = i;
                        globalGameState.whiteKingColumn = j;
                    }
                    ++index;
                } else {
                    ++index;
                    --j;
                }
            }
        }
        if (inputFEN_sb[index] != ' ') {
            ++index;
        }
        globalGameState.whoMoves = inputFEN_sb[++index];
        index += 2;

        if (inputFEN_sb[index] == '-') {
            globalGameState.white_O_O = false;
            globalGameState.black_O_O = false;
            globalGameState.white_O_O_O = false;
            globalGameState.black_O_O_O = false;
            ++index;
        } else {
            while (inputFEN_sb[index] != ' ') {
                switch (inputFEN_sb[index]) {
                    case 'K': globalGameState.white_O_O = true; break;
                    case 'Q': globalGameState.white_O_O_O = true; break;
                    case 'k': globalGameState.black_O_O = true; break;
                    case 'q': globalGameState.black_O_O_O = true; break;
                    default: Debug.Log("Unrecognized character in 3rd section of FEN"); break;
                }
                ++index;
            }
        }
        ++index;
        if (inputFEN_sb[index] == '-') {
            globalGameState.enPassantFile = '*';
            globalGameState.enPassantRank = 0;
            ++index;
        } else {
            globalGameState.enPassantFile = inputFEN_sb[index++];
            globalGameState.enPassantRank = inputFEN_sb[index++] - '0';
        }
        ++index;

        string moveCountersString = inputFEN_sb.ToString(index, inputFEN_sb.Length - index);
        string[] moveCounters = moveCountersString.Split(' ');

        globalGameState.moveCounter50Move = int.Parse(moveCounters[0]);
        globalGameState.moveCounterFull = int.Parse(moveCounters[1]);

        Debug.Log("Global GameState Generated:");
        Debug.Log(globalGameState);
    }
}
