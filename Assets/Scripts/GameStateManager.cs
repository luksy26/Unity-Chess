using UnityEngine;
using System.Text;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    // very important structure, will be used across multiple game components
    public GameState gameState;

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
        gameState = new();
    }

    public void GenerateGameState(string inputFEN) {
        if (string.IsNullOrEmpty(inputFEN)) {
            inputFEN = defaultFEN;
        }

        gameState.boardConfiguration = new char[8, 8];

        StringBuilder inputFEN_sb = new(inputFEN);
        int index = 0;
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (char.IsDigit(inputFEN_sb[index])) {
                    int emptySpaces = inputFEN_sb[index] - '0';
                    if (emptySpaces > 0) {
                        gameState.boardConfiguration[i, j] = '-';
                        inputFEN_sb[index] = (char)('0' + emptySpaces - 1);
                    } else {
                        ++index;
                        --j;
                    }
                } else if (inputFEN_sb[index] != '/') {
                    gameState.boardConfiguration[i, j] = inputFEN_sb[index];
                    ++index;
                } else {
                    ++index;
                    --j;
                }
            }
        }
        gameState.whoMoves = inputFEN_sb[++index];
        index += 2;

        if (inputFEN_sb[index] == '-') {
            gameState.white_O_O = false;
            gameState.black_O_O = false;
            gameState.white_O_O_O = false;
            gameState.black_O_O_O = false;
            ++index;
        } else {
            while (inputFEN_sb[index] != ' ') {
                switch (inputFEN_sb[index]) {
                    case 'K': gameState.white_O_O = true; break;
                    case 'Q': gameState.white_O_O_O = true; break;
                    case 'k': gameState.black_O_O = true; break;
                    case 'q': gameState.black_O_O_O = true; break;
                    default: Debug.Log("Unrecognized character in 3rd section of FEN"); break;
                }
                ++index;
            }
        }
        ++index;
        if (inputFEN_sb[index] == '-') {
            gameState.enPassantFile = '\0';
            gameState.enPassantRank = 0;
            ++index;
        } else {
            gameState.enPassantFile = inputFEN_sb[index++];
            gameState.enPassantRank = inputFEN_sb[index++] - '0';
        }
        ++index;

        string moveCountersString = inputFEN_sb.ToString(index, inputFEN_sb.Length - index);
        string[] moveCounters = moveCountersString.Split(' ');

        gameState.moveCounter50Move = int.Parse(moveCounters[0]);
        gameState.moveCounterFull = int.Parse(moveCounters[1]);

        Debug.Log("GameState Generated:");
        PrintGameState();
    }

    public void MovePiece(int old_i, int old_j, int new_i, int new_j) {
        
        if (gameState.boardConfiguration[new_i, new_j] != '-' || char.ToLower(gameState.boardConfiguration[old_i, old_j]) == 'p') {
            gameState.moveCounter50Move = 0;
        } else {
            ++gameState.moveCounter50Move;
        }
        gameState.boardConfiguration[new_i, new_j] = gameState.boardConfiguration[old_i, old_j];
        gameState.boardConfiguration[old_i, old_j] = '-';

        if (gameState.whoMoves == 'b') {
            ++gameState.moveCounterFull;
            gameState.whoMoves = 'w';
        } else {
            gameState.whoMoves = 'b';
        }
        Debug.Log("GameState changed:");
        PrintGameState();
    }

    public void PrintGameState() {
        for (int i = 0; i < 8; i++) {
            string row = "";
            for (int j = 0; j < 8; j++) {
                row += gameState.boardConfiguration[i, j];
            }
            Debug.Log(row);
        }
        Debug.Log("Current player: " + gameState.whoMoves);
        Debug.Log("White can" + (gameState.white_O_O ? " " : "\'t ") + "short castle");
        Debug.Log("White can" + (gameState.white_O_O_O ? " " : "\'t ") + "long castle");
        Debug.Log("Black can" + (gameState.black_O_O ? " " : "\'t ") + "short castle");
        Debug.Log("Black can" + (gameState.black_O_O ? " " : "\'t ") + "long castle");
        Debug.Log(gameState.enPassantRank == 0 ? "no en-passant available" : (gameState.enPassantFile + gameState.enPassantRank));
        Debug.Log("50 move counter " + gameState.moveCounter50Move + " fullmove counter " + gameState.moveCounterFull);
    }
}
