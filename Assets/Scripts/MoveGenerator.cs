using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static KingSafety;

public static class MoveGenerator {
    public static List<IndexMove> GetLegalMoves(GameState gameState) {
        List<IndexMove> legalMoves = new();
        char[,] boardConfiguration = gameState.boardConfiguration;
        // Hashtable piecesHashtable = gameState.whoMoves == 'w' ? gameState.whitePiecesPositions : gameState.blackPiecesPositions;
        // foreach (int key in piecesHashtable.Keys) {
        //     int i = key / 8, j = key % 8;
        //     switch (boardConfiguration[i, j]) {
        //         case 'p' or 'P': AddLegalPawnMoves(gameState, i, j, legalMoves); break;
        //         case 'b' or 'B': AddLegalBishopMoves(gameState, i, j, legalMoves); break;
        //         case 'n' or 'N': AddLegalKnightMoves(gameState, i, j, legalMoves); break;
        //         case 'r' or 'R': AddLegalRookMoves(gameState, i, j, legalMoves); break;
        //         case 'q' or 'Q': AddLegalQueenMoves(gameState, i, j, legalMoves); break;
        //         case 'k' or 'K': AddLegalKingMoves(gameState, i, j, legalMoves); break;
        //         default: break;
        //     }
        // }

        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                if (GetPieceOwner(boardConfiguration[i, j]) == gameState.whoMoves) {
                    switch (boardConfiguration[i, j]) {
                        case 'p' or 'P': AddLegalPawnMoves(gameState, i, j, legalMoves); break;
                        case 'b' or 'B': AddLegalBishopMoves(gameState, i, j, legalMoves); break;
                        case 'n' or 'N': AddLegalKnightMoves(gameState, i, j, legalMoves); break;
                        case 'r' or 'R': AddLegalRookMoves(gameState, i, j, legalMoves); break;
                        case 'q' or 'Q': AddLegalQueenMoves(gameState, i, j, legalMoves); break;
                        case 'k' or 'K': AddLegalKingMoves(gameState, i, j, legalMoves); break;
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
        char opponent, whoMoves = gameState.whoMoves;
        bool canPromote = (gameState.whoMoves == 'w' && old_i == 1) || (gameState.whoMoves == 'b' && old_i == 6);
        if (whoMoves == 'w') {
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
                if (canPromote) {
                    AddAllPromotionTypes(pawnOneUp, whoMoves, legalMoves);
                } else {
                    legalMoves.Add(pawnOneUp);
                }
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
        if (old_j + 1 <= 7) {
            char pieceOwner = GetPieceOwner(boardConfiguration[old_i + forwardY, old_j + 1]);
            // capturing enemy piece (could also be en-passant target, which is always an enemy piece)
            if (pieceOwner == opponent || (pieceOwner == '-' &&
                RowToRank(old_i + forwardY) == gameState.enPassantRank && ColumnToFile(old_j + 1) == gameState.enPassantFile)) {
                IndexMove captureRight = new(old_i, old_j, old_i + forwardY, old_j + 1);
                if (IsKingSafeAt(kingRow, kingColumn, gameState, captureRight)) {
                    if (canPromote) {
                        AddAllPromotionTypes(captureRight, whoMoves, legalMoves);
                    } else {
                        legalMoves.Add(captureRight);
                    }
                }
            }
        }
        // capture to the left, check in-bounds first
        if (old_j - 1 >= 0) {
            char pieceOwner = GetPieceOwner(boardConfiguration[old_i + forwardY, old_j - 1]);
            // capturing enemy piece (could also be en-passant target, which is always an enemy piece)
            if (pieceOwner == opponent || (pieceOwner == '-' &&
                RowToRank(old_i + forwardY) == gameState.enPassantRank && ColumnToFile(old_j - 1) == gameState.enPassantFile)) {
                IndexMove captureLeft = new(old_i, old_j, old_i + forwardY, old_j - 1);
                if (IsKingSafeAt(kingRow, kingColumn, gameState, captureLeft)) {
                    if (canPromote) {
                        AddAllPromotionTypes(captureLeft, whoMoves, legalMoves);
                    } else {
                        legalMoves.Add(captureLeft);
                    }
                }
            }
        }
    }

    public static void AddAllPromotionTypes(IndexMove indexMove, char whoMoves, List<IndexMove> legalMoves) {
        IndexMove promoteQueen, promoteKnight, promoteBishop, promoteRook;

        promoteQueen = new IndexMove(indexMove) {
            promotesInto = whoMoves == 'w' ? 'Q' : 'q'
        };
        promoteKnight = new IndexMove(indexMove) {
            promotesInto = whoMoves == 'w' ? 'N' : 'n'
        };
        promoteBishop = new IndexMove(indexMove) {
            promotesInto = whoMoves == 'w' ? 'B' : 'b'
        };
        promoteRook = new IndexMove(indexMove) {
            promotesInto = whoMoves == 'w' ? 'R' : 'r'
        };

        legalMoves.Add(promoteQueen);
        legalMoves.Add(promoteKnight);
        legalMoves.Add(promoteBishop);
        legalMoves.Add(promoteRook);
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
        if (IsKingSafeAt(kingRow, kingColumn, gameState)) {
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
        char[,] boardConfiguration = gameState.boardConfiguration;
        int kingRow, kingColumn;
        if (gameState.whoMoves == 'w') {
            kingRow = gameState.whiteKingRow;
            kingColumn = gameState.whiteKingColumn;

        } else {
            kingRow = gameState.blackKingRow;
            kingColumn = gameState.blackKingColumn;
        }
        int[] LshapeXIncrements = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] LshapeYIncrements = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };

        bool initialPositionSafe;

        if (IsKingSafeAt(kingRow, kingColumn, gameState)) {
            initialPositionSafe = true;
        } else {
            initialPositionSafe = false;
        }

        int noSafeSquares = 0;
        int noUnsafeSquares = 0;

        for (int idx = 0; idx < 8; ++idx) {
            int new_i = old_i + LshapeYIncrements[idx];
            int new_j = old_j + LshapeXIncrements[idx];
            // in bounds
            if (new_i >= 0 && new_i <= 7 && new_j >= 0 && new_j <= 7) {
                char pieceOwner = GetPieceOwner(boardConfiguration[new_i, new_j]);
                // not trying to capture own piece
                if (pieceOwner != gameState.whoMoves) {
                    IndexMove knightMove = new(old_i, old_j, new_i, new_j);
                    // if 3 safe squares were already found or if the knight is not pinned, it can move freely
                    if ((noSafeSquares > 2) || (noSafeSquares > 0 && initialPositionSafe) || IsKingSafeAt(kingRow, kingColumn, gameState, knightMove)) {
                        legalMoves.Add(knightMove);
                        ++noSafeSquares;
                        // knight may only block a line or diagonal check with 2 movements at most (knight check with only one, by capturing)
                        if (noUnsafeSquares > 0 && noSafeSquares > 1) {
                            break; // no point in searching for more safe squares
                        }
                    } else {
                        ++noUnsafeSquares;
                        // knight is pinned or we already found the 2 safe squares that block the check
                        if (initialPositionSafe || noSafeSquares > 1) {
                            break; // no point in searching for more safe squares
                        }
                    }
                }
            }
        }
    }
    public static void AddLegalRookMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {
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
            We check the vertical and horizontal line of the rook's movement
            Each line has two directions:
                - Horizontal line: right (1, 0) and left (-1, 0)
                - Vertical line: up (0, -1) and down (0, 1)
            The following arrays contain row and column increments for each line's directions
        */
        int[,] lineXIncrements = new int[,] { { 1, -1 }, { 0, 0 } };
        int[,] lineYIncrements = new int[,] { { 0, 0 }, { -1, 1 } };
        int noSafeSquaresInit = 0;
        int noUnsafeSquaresInit = 0;
        // rook's current square is part of both lines
        if (IsKingSafeAt(kingRow, kingColumn, gameState)) {
            ++noSafeSquaresInit;
        } else {
            ++noUnsafeSquaresInit;
        }

        for (int line = 0; line < 2; ++line) {
            int noSafeSquares = noSafeSquaresInit;
            int noUnsafeSquares = noUnsafeSquaresInit;
            for (int direction = 0; direction < 2; ++direction) {
                for (int new_i = old_i + lineYIncrements[line, direction], new_j = old_j + lineXIncrements[line, direction];
                    new_i >= 0 && new_i <= 7 && new_j >= 0 && new_j <= 7;
                    new_i += lineYIncrements[line, direction], new_j += lineXIncrements[line, direction]) {

                    char pieceOwner = GetPieceOwner(boardConfiguration[new_i, new_j]);

                    // rook is blocked by own piece
                    if (pieceOwner == gameState.whoMoves) {
                        break;
                    }
                    IndexMove rookMove = new(old_i, old_j, new_i, new_j);

                    // capturing an enemy piece
                    if (pieceOwner == opponent) {
                        // do final checks and stop search in this direction
                        // if 2 safe squares were already found, it means we are on a safe line
                        if (noSafeSquares > 1 || IsKingSafeAt(kingRow, kingColumn, gameState, rookMove)) {
                            legalMoves.Add(rookMove);
                            ++noSafeSquares;
                            if (noUnsafeSquares > 0) { // unsafe line, so this was the only safe square
                                ++direction; // no point in searching more safe squares on this line
                            }
                        } else {
                            ++noUnsafeSquares;
                            if (noSafeSquares > 0) { // the only safe square on this unsafe line was already found
                                ++direction; // no point in searching more safe squares on this line
                            }
                        }
                        break; // this direction is blocked
                    }
                    // if we get here it means the rook is moving to an empty square
                    // if 2 safe squares were already found, it means we are on a safe line
                    if (noSafeSquares > 1 || IsKingSafeAt(kingRow, kingColumn, gameState, rookMove)) {
                        legalMoves.Add(rookMove);
                        ++noSafeSquares;
                        if (noUnsafeSquares > 0) { // unsafe line, so this was the only safe square
                            ++direction; // no point in searching more safe squares on this line
                            break; // of course, not in either direction either, since they're part of the line
                        }
                    } else {
                        ++noUnsafeSquares;
                        if (noSafeSquares > 0) { // the only safe square on this unsafe line was already found
                            ++direction; // no point in searching more safe squares on this line
                            break; // of course, not in either direction either, since they're part of the line
                        }
                    }
                }
            }
        }
    }

    public static void AddLegalQueenMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {
        // queen can move either as a bishop or as a rook
        AddLegalBishopMoves(gameState, old_i, old_j, legalMoves);
        AddLegalRookMoves(gameState, old_i, old_j, legalMoves);
    }
    public static void AddLegalKingMoves(GameState gameState, int old_i, int old_j, List<IndexMove> legalMoves) {
        char[,] boardConfiguration = gameState.boardConfiguration;

        // The following arrays contain row and column increments for one-square king-movement
        int[] smallSquareXIncrements = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] smallSquareYIncrements = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int idx = 0; idx < 8; ++idx) {
            int new_i = old_i + smallSquareYIncrements[idx];
            int new_j = old_j + smallSquareXIncrements[idx];
            // in bounds
            if (new_i >= 0 && new_i <= 7 && new_j >= 0 && new_j <= 7) {
                char pieceOwner = GetPieceOwner(boardConfiguration[new_i, new_j]);
                // not trying to capture own piece
                if (pieceOwner != gameState.whoMoves) {
                    // king not in check
                    if (IsKingSafeAt(new_i, new_j, gameState)) {
                        legalMoves.Add(new IndexMove(old_i, old_j, new_i, new_j));
                    }
                }
            }
        }
        // if king is in check we cannot castle
        if (!IsKingSafeAt(old_i, old_j, gameState)) {
            return;
        }
        // try to short castle
        if ((gameState.whoMoves == 'w' && gameState.canWhite_O_O) || (gameState.whoMoves == 'b' && gameState.canBlack_O_O)) {
            // it's implied the king is on its starting square
            // no blocking pieces
            if (boardConfiguration[old_i, old_j + 1] == '-' && boardConfiguration[old_i, old_j + 2] == '-') {
                // not castling through or into check
                if (IsKingSafeAt(old_i, old_j + 1, gameState) && IsKingSafeAt(old_i, old_j + 2, gameState)) {
                    legalMoves.Add(new IndexMove(old_i, old_j, old_i, old_j + 2));
                }
            }
        }
        // try to long castle
        if ((gameState.whoMoves == 'w' && gameState.canWhite_O_O_O) || (gameState.whoMoves == 'b' && gameState.canBlack_O_O_O)) {
            // it's implied the king is on its starting square
            // no blocking pieces
            if (boardConfiguration[old_i, old_j - 1] == '-' && boardConfiguration[old_i, old_j - 2] == '-' && boardConfiguration[old_i, old_j - 3] == '-') {
                // not castling through or into check
                if (IsKingSafeAt(old_i, old_j - 1, gameState) && IsKingSafeAt(old_i, old_j - 2, gameState)) {
                    legalMoves.Add(new IndexMove(old_i, old_j, old_i, old_j - 2));
                }
            }
        }
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
