using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

public static class KingSafety {

    public static List<int> GetKingAttackers(GameState gameState, IndexMove indexMove = null) {
        int king_i, king_j;

        if (gameState.whoMoves == 'w') {
            king_i = gameState.whiteKingRow;
            king_j = gameState.whiteKingColumn;
        } else {
            king_i = gameState.blackKingRow;
            king_j = gameState.blackKingColumn;
        }

        List<int> attackers = new();

        char[,] boardConfiguration = gameState.boardConfiguration;

        bool restoreBoard = false;
        bool targetingEnPassant = false;
        char oldSquare = '-', newSquare = '-', enPassantSquare = '-';

        // a piece is moving so we need to check king safety on a new configuration
        if (indexMove != null) {
            int old_i, new_i, old_j, new_j;

            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;
            new_i = indexMove.newRow;
            new_j = indexMove.newColumn;

            restoreBoard = true;
            oldSquare = boardConfiguration[old_i, old_j];
            newSquare = boardConfiguration[new_i, new_j];
            targetingEnPassant = gameState.enPassantFile == ColumnToFile(new_j) && gameState.enPassantRank == RowToRank(new_i);
            if (char.ToLower(oldSquare) == 'p' && targetingEnPassant) {
                if (gameState.whoMoves == 'w') {
                    enPassantSquare = boardConfiguration[new_i + 1, new_j];
                    boardConfiguration[new_i + 1, new_j] = '-';
                } else {
                    enPassantSquare = boardConfiguration[new_i - 1, new_j];
                    boardConfiguration[new_i - 1, new_j] = '-';
                }
            }
            boardConfiguration[old_i, old_j] = '-';
            boardConfiguration[new_i, new_j] = oldSquare;
        }

        // check for attacking knights

        int[] knightXCheckList = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] knightYCheckList = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };
        char enemyKnight = gameState.whoMoves == 'w' ? 'n' : 'N';

        for (int idx = 0; idx < 8; ++idx) {
            int knight_i = king_i + knightYCheckList[idx];
            if (knight_i >= 0 && knight_i <= 7) {
                int knight_j = king_j + knightXCheckList[idx];
                if (knight_j >= 0 && knight_j <= 7) {
                    // if we are here then the position is in bounds
                    if (boardConfiguration[knight_i, knight_j] == enemyKnight) {
                        attackers.Add(knight_i * 8 + knight_j);
                        if (attackers.Count > 1) {
                            break;
                        }
                    }
                }
            }
        }

        if (attackers.Count < 2) {
            // check for attacking pawns
            char enemyPawn = gameState.whoMoves == 'w' ? 'p' : 'P';
            int pawn_i = gameState.whoMoves == 'w' ? king_i - 1 : king_i + 1; // the rank where pawns are attacking from
            if (pawn_i >= 0 && pawn_i <= 7) {
                // check for in bounds and if it's an enemy pawn
                if (king_j + 1 <= 7 && boardConfiguration[pawn_i, king_j + 1] == enemyPawn) {
                    attackers.Add(pawn_i * 8 + king_j + 1);
                }
                if (attackers.Count < 2 && king_j - 1 >= 0 && boardConfiguration[pawn_i, king_j - 1] == enemyPawn) {
                    attackers.Add(pawn_i * 8 + king_j - 1);
                }
            }
        }

        char kingChar = gameState.whoMoves == 'w' ? 'K' : 'k';
        char enemyRook = gameState.whoMoves == 'w' ? 'r' : 'R';
        char enemyQueen = gameState.whoMoves == 'w' ? 'q' : 'Q';
        char enemyBishop = gameState.whoMoves == 'w' ? 'b' : 'B';

        if (attackers.Count < 2) {
            // check for attacking diagonal pieces

            int[] diagonalXIncrements = new int[4] { -1, -1, 1, 1 };
            int[] diagonalYIncrements = new int[4] { 1, -1, -1, 1 };

            // check all four diagonals
            for (int idx = 0; idx < 4; ++idx) {
                for (int piece_i = king_i + diagonalYIncrements[idx], piece_j = king_j + diagonalXIncrements[idx];
                    piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
                    piece_i += diagonalYIncrements[idx], piece_j += diagonalXIncrements[idx]) {
                    // if we are here then the piece is in bounds

                    char potentialPiece = boardConfiguration[piece_i, piece_j];
                    if (potentialPiece == enemyQueen || potentialPiece == enemyBishop) {
                        attackers.Add(piece_i * 8 + piece_j);
                        if (attackers.Count > 1) {
                            // we don't have to look for more attackers
                            idx = 4;
                        }
                        // this diagonal was checked
                        break;
                    } else if (potentialPiece != '-' && potentialPiece != kingChar) {
                        // this means we found a piece that would block the check
                        break;
                    }
                }
            }
        }

        if (attackers.Count < 2) {
            // check for line-attacking pieces

            int[] lineXIncrements = new int[4] { -1, 0, 0, 1 };
            int[] lineYIncrements = new int[4] { 0, -1, 1, 0 };

            // check all four directions (left, right, up, down)
            for (int idx = 0; idx < 4; ++idx) {
                for (int piece_i = king_i + lineYIncrements[idx], piece_j = king_j + lineXIncrements[idx];
                    piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
                    piece_i += lineYIncrements[idx], piece_j += lineXIncrements[idx]) {
                    // if we are here then the piece is in bounds

                    char potentialPiece = boardConfiguration[piece_i, piece_j];
                    if (potentialPiece == enemyQueen || potentialPiece == enemyRook) {
                        attackers.Add(piece_i * 8 + piece_j);
                        if (attackers.Count > 1) {
                            // we don't have to look for more attackers
                            idx = 4;
                        }
                        // this direction was checked
                        break;
                    } else if (potentialPiece != '-' && potentialPiece != kingChar) {
                        // this means we found a piece that would block the check
                        break;
                    }
                }
            }
        }

        if (restoreBoard) {
            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
        }

        return attackers;
    }

    public static bool IsKingSafeFromBishop(int king_i, int king_j, int bishop_i, int bishop_j, IndexMove indexMove = null) {
        if (indexMove == null) {
            // king and bishop are not even on the same diagonal
            if (Math.Abs(king_i - bishop_i) != Math.Abs(king_j - bishop_j)) {
                return true;
            }
            // king captured the bishop
            if (king_i == bishop_i && king_j == bishop_j) {
                return true;
            }
            return false;
        }
        int piece_i = indexMove.newRow, piece_j = indexMove.newColumn;
        // moved piece is not on the same diagonal as the bishop and king, therefore it can't resolve the check
        if (Math.Abs(king_i - piece_i) != Math.Abs(king_j - piece_j) || Math.Abs(bishop_i - piece_i) != Math.Abs(bishop_j - piece_j)) {
            return false;
        }
        // if bishop is blocked or captured
        if ((king_i < bishop_i && piece_i > king_i && piece_i <= bishop_i) || (king_i > bishop_i && piece_i < king_i && piece_i >= bishop_i)) {
            return true;
        }
        return false;
    }

    public static bool IsKingSafeFromRook(int king_i, int king_j, int rook_i, int rook_j, IndexMove indexMove = null) {
        if (indexMove == null) {
            // king and rook are not even on the same file
            if (king_i != rook_i && king_j != rook_j) {
                return true;
            }
            // king captured the rook
            if (king_i == rook_i && king_j == rook_j) {
                return true;
            }
            return false;
        }
        int piece_i = indexMove.newRow, piece_j = indexMove.newColumn;
        // moved piece is not on the same file or rank as the rook and king, therefore it can't resolve the check
        if ((king_i == rook_i && piece_i != king_i) || (king_j == rook_j && piece_j != king_j)) {
            return false;
        }
        // if king and rook are on the same rank
        if (king_i == rook_i) {
            // if rook is blocked or captured
            if ((king_j < rook_j && piece_j > king_j && piece_j <= rook_j) || (king_j > rook_j && piece_j < king_j && piece_j >= rook_j)) {
                return true;
            }
            return false;
        }
        // here king and rook are on the same file, we check if rook is blocked or captured again
        if ((king_i < rook_i && piece_i > king_i && piece_i <= rook_i) || (king_i > rook_i && piece_i < king_i && piece_i >= rook_i)) {
            return true;
        }
        return false;
    }

    public static bool IsKingSafeFromKnight(int king_i, int king_j, int knight_i, int knight_j) {
        // king captured the knight
        if (king_i == knight_i || king_j == knight_j) {
            return true;
        }
        // knight cannot reach the king in an L shape (2 + 1 or 1 + 2) - 3 + 0 and 0 + 3 were dealt with in the previous if
        if (Math.Abs(king_i - knight_i) + Math.Abs(king_i - knight_i) != 3) {
            return true;
        }
        return false;
    }

    public static bool IsKingSafeFromPawn(int pawn_i, int pawn_j, GameState gameState, IndexMove indexMove) {
        // the piece is capturing the pawn
        if (indexMove.newRow == pawn_i && indexMove.newColumn == pawn_j) {
            return true;
        }
        // no chance of en-passant
        if (char.ToLower(gameState.boardConfiguration[indexMove.oldRow, indexMove.oldColumn]) != 'p') {
            return false;
        }
        // not capturing the en-passant pawn (which obviously has to be the pawn checking the king)
        if (indexMove.newRow != RowToRank(gameState.enPassantRank) || ColumnToFile(indexMove.newColumn) != gameState.enPassantFile) {
            return false;
        }
        return true;
    }

    public static bool IsKingSafeFromDiagonalDiscovery(GameState gameState, IndexMove indexMove) {
        int king_i, king_j;
        if (gameState.whoMoves == 'w') {
            king_i = gameState.whiteKingRow;
            king_j = gameState.whiteKingColumn;

        } else {
            king_i = gameState.blackKingRow;
            king_j = gameState.blackKingColumn;
        }
        char[,] boardConfiguration = gameState.boardConfiguration;

        bool restoreBoard = false, restorePiece = false;
        bool targetingEnPassant = false;
        char oldSquare = '-', newSquare = '-', enPassantSquare = '-';

        // a piece is moving so we need to check king safety on a new configuration
        if (indexMove.newRow != -1) {
            int old_i, new_i, old_j, new_j;

            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;
            new_i = indexMove.newRow;
            new_j = indexMove.newColumn;

            restoreBoard = true;
            oldSquare = boardConfiguration[old_i, old_j];
            newSquare = boardConfiguration[new_i, new_j];
            targetingEnPassant = gameState.enPassantFile == ColumnToFile(new_j) && gameState.enPassantRank == RowToRank(new_i);
            if (char.ToLower(oldSquare) == 'p' && targetingEnPassant) {
                if (gameState.whoMoves == 'w') {
                    enPassantSquare = boardConfiguration[new_i + 1, new_j];
                    boardConfiguration[new_i + 1, new_j] = '-';
                } else {
                    enPassantSquare = boardConfiguration[new_i - 1, new_j];
                    boardConfiguration[new_i - 1, new_j] = '-';
                }
            }
            boardConfiguration[old_i, old_j] = '-';
            boardConfiguration[new_i, new_j] = oldSquare;
        } else { // we need to make a piece dissapear
            int old_i, old_j;
            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;

            restorePiece = true;
            oldSquare = boardConfiguration[old_i, old_j];
            boardConfiguration[old_i, old_j] = '-';
        }

        int diagonalXIncrement = king_j > indexMove.oldColumn ? -1 : 1;
        int diagonalYIncrement = king_i > indexMove.oldRow ? -1 : 1;

        char enemyQueen = gameState.whoMoves == 'w' ? 'q' : 'Q';
        char enemyBishop = gameState.whoMoves == 'w' ? 'b' : 'B';

        // check for attacking diagonal pieces

        for (int piece_i = king_i + diagonalYIncrement, piece_j = king_j + diagonalXIncrement;
            piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
            piece_i += diagonalYIncrement, piece_j += diagonalXIncrement) {
            // if we are here then the piece is in bounds
            char potentialPiece = boardConfiguration[piece_i, piece_j];
            if (potentialPiece == enemyQueen || potentialPiece == enemyBishop) {
                if (restoreBoard) {
                    RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                } else if (restorePiece) {
                    boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = oldSquare;
                }
                return false;
            } else if (potentialPiece != '-') {
                // this means we found a piece that would block the check
                break;
            }
        }

        if (restoreBoard) {
            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
        } else if (restorePiece) {
            boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = oldSquare;
        }

        // the discovered diagonal is safe
        return true;
    }

    public static bool IsKingSafeFromLineDiscovery(GameState gameState, IndexMove indexMove) {
        int king_i, king_j;
        if (gameState.whoMoves == 'w') {
            king_i = gameState.whiteKingRow;
            king_j = gameState.whiteKingColumn;

        } else {
            king_i = gameState.blackKingRow;
            king_j = gameState.blackKingColumn;
        }
        char[,] boardConfiguration = gameState.boardConfiguration;

        bool restoreBoard = false, restorePiece = false;
        bool targetingEnPassant = false;
        char oldSquare = '-', newSquare = '-', enPassantSquare = '-';

        // a piece is moving so we need to check king safety on a new configuration
        if (indexMove.newRow != -1) {
            int old_i, new_i, old_j, new_j;

            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;
            new_i = indexMove.newRow;
            new_j = indexMove.newColumn;

            restoreBoard = true;
            oldSquare = boardConfiguration[old_i, old_j];
            newSquare = boardConfiguration[new_i, new_j];
            targetingEnPassant = gameState.enPassantFile == ColumnToFile(new_j) && gameState.enPassantRank == RowToRank(new_i);
            if (char.ToLower(oldSquare) == 'p' && targetingEnPassant) {
                if (gameState.whoMoves == 'w') {
                    enPassantSquare = boardConfiguration[new_i + 1, new_j];
                    boardConfiguration[new_i + 1, new_j] = '-';
                } else {
                    enPassantSquare = boardConfiguration[new_i - 1, new_j];
                    boardConfiguration[new_i - 1, new_j] = '-';
                }
            }
            boardConfiguration[old_i, old_j] = '-';
            boardConfiguration[new_i, new_j] = oldSquare;
        } else { // we need to make a piece dissapear
            int old_i, old_j;
            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;

            restorePiece = true;
            oldSquare = boardConfiguration[old_i, old_j];
            boardConfiguration[old_i, old_j] = '-';
        }

        int lineXIncrement = king_j > indexMove.oldColumn ? -1 : (king_j == indexMove.oldColumn ? 0 : 1);
        int lineYIncrement = king_i > indexMove.oldRow ? -1 : (king_i == indexMove.oldRow ? 0 : 1);

        char enemyQueen = gameState.whoMoves == 'w' ? 'q' : 'Q';
        char enemyRook = gameState.whoMoves == 'w' ? 'r' : 'R';

        // check for attacking line pieces

        for (int piece_i = king_i + lineYIncrement, piece_j = king_j + lineXIncrement;
            piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
            piece_i += lineYIncrement, piece_j += lineXIncrement) {
            // if we are here then the piece is in bounds
            char potentialPiece = boardConfiguration[piece_i, piece_j];
            if (potentialPiece == enemyQueen || potentialPiece == enemyRook) {
                if (restoreBoard) {
                    RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                } else if (restorePiece) {
                    boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = oldSquare;
                }
                return false;
            } else if (potentialPiece != '-') {
                // this means we found a piece that would block the check
                break;
            }
        }

        if (restoreBoard) {
            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
        } else if (restorePiece) {
            boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = oldSquare;
        }

        // the discovered line is safe
        return true;
    }

    public static bool IsKingSafeAt(int king_i, int king_j, GameState gameState, IndexMove indexMove = null) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        char[,] boardConfiguration = gameState.boardConfiguration;

        bool restoreBoard = false;
        bool targetingEnPassant = false;
        char oldSquare = '-', newSquare = '-', enPassantSquare = '-';

        // a piece is moving so we need to check king safety on a new configuration
        if (indexMove != null) {
            int old_i, new_i, old_j, new_j;

            old_i = indexMove.oldRow;
            old_j = indexMove.oldColumn;
            new_i = indexMove.newRow;
            new_j = indexMove.newColumn;

            restoreBoard = true;
            oldSquare = boardConfiguration[old_i, old_j];
            newSquare = boardConfiguration[new_i, new_j];
            targetingEnPassant = gameState.enPassantFile == ColumnToFile(new_j) && gameState.enPassantRank == RowToRank(new_i);
            if (char.ToLower(oldSquare) == 'p' && targetingEnPassant) {
                if (gameState.whoMoves == 'w') {
                    enPassantSquare = boardConfiguration[new_i + 1, new_j];
                    boardConfiguration[new_i + 1, new_j] = '-';
                } else {
                    enPassantSquare = boardConfiguration[new_i - 1, new_j];
                    boardConfiguration[new_i - 1, new_j] = '-';
                }
            }
            boardConfiguration[old_i, old_j] = '-';
            boardConfiguration[new_i, new_j] = oldSquare;
        }

        // check for attacking knights

        int[] knightXCheckList = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] knightYCheckList = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };
        char enemyKnight = gameState.whoMoves == 'w' ? 'n' : 'N';

        for (int idx = 0; idx < 8; ++idx) {
            int knight_i = king_i + knightYCheckList[idx];
            if (knight_i >= 0 && knight_i <= 7) {
                int knight_j = king_j + knightXCheckList[idx];
                if (knight_j >= 0 && knight_j <= 7) {
                    // if we are here then the position is in bounds
                    if (boardConfiguration[knight_i, knight_j] == enemyKnight) {
                        if (restoreBoard) {
                            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                        }
                        stopwatch.Stop();
                        GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
                        return false;
                    }
                }
            }
        }

        // check for attacking pawns

        char enemyPawn = gameState.whoMoves == 'w' ? 'p' : 'P';
        int pawn_i = gameState.whoMoves == 'w' ? king_i - 1 : king_i + 1; // the rank where pawns are attacking from
        if (pawn_i >= 0 && pawn_i <= 7) {
            // check for in bounds and if it's an enemy pawn
            if ((king_j + 1 <= 7 && boardConfiguration[pawn_i, king_j + 1] == enemyPawn) ||
                (king_j - 1 >= 0 && boardConfiguration[pawn_i, king_j - 1] == enemyPawn)) {
                if (restoreBoard) {
                    RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                }
                stopwatch.Stop();
                GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
                return false;
            }
        }

        // check for proximity with the other king

        int[] kingXCheckList = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] kingYCheckList = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
        char enemyKing = gameState.whoMoves == 'w' ? 'k' : 'K';

        for (int idx = 0; idx < 8; ++idx) {
            int enemy_king_i = king_i + kingYCheckList[idx];
            if (enemy_king_i >= 0 && enemy_king_i <= 7) {
                int enemy_king_j = king_j + kingXCheckList[idx];
                if (enemy_king_j >= 0 && enemy_king_j <= 7) {
                    // if we are here then the position is in bounds
                    if (boardConfiguration[enemy_king_i, enemy_king_j] == enemyKing) {
                        if (restoreBoard) {
                            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                        }
                        stopwatch.Stop();
                        GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
                        return false;
                    }
                }
            }
        }

        char kingChar = gameState.whoMoves == 'w' ? 'K' : 'k';
        char enemyRook = gameState.whoMoves == 'w' ? 'r' : 'R';
        char enemyQueen = gameState.whoMoves == 'w' ? 'q' : 'Q';
        char enemyBishop = gameState.whoMoves == 'w' ? 'b' : 'B';

        // check for attacking diagonal pieces

        int[] diagonalXIncrements = new int[4] { -1, -1, 1, 1 };
        int[] diagonalYIncrements = new int[4] { 1, -1, -1, 1 };

        // check all four diagonals
        for (int idx = 0; idx < 4; ++idx) {
            for (int piece_i = king_i + diagonalYIncrements[idx], piece_j = king_j + diagonalXIncrements[idx];
                piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
                piece_i += diagonalYIncrements[idx], piece_j += diagonalXIncrements[idx]) {
                // if we are here then the piece is in bounds

                char potentialPiece = boardConfiguration[piece_i, piece_j];
                if (potentialPiece == enemyQueen || potentialPiece == enemyBishop) {
                    if (restoreBoard) {
                        RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                    }
                    stopwatch.Stop();
                    GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
                    return false;
                } else if (potentialPiece != '-' && potentialPiece != kingChar) {
                    // this means we found a piece that would block the check
                    break;
                }
            }
        }

        // check for line-attacking pieces

        int[] lineXIncrements = new int[4] { -1, 0, 0, 1 };
        int[] lineYIncrements = new int[4] { 0, -1, 1, 0 };

        // check all four directions (left, right, up, down)
        for (int idx = 0; idx < 4; ++idx) {
            for (int piece_i = king_i + lineYIncrements[idx], piece_j = king_j + lineXIncrements[idx];
                piece_i >= 0 && piece_i <= 7 && piece_j >= 0 && piece_j <= 7;
                piece_i += lineYIncrements[idx], piece_j += lineXIncrements[idx]) {
                // if we are here then the piece is in bounds

                char potentialPiece = boardConfiguration[piece_i, piece_j];
                if (potentialPiece == enemyQueen || potentialPiece == enemyRook) {
                    if (restoreBoard) {
                        RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
                    }
                    stopwatch.Stop();
                    GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
                    return false;
                } else if (potentialPiece != '-' && potentialPiece != kingChar) {
                    // this means we found a piece that would block the check
                    break;
                }
            }
        }

        if (restoreBoard) {
            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, indexMove, oldSquare, newSquare, enPassantSquare);
        }

        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks6 += stopwatch.ElapsedTicks;
        // we have checked all possible enemy pieces attacks
        return true;
    }

    static void RestoreBoard(char[,] boardConfiguration, bool targetingEnPassant, char whoMoves, IndexMove indexMove,
        char oldSquare, char newSquare, char enPassantSquare) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

        if (char.ToLower(oldSquare) == 'p' && targetingEnPassant) {
            if (whoMoves == 'w') {
                boardConfiguration[new_i + 1, new_j] = enPassantSquare;
            } else {
                boardConfiguration[new_i - 1, new_j] = enPassantSquare;
            }
        }
        boardConfiguration[old_i, old_j] = oldSquare;
        boardConfiguration[new_i, new_j] = newSquare;
    }
    public static char ColumnToFile(int j) {
        return (char)(j + 'a');
    }
    public static int RowToRank(int i) {
        return 8 - i;
    }
}
