using System;
using static KingSafety;

public static class MoveValidator {

    public static bool IsLegalMove(Move move, GameState gameState) {
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

    public static bool IsLegalPawnMove(IndexMove indexMove, GameState gameState) {
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
    public static bool IsLegalRookMove(IndexMove indexMove, GameState gameState) {
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

    public static bool IsLegalBishopMove(IndexMove indexMove, GameState gameState) {
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

    public static bool IsLegalQueenMove(IndexMove indexMove, GameState gameState) {
        // queen can move either as a rook or as a bishop
        return IsLegalBishopMove(indexMove, gameState) || IsLegalRookMove(indexMove, gameState);
    }

    public static bool IsLegalKnightMove(IndexMove indexMove, GameState gameState) {
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

    public static bool IsLegalKingMove(IndexMove indexMove, GameState gameState) {
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

    private static string GetPieceType(char x) {
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
}
