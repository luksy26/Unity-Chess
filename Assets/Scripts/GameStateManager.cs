using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using static MoveGenerator;
using static KingSafety;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    // very important structure, will be used across multiple game components
    public GameState globalGameState;
    public bool IsEngineRunning;

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
        IsEngineRunning = false;
    }

    public void GenerateGameState(string inputFEN) {
        if (string.IsNullOrEmpty(inputFEN)) {
            inputFEN = defaultFEN;
        }

        globalGameState.boardConfiguration = new char[8, 8];
        globalGameState.noBlackPieces = 0;
        globalGameState.noWhitePieces = 0;

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
                    if (char.IsUpper(inputFEN_sb[index])) {
                        ++globalGameState.noWhitePieces;
                    } else {
                        ++globalGameState.noBlackPieces;
                    }
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

        globalGameState.white_O_O = false;
        globalGameState.black_O_O = false;
        globalGameState.white_O_O_O = false;
        globalGameState.black_O_O_O = false;
        if (inputFEN_sb[index] == '-') {
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

        if (index > inputFEN_sb.Length) {
            globalGameState.moveCounter50Move = 0;
            globalGameState.moveCounterFull = 1;
        } else {
            string moveCountersString = inputFEN_sb.ToString(index, inputFEN_sb.Length - index);
            string[] moveCounters = moveCountersString.Split(' ');

            globalGameState.moveCounter50Move = int.Parse(moveCounters[0]);
            globalGameState.moveCounterFull = int.Parse(moveCounters[1]);
        }
        // Debug.Log("Global GameState Generated:");
        // Debug.Log(globalGameState);
    }

    public GameConclusion GetGameConclusion(GameState gameState, Hashtable gameStates) {
        if (gameState.moveCounter50Move == 100) {
            return GameConclusion.DrawBy50MoveRule;
        }
        if (gameStates != null) {
            if ((int)gameStates[gameState] >= 3) {
                return GameConclusion.DrawByRepetition;
            }
        }
        if (gameState.noBlackPieces == 1 && gameState.noWhitePieces == 1) {
            return GameConclusion.DrawByInsufficientMaterial;
        }
        // black only has a lone king
        if (gameState.noBlackPieces == 1) {
            // white has a king and one other piece
            if (gameState.noWhitePieces == 2) {
                string board = new(gameState.boardConfiguration.Cast<char>().ToArray());
                int whiteKnightFound = board.IndexOf('N');
                int whiteBishopFound = board.IndexOf('B');
                // king vs king and knight
                if (whiteKnightFound >= 0) {
                    return GameConclusion.DrawByInsufficientMaterial;
                }
                // king vs king and bishop
                if (whiteBishopFound >= 0) {
                    return GameConclusion.DrawByInsufficientMaterial;
                }
            }
        }
        // white only has a lone king
        if (gameState.noWhitePieces == 1) {
            // black has a king and one other piece
            if (gameState.noBlackPieces == 2) {
                string board = new(gameState.boardConfiguration.Cast<char>().ToArray());
                int blackKnightFound = board.IndexOf('n');
                int blackBishopFound = board.IndexOf('b');
                // king vs king and knight
                if (blackKnightFound >= 0) {
                    return GameConclusion.DrawByInsufficientMaterial;
                }
                // king vs king and bishop
                if (blackBishopFound >= 0) {
                    return GameConclusion.DrawByInsufficientMaterial;
                }
            }
        }
        if (gameState.noWhitePieces == 2 && gameState.noBlackPieces == 2) {
            string board = new(gameState.boardConfiguration.Cast<char>().ToArray());
            int blackBishopFound = board.IndexOf('b');
            int whiteBishopFound = board.IndexOf('B');
            // king and bishop vs king and bishop
            if (blackBishopFound >= 0 && whiteBishopFound >= 0) {
                int blackRow = blackBishopFound / 8;
                int blackColumn = blackBishopFound % 8;
                int whiteRow = whiteBishopFound / 8;
                int whiteColumn = whiteBishopFound % 8;
                // check if the bishops are the same color
                if (blackRow + blackColumn % 2 == whiteRow + whiteColumn % 2) {
                    return GameConclusion.DrawByInsufficientMaterial;
                }
            }
        }
        List<IndexMove> moves = GetLegalMoves(gameState);

        // Debug.Log("found " + moves.Count + " legal moves:");
        // foreach(IndexMove move in moves) {
        //     Debug.Log(new Move(move));
        // }

        if (moves.Count == 0) {
            int kingRow = gameState.whoMoves == 'w' ? gameState.whiteKingRow : gameState.blackKingRow;
            int kingColumn = gameState.whoMoves == 'w' ? gameState.whiteKingColumn : gameState.blackKingColumn;
            if (IsKingSafeAt(kingRow, kingColumn, gameState, null)) {
                return GameConclusion.Stalemate;
            }
            return GameConclusion.Checkmate;
        }
        return GameConclusion.NotOver;
    }
}
