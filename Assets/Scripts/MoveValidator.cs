using System;
using UnityEngine;

public class MoveValidator : MonoBehaviour {

    public bool IsLegalMove(char old_file, int old_rank, char new_file, int new_rank, GameState gameState) {
        // trying to move in place
        if (old_file == new_file && old_rank == new_rank) {
            return false;
        }
        char[,] boardConfiguration = gameState.boardConfiguration;
        int old_i = 8 - old_rank;
        int old_j = old_file - 'a';
        int new_i = 8 - new_rank;
        int new_j = new_file - 'a';

        char pieceChar = boardConfiguration[old_i, old_j];
        char targetSquareChar = boardConfiguration[new_i, new_j];

        if (GetPieceType(targetSquareChar) != "") {
            // trying to capture own piece
            if (char.IsUpper(pieceChar) == char.IsUpper(targetSquareChar)) {
                return false;
            }
        }
        return GetPieceType(pieceChar) switch {
            "rook" => IsLegalRookMove(old_i, old_j, new_i, new_j, gameState),
            "knight" => IsLegalKnightMove(old_i, old_j, new_i, new_j, gameState),
            "bishop" => IsLegalBishopMove(old_i, old_j, new_i, new_j, gameState),
            "queen" => IsLegalQueenMove(old_i, old_j, new_i, new_j, gameState),
            "king" => IsLegalKingMove(old_i, old_j, new_i, new_j, gameState),
            "pawn" => IsLegalPawnMove(old_i, old_j, new_i, new_j, gameState),
            _ => false,
        };
    }

    public bool IsLegalPawnMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        bool forwardOrDiagonalPawn = (Math.Abs(new_i - old_i) <= 2) && (Math.Abs(old_j - new_j) <= 1);
        // pawn moving more than 2 ranks or more than one file
        if (!forwardOrDiagonalPawn) {
            return false;
        }

        // pawn moving 1 file and 2 or 0 ranks
        bool capturing = old_j != new_j;
        bool moving2Squares = Math.Abs(old_i - new_i) == 2;
        if ((moving2Squares || old_i == new_i) && capturing) {
            return false;
        }

        char whoMoves = gameState.whoMoves;
        bool blackMove = new_i > old_i;

        // white is trying to move their pawn backwards
        if (blackMove && whoMoves == 'w') {
            return false;
        }

        bool whiteMove = !blackMove;
        // black is trying to move their pawn backwards
        if (whiteMove && whoMoves == 'b') {
            return false;
        }

        bool emptyTarget = gameState.boardConfiguration[new_i, new_j] == '-';
        // trying to move forward on an occupied square
        if (!emptyTarget && !capturing) {
            return false;
        }

        if (moving2Squares) {
            int old_rank = RowIndexToRank(old_i);
            // trying to move 2 ranks, but the pawn is not on its starting position or the pawn is jumping over a piece
            if (whoMoves == 'w' && (old_rank != 2 || gameState.boardConfiguration[5, old_j] != '-')) {
                return false;
            }
            if (whoMoves == 'b' && (old_rank != 7 || gameState.boardConfiguration[2, old_j] != '-')) {
                return false;
            }
        }

        // if capturing an empty, non en-passant square
        if (capturing && gameState.boardConfiguration[new_i, new_j] == '-' && 
                (RowIndexToRank(new_i) != gameState.enPassantRank || ColumnIndexToFile(new_j) != gameState.enPassantFile)) {
            return false;
        }
        // we have covered all possible illegal pawn moves (not considering pinned pieces)
        return true;
    }
    public bool IsLegalRookMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        bool movingSameFile = old_j == new_j;
        bool movingSameRank = old_i == new_i;

        // not moving in a line (both true: stationary piece; both false: weird diagonal)
        if (movingSameRank == movingSameFile) {
            Debug.Log("false 1");
            return false;
        }

        if (movingSameRank) { // moving horizontally
            int increment = (new_j > old_j) ? 1: -1; // choose direction
            for (int j = old_j + increment; j != new_j; j += increment) {
                // found a piece in the way
                if (gameState.boardConfiguration[old_i, j] != '-') {
                    Debug.Log("false 2");
                    return false;
                }
            }
        } else { // moving vertically
            int increment = (new_i > old_i) ? 1: -1; // choose direction
            for (int i = old_i + increment; i != new_i; i += increment) {
                // found a piece in the way
                if (gameState.boardConfiguration[i, old_j] != '-') {
                    Debug.Log("false 3");
                    return false;
                }
            }
        }
        // we have covered all possible illegal rook moves (not considering pinned piecees)
        return true;
    }

    public bool IsLegalBishopMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        int rankDiff = new_i - old_i;
        int fileDiff = new_j - old_j;

        // not moving diagonally
        if (Math.Abs(rankDiff) != Math.Abs(fileDiff)) {
            return false;
        }
        // choosing diagonal direction
        int rankIncrement = rankDiff > 0 ? 1 : -1;
        int fileIncrement = fileDiff > 0 ? 1 : -1;
        for (int i = old_i + rankIncrement, j = old_j + fileIncrement; i != new_i; i += rankIncrement, j += fileIncrement) {
            // found a piece in the way
            if (gameState.boardConfiguration[i, j] != '-') {
                return false;
            }
        }
        // we have covered all possible illegal bishop moves (not considering pinned pieces)
        return true;
    }

    public bool IsLegalQueenMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        // queen can move either as a rook or as a bishop
        return IsLegalBishopMove(old_i, old_j, new_i, new_j, gameState) || IsLegalRookMove(old_i, old_j, new_i, new_j, gameState);
    }

    public bool IsLegalKnightMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        int rankDiff = new_i - old_i;
        int fileDiff = new_j - old_j;
        // moving too far or moving like a rook
        if (Math.Abs(rankDiff) > 2 || Math.Abs(fileDiff) > 2 || rankDiff == 0 || fileDiff == 0) {
            return false;
        }
        // we don't have an L shape movement
        if (Math.Abs(rankDiff) == Math.Abs(fileDiff)) {
            return false;
        }
        // only valid knight movement left (2 and 1 for the values)
        return true;
    }

    public bool IsLegalKingMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        // moving too far
        if (Math.Abs(new_i - old_i) > 1 || Math.Abs(new_j - old_j) > 1) {
            return false;
        }
        // we have covered all possible illegal king moves (not considering moving into check)
        return true;
    }

    private string GetPieceType(char x) {
        return x switch {
            'r' or 'R' => "rook",
            'n' or 'N' => "knight",
            'b' or 'B' => "bishop",
            'q' or 'Q' => "queen",
            'k' or 'K' => "king",
            'p' or 'P' => "pawn",
            _ => "",
        };
    }
    private char ColumnIndexToFile(int j) {
        return (char)(j + 'a');
    }
    private int RowIndexToRank(int i) {
        return 8 - i;
    }
}
