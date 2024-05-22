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
        char whoMoves = gameState.whoMoves;
        int king_i, king_j;
        if (whoMoves == 'w') {
            king_i = gameState.whiteKingRow;
            king_j = gameState.whiteKingColumn;
        } else {
            king_i = gameState.blackKingRow;
            king_j = gameState.blackKingColumn;
        }

        return GetPieceType(pieceChar) switch {
            "rook" => IsLegalRookMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(king_i, king_j, gameState, old_i, old_j, new_i, new_j),
            "knight" => IsLegalKnightMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(king_i, king_j, gameState, old_i, old_j, new_i, new_j),
            "bishop" => IsLegalBishopMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(king_i, king_j, gameState, old_i, old_j, new_i, new_j),
            "queen" => IsLegalQueenMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(king_i, king_j, gameState, old_i, old_j, new_i, new_j),
            "king" => IsLegalKingMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(new_i, new_j, gameState, -1, -1, -1, -1),
            "pawn" => IsLegalPawnMove(old_i, old_j, new_i, new_j, gameState) && IsKingSafeAt(king_i, king_j, gameState, old_i, old_j, new_i, new_j),
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
            return false;
        }

        if (movingSameRank) { // moving horizontally
            int increment = (new_j > old_j) ? 1 : -1; // choose direction
            for (int j = old_j + increment; j != new_j; j += increment) {
                // found a piece in the way
                if (gameState.boardConfiguration[old_i, j] != '-') {
                    return false;
                }
            }
        } else { // moving vertically
            int increment = (new_i > old_i) ? 1 : -1; // choose direction
            for (int i = old_i + increment; i != new_i; i += increment) {
                // found a piece in the way
                if (gameState.boardConfiguration[i, old_j] != '-') {
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

    public bool IsKingSafeAt(int king_i, int king_j, GameState gameState, int old_i, int old_j, int new_i, int new_j) {
        char[,] boardConfiguration = gameState.boardConfiguration;

        bool restoreBoard = false;
        bool targetingEnPassant = false;
        char oldSquare = '-', newSquare = '-', enPassantSquare = '-';
        
        // a piece is moving so we need to check king safety on a new configuration
        if (old_i != -1) {
            restoreBoard = true;
            oldSquare = boardConfiguration[old_i, old_j];
            newSquare = boardConfiguration[new_i, new_j];
            targetingEnPassant = gameState.enPassantFile == (char)('a' + new_j) && gameState.enPassantRank == 8 - new_i;
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
                        Debug.Log("false 1");
                        if (restoreBoard) {
                            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                                oldSquare, newSquare, enPassantSquare);
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
                    RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                        oldSquare, newSquare, enPassantSquare);
                }
                Debug.Log("false 2");
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
                            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                                oldSquare, newSquare, enPassantSquare);
                        }
                        Debug.Log("false 3");
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
                    Debug.Log("false 4");
                    if (restoreBoard) {
                        RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                            oldSquare, newSquare, enPassantSquare);
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
                    Debug.Log("false 5");
                    if (restoreBoard) {
                        RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                            oldSquare, newSquare, enPassantSquare);
                    }
                    return false;
                } else if (potentialPiece != '-' && potentialPiece != kingChar) {
                    // this means we found a piece that would block the check
                    break;
                }
            }
        }

        if (restoreBoard) {
            RestoreBoard(boardConfiguration, targetingEnPassant, gameState.whoMoves, old_i, old_j, new_i, new_j,
                oldSquare, newSquare, enPassantSquare);
        }

        // we have checked all possible enemy pieces attacks
        return true;
    }

    void RestoreBoard(char[,] boardConfiguration, bool targetingEnPassant, char whoMoves, int old_i, int old_j,
                        int new_i, int new_j, char oldSquare, char newSquare, char enPassantSquare) {
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
