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
            "rook" => true,
            "knight" => true,
            "bishop" => true,
            "queen" => true,
            "king" => true,
            "pawn" => IsLegalPawnMove(old_i, old_j, new_i, new_j, gameState),
            _ => false,
        };
    }

    public bool IsLegalPawnMove(int old_i, int old_j, int new_i, int new_j, GameState gameState) {
        bool forwardOrDiagonalPawn = (Math.Abs(new_i - old_i) <= 2) && (Math.Abs(old_j - new_j) <= 1);
        // pawn moving more than 2 ranks or more than one file
        if (!forwardOrDiagonalPawn) {
            Debug.Log("returning false 1");
            return false;
        }

        // pawn moving 1 file and 2 or 0 ranks
        bool capturing = old_j != new_j;
        bool moving2Squares = Math.Abs(old_i - new_i) == 2;
        if ((moving2Squares || old_i == new_i) && capturing) {
            Debug.Log("returning false 2");
            return false;
        }

        char whoMoves = gameState.whoMoves;
        bool blackMove = new_i > old_i;

        // white is trying to move their pawn backwards
        if (blackMove && whoMoves == 'w') {
            Debug.Log("returning false 3");
            return false;
        }

        bool whiteMove = !blackMove;
        // black is trying to move their pawn backwards
        if (whiteMove && whoMoves == 'b') {
            Debug.Log("returning false 4");
            return false;
        }

        bool emptyTarget = gameState.boardConfiguration[new_i, new_j] == '-';
        // trying to move forward on an occupied square
        if (!emptyTarget && !capturing) {
            Debug.Log("returning false 5");
            return false;
        }

        if (moving2Squares) {
            int old_rank = RowIndexToRank(old_i);
            // trying to move 2 ranks, but the pawn is not on its starting position or the pawn is jumping over a piece
            if (whoMoves == 'w' && (old_rank != 2 || gameState.boardConfiguration[5, old_j] != '-')) {
                Debug.Log("returning false 6");
                return false;
            }
            if (whoMoves == 'b' && (old_rank != 7 || gameState.boardConfiguration[2, old_j] != '-')) {
                Debug.Log("returning false 7");
                return false;
            }
        }

        // if capturing an empty, non en-passant square
        if (capturing && gameState.boardConfiguration[new_i, new_j] == '-' && 
                (RowIndexToRank(new_i) != gameState.enPassantRank || ColumnIndexToFile(new_j) != gameState.enPassantFile)) {
            Debug.Log("returning false 8");
            return false;
        }
        // we have covered all possible illegal moves (not considering discovered check)
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
