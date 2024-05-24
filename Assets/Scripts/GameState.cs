

using System;

public class GameState {
    public char[,] boardConfiguration;
    public char whoMoves;
    public int moveCounter50Move;
    public int moveCounterFull;
    public char enPassantFile;
    public int enPassantRank;
    public int blackKingRow, blackKingColumn, whiteKingRow, whiteKingColumn;
    public bool white_O_O, white_O_O_O, black_O_O, black_O_O_O;

    public override int GetHashCode() {
        int hash = whoMoves.GetHashCode();
        hash = (hash * 37) ^ enPassantFile.GetHashCode();
        hash = (hash * 37) ^ enPassantRank;
        hash = (hash * 37) ^ white_O_O.GetHashCode();
        hash = (hash * 37) ^ white_O_O_O.GetHashCode();
        hash = (hash * 37) ^ black_O_O.GetHashCode();
        hash = (hash * 37) ^ black_O_O_O.GetHashCode();

        // Efficiently hash the board configuration
        for (int i = 0; i < boardConfiguration.GetLength(0); i++) {
            for (int j = 0; j < boardConfiguration.GetLength(1); j++) {
                hash = (hash * 37) ^ boardConfiguration[i, j].GetHashCode();
            }
        }
        return hash;
    }

    public override bool Equals(object obj) {
        if (obj is GameState other) {
            // Compare all fields for equality
            return whoMoves == other.whoMoves &&
                   enPassantFile == other.enPassantFile &&
                   enPassantRank == other.enPassantRank &&
                   white_O_O == other.white_O_O &&
                   white_O_O_O == other.white_O_O_O &&
                   black_O_O == other.black_O_O &&
                   black_O_O_O == other.black_O_O_O &&
                   AreBoardsEqual(boardConfiguration, other.boardConfiguration);
        }
        return false;
    }

    public static bool AreBoardsEqual(char[,] board1, char[,] board2) {
        // Check if the dimensions are the same
        if (board1.GetLength(0) != board2.GetLength(0) || board1.GetLength(1) != board2.GetLength(1)) {
            return false;
        }

        // Compare each element
        for (int i = 0; i < board1.GetLength(0); i++) {
            for (int j = 0; j < board1.GetLength(1); j++) {
                if (board1[i, j] != board2[i, j]) {
                    return false;
                }
            }
        }

        // If all elements are the same, the boards are equal
        return true;
    }
}
