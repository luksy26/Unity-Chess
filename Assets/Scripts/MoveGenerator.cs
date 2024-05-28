using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static KingSafety;

public static class MoveGenerator {
    public static List<IndexMove> GetLegalMoves(GameState gameState) {
        List<IndexMove> legalMoves = new();
        char[,] boardConfiguration = gameState.boardConfiguration;
        char whoMoves = gameState.whoMoves;
        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                // piece belongs to current player
                if (whoMoves == GetPieceOwner(boardConfiguration[i, j])) {
                    switch (boardConfiguration[i, j]) {
                        case 'p' or 'P': AddLegalPawnMoves(gameState, i, j, legalMoves); break;
                        case 'b' or 'B': AddLegalBishopMoves(gameState, i, j, legalMoves); break;
                        case 'n' or 'N': AddLegalKnightMoves(gameState, i, j, legalMoves); break;
                        case 'r' or 'R': AddLegalRookMoves(gameState, i, j, legalMoves); break;
                        case 'q' or 'Q': AddLegalQueenMoves(gameState, i, j, legalMoves); break;
                        case 'k' or 'Q': AddLegalKingMoves(gameState, i, j, legalMoves); break;
                        default: break;
                    }
                }
            }
        }
        return legalMoves;
    }

    public static void AddLegalPawnMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {
        char[,] boardConfiguration = gameState.boardConfiguration;
        int forwardY, kingRow, kingColumn, startingRow;
        char opponent;
        if (gameState.whoMoves == 'w') {
            forwardY = -1;
            kingRow = gameState.whiteKingRow;
            kingColumn = gameState.whiteKingColumn;
            startingRow = 6;
            opponent = 'b';

        } else {
            forwardY = 1;
            kingRow = gameState.blackKingRow;
            kingColumn = gameState.blackKingColumn;
            startingRow = 1;
            opponent = 'w';
        }
        // check forward pawn movement (square above the pawn must be empty)
        if (boardConfiguration[old_i + forwardY, old_j] == '-') {
            IndexMove pawnOneUp = new(old_i, old_j, old_i + forwardY, old_j);
            if (IsKingSafeAt(kingRow, kingColumn, gameState, pawnOneUp)) {
                legalMoves.Add(pawnOneUp);
            }
            // pawn can move two squares
            if (old_i == startingRow && boardConfiguration[old_i + 2 * forwardY, old_j] == '-') {
                IndexMove pawnTwoUp = new(old_i, old_j, old_i + 2 * forwardY, old_j);
                if (IsKingSafeAt(kingRow, kingColumn, gameState, pawnTwoUp)) {
                    legalMoves.Add(pawnTwoUp);
                }
            }
        }
        // check diagonal pawn movement (captures)

        // capture to the right, check in-bounds first
        if (old_j + 1 < 8) {
            char pieceOwner = GetPieceOwner(boardConfiguration[old_i + forwardY, old_j + 1]);
            // capturing enemy piece (could also be en-passant target, which is always an enemy piece)
            if (pieceOwner == opponent || (pieceOwner == '-' &&
                RowToRank(old_i + forwardY) == gameState.enPassantRank && ColumnToFile(old_j + 1) == gameState.enPassantFile)) {
                IndexMove captureRight = new(old_i, old_j, old_i + forwardY, old_j + 1);
                if (IsKingSafeAt(kingRow, kingColumn, gameState, captureRight)) {
                    legalMoves.Add(captureRight);
                }
            }
        }
        // capture to the left, check in-bounds first
        if (old_j - 1 > 0) {
            char pieceOwner = GetPieceOwner(boardConfiguration[old_i + forwardY, old_j - 1]);
            // capturing enemy piece (could also be en-passant target, which is always an enemy piece)
            if (pieceOwner == opponent || (pieceOwner == '-' &&
                RowToRank(old_i + forwardY) == gameState.enPassantRank && ColumnToFile(old_j - 1) == gameState.enPassantFile)) {
                IndexMove captureLeft = new(old_i, old_j, old_i + forwardY, old_j - 1);
                if (IsKingSafeAt(kingRow, kingColumn, gameState, captureLeft)) {
                    legalMoves.Add(captureLeft);
                }
            }
        }
    }
    public static void AddLegalBishopMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {

    }
    public static void AddLegalKnightMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {

    }
    public static void AddLegalRookMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {

    }

    public static void AddLegalQueenMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {

    }
    public static void AddLegalKingMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {

    }

    private static char GetPieceOwner(char pieceChar) {
        if (!char.IsLetter(pieceChar)) {
            return '-';
        }
        if (char.IsUpper(pieceChar)) {
            return 'w';
        }
        return 'b';
    }
}
