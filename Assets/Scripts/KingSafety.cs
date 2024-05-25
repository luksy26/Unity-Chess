public static class KingSafety {
    public static bool IsKingSafeAt(int king_i, int king_j, GameState gameState, IndexMove indexMove) {

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

        if (targetingEnPassant) {
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
