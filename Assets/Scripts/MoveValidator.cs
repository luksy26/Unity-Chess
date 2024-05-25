using System;
using UnityEngine;

public class MoveValidator : MonoBehaviour {

    public bool IsLegalMove(Move move, GameState gameState) {
        char oldFile, newFile;
        int oldRank, newRank;

        oldFile = move.oldFile;
        newFile = move.newFile;
        oldRank = move.oldRank;
        newRank = move.newRank;

        // trying to move in place
        if (oldFile == newFile && oldRank == newRank) {
            return false;
        }
        char[,] boardConfiguration = gameState.boardConfiguration;

        IndexMove indexMove = new(move);
        int old_i = indexMove.oldRow;
        int old_j = indexMove.oldColumn;
        int new_i = indexMove.newRow;
        int new_j = indexMove.newColumn;

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
            "rook" => IsLegalRookMove(indexMove, gameState) && IsKingSafeAt(king_i, king_j, gameState, indexMove),
            "knight" => IsLegalKnightMove(indexMove, gameState) && IsKingSafeAt(king_i, king_j, gameState, indexMove),
            "bishop" => IsLegalBishopMove(indexMove, gameState) && IsKingSafeAt(king_i, king_j, gameState, indexMove),
            "queen" => IsLegalQueenMove(indexMove, gameState) && IsKingSafeAt(king_i, king_j, gameState, indexMove),
            "king" => IsLegalKingMove(indexMove, gameState) && IsKingSafeAt(new_i, new_j, gameState, null),
            "pawn" => IsLegalPawnMove(indexMove, gameState) && IsKingSafeAt(king_i, king_j, gameState, indexMove),
            _ => false,
        };
    }

    public bool IsLegalPawnMove(IndexMove indexMove, GameState gameState) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

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
            int old_rank = RowToRank(old_i);
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
                (RowToRank(new_i) != gameState.enPassantRank || ColumnToFile(new_j) != gameState.enPassantFile)) {
            return false;
        }
        // we have covered all possible illegal pawn moves (not considering pinned pieces)
        return true;
    }
    public bool IsLegalRookMove(IndexMove indexMove, GameState gameState) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

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

    public bool IsLegalBishopMove(IndexMove indexMove, GameState gameState) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

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

    public bool IsLegalQueenMove(IndexMove indexMove, GameState gameState) {
        // queen can move either as a rook or as a bishop
        return IsLegalBishopMove(indexMove, gameState) || IsLegalRookMove(indexMove, gameState);
    }

    public bool IsLegalKnightMove(IndexMove indexMove, GameState gameState) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

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

    public bool IsLegalKingMove(IndexMove indexMove, GameState gameState) {
        int old_i, new_i, old_j, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

        bool moving2Files = Math.Abs(new_j - old_j) == 2;

        // check if trying to castle
        if (moving2Files) {
            bool kingOnProperFile = old_j == 4;
            if (!kingOnProperFile) {
                return false;
            }
            bool kingSameRank = old_i == new_i;
            if (!kingSameRank) {
                return false;
            }
            bool backRank = (gameState.whoMoves == 'w' && old_i == 7) || (gameState.whoMoves == 'b' && old_i == 0);
            if (!backRank) {
                return false;
            }
            // trying to castle while checked
            if (!IsKingSafeAt(old_i, old_j, gameState, null)) {
                return false;
            }
            char[,] boardConfiguration = gameState.boardConfiguration;
            // trying to short castle
            if (new_j > old_j) {
                // already moved the king or rooks
                if ((gameState.whoMoves == 'w' && !gameState.white_O_O) || (gameState.whoMoves == 'b' && !gameState.black_O_O)) {
                    return false;
                }
                bool blockingPieces = boardConfiguration[old_i, old_j + 1] != '-' || boardConfiguration[old_i, old_j + 2] != '-';
                if (blockingPieces) {
                    return false;
                }
                // trying to castle through check
                if (!IsKingSafeAt(old_i, old_j + 1, gameState, null)) {
                    return false;
                }
                // can castle (haven't checked for final position checks)
                return true;
            }
            // trying to long castle
            if (new_j < old_j) {
                // already moved the king or rooks
                if ((gameState.whoMoves == 'w' && !gameState.white_O_O_O) || (gameState.whoMoves == 'b' && !gameState.black_O_O_O)) {
                    return false;
                }
                bool blockingPieces = boardConfiguration[old_i, old_j - 1] != '-' || boardConfiguration[old_i, old_j - 2] != '-' ||
                        boardConfiguration[old_i, old_j - 3] != '-';
                if (blockingPieces) {
                    return false;
                }
                // trying to castle through check
                if (!IsKingSafeAt(old_i, old_j - 1, gameState, null)) {
                    return false;
                }
                // can castle (haven't checked for final position checks)
                return true;
            }
        }

        // moving too far
        if (Math.Abs(new_i - old_i) > 1 || Math.Abs(new_j - old_j) > 1) {
            return false;
        }
        // we have covered all possible illegal king moves (not considering moving into check)
        return true;
    }

    public bool IsKingSafeAt(int king_i, int king_j, GameState gameState, IndexMove indexMove) {

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

    void RestoreBoard(char[,] boardConfiguration, bool targetingEnPassant, char whoMoves, IndexMove indexMove,
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
    private char ColumnToFile(int j) {
        return (char)(j + 'a');
    }
    private int RowToRank(int i) {
        return 8 - i;
    }
}
