using System.Collections.Generic;
using static KingSafety;
using UnityEngine;

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
                        //case 'p' or 'P': AddLegalPawnMoves(gameState, i, j, legalMoves); break;
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
        char[,] boardConfiguration = gameState.boardConfiguration;
        int kingRow, kingColumn;
        char opponent;
        if (gameState.whoMoves == 'w') {
            kingRow = gameState.whiteKingRow;
            kingColumn = gameState.whiteKingColumn;
            opponent = 'b';

        } else {
            kingRow = gameState.blackKingRow;
            kingColumn = gameState.blackKingColumn;
            opponent = 'w';
        }

        /*
            We check the main and secondary diagonal of the bishop's movement
            Each diagonal spans 2 quadrants:
                - Main diagonal: upper right (-1, 1) and lower left (1, -1)
                - Secondary diagonal: upper left (-1, -1) and lower right (1, 1)
            The following arrays contain row and column increments for each diagonal's quadrants
        */
        int[,] diagonalXIncrements = new int[,] { { 1, -1 }, { -1, 1 } };
        int[,] diagonalYIncrements = new int[,] { { -1, 1 }, { -1, 1 } };
        int noSafeSquaresInit = 0;
        int noUnsafeSquaresInit = 0;
        // bishop's current square is part of both diagonals
        if (IsKingSafeAt(kingRow, kingColumn, gameState, null)) {
            ++noSafeSquaresInit;
        } else {
            ++noUnsafeSquaresInit;
        }

        for (int diagonal = 0; diagonal < 2; ++diagonal) {
            int noSafeSquares = noSafeSquaresInit;
            int noUnsafeSquares = noUnsafeSquaresInit;
            for (int quadrant = 0; quadrant < 2; ++quadrant) {
                for (int new_i = old_i + diagonalYIncrements[diagonal, quadrant], new_j = old_j + diagonalXIncrements[diagonal, quadrant];
                    new_i >= 0 && new_i <= 7 && new_j >= 0 && new_j <= 7;
                    new_i += diagonalYIncrements[diagonal, quadrant], new_j += diagonalXIncrements[diagonal, quadrant]) {
                    
                    //Debug.Log("diagonal " + diagonal + " quadrant " + quadrant + " safe: " + noSafeSquares + " unsafe: " + noUnsafeSquares);
                    char pieceOwner = GetPieceOwner(boardConfiguration[new_i, new_j]);

                    // bishop is blocked by own piece
                    if (pieceOwner == gameState.whoMoves) {
                        break;
                    }
                    IndexMove bishopMove = new(old_i, old_j, new_i, new_j);

                    // capturing an enemy piece
                    if (pieceOwner == opponent) {
                        // do final checks and stop search in this quadrant
                        // if 2 safe squares were already found, it means we are on a safe diagonal
                        if (noSafeSquares > 1 || IsKingSafeAt(kingRow, kingColumn, gameState, bishopMove)) {
                            legalMoves.Add(bishopMove);
                            ++noSafeSquares;
                            if (noUnsafeSquares > 0) { // unsafe diagonal, so this was the only safe square
                                ++quadrant; // no point in searching more safe squares on this diagonal
                            }
                        } else {
                            ++noUnsafeSquares;
                            if (noSafeSquares > 0) { // the only safe square on this unsafe diagonal was already found
                                ++quadrant; // no point in searching more safe squares on this diagonal
                            }
                        }
                        break; // rest of quadrant is blocked
                    }
                    // if we get here it means the bishop is moving to an empty square
                    // if 2 safe squares were already found, it means we are on a safe diagonal
                    if (noSafeSquares > 1 || IsKingSafeAt(kingRow, kingColumn, gameState, bishopMove)) {
                        legalMoves.Add(bishopMove);
                        ++noSafeSquares;
                        if (noUnsafeSquares > 0) { // unsafe diagonal, so this was the only safe square
                            ++quadrant; // no point in searching more safe squares on this diagonal
                            break; // of course, not in the quadrant either, since it's part of the diagonal
                        }
                    } else {
                        ++noUnsafeSquares;
                        if (noSafeSquares > 0) { // the only safe square on this unsafe diagonal was already found
                            ++quadrant; // no point in searching more safe squares on this diagonal
                            break; // of course, not on the quadrant either, since it's part of the diagonal
                        }
                    }
                }
            }
        }
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
