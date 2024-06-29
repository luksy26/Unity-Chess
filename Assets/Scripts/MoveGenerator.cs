using System;
using System.Collections.Generic;
using System.Diagnostics;
using static KingSafety;

public static class MoveGenerator {

    public struct Attacker {
        public int type, row, column;
    }
    public static List<IndexMove> GetLegalMoves(GameState gameState) {
        List<IndexMove> legalMoves = new();
        char[,] boardConfiguration = gameState.boardConfiguration;
        bool kingInDoubleCheck = false;
        int kingRow, kingColumn;

        if (gameState.whoMoves == 'w') {
            kingRow = gameState.whiteKingRow;
            kingColumn = gameState.whiteKingColumn;

        } else {
            kingRow = gameState.blackKingRow;
            kingColumn = gameState.blackKingColumn;
        }

        List<int> attackers = GetKingAttackers(gameState);

        Attacker attacker1 = new() {
            type = -1
        }, attacker2 = new() {
            type = -1
        };

        if (attackers.Count > 0) {
            attacker1.row = attackers[0] / 8;
            attacker1.column = attackers[0] % 8;
            switch (char.ToLower(boardConfiguration[attacker1.row, attacker1.column])) {
                case 'p': attacker1.type = 0; break;
                case 'b': attacker1.type = 1; break;
                case 'r': attacker1.type = 2; break;
                case 'n': attacker1.type = 3; break;
                case 'q': // queen is attacking like a rook
                    if (attacker1.row == kingRow || attacker1.column == kingColumn) {
                        attacker1.type = 2;
                    } else { // queen is attacking like a bishop
                        attacker1.type = 1;
                    }
                    break;
                default: break;
            }
            if (attackers.Count > 1) {
                kingInDoubleCheck = true;
                attacker2.row = attackers[1] / 8;
                attacker2.column = attackers[1] % 8;
                switch (char.ToLower(boardConfiguration[attacker2.row, attacker2.column])) {
                    case 'p': attacker2.type = 0; break;
                    case 'b': attacker2.type = 1; break;
                    case 'r': attacker2.type = 2; break;
                    case 'n': attacker2.type = 3; break;
                    case 'q': // queen is attacking like a rook
                        if (attacker2.row == kingRow || attacker2.column == kingColumn) {
                            attacker2.type = 2;
                        } else { // queen is attacking like a bishop
                            attacker2.type = 1;
                        }
                        break;
                    default: break;
                }
            }
        }

        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                if (GetPieceOwner(boardConfiguration[i, j]) == gameState.whoMoves) {
                    switch (boardConfiguration[i, j]) {
                        case 'p' or 'P': if (!kingInDoubleCheck) AddLegalPawnMoves(gameState, i, j, attacker1, legalMoves); break;
                        case 'b' or 'B': if (!kingInDoubleCheck) AddLegalBishopMoves(gameState, i, j, attacker1, legalMoves); break;
                        case 'n' or 'N': if (!kingInDoubleCheck) AddLegalKnightMoves(gameState, i, j, attacker1, legalMoves); break;
                        case 'r' or 'R': if (!kingInDoubleCheck) AddLegalRookMoves(gameState, i, j, attacker1, legalMoves); break;
                        case 'q' or 'Q': if (!kingInDoubleCheck) AddLegalQueenMoves(gameState, i, j, attacker1, legalMoves); break;
                        case 'k' or 'K': AddLegalKingMoves(gameState, i, j, attacker1, attacker2, legalMoves); break;
                        default: break;
                    }
                }
            }
        }
        return legalMoves;
    }

    public static void AddLegalPawnMoves(GameState gameState, int old_i, int old_j, Attacker attacker, List<IndexMove> legalMoves) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

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

        bool sameDiagonal = Math.Abs(kingRow - old_i) == Math.Abs(kingColumn - old_j);
        bool sameRank = kingRow == old_i;
        bool sameFile = kingColumn == old_j;
        bool sameLine = sameRank || sameFile;

        // check forward pawn movement (square above the pawn must be empty)
        if (boardConfiguration[old_i + forwardY, old_j] == '-') {
            IndexMove pawnOneUp = new(old_i, old_j, old_i + forwardY, old_j);
            bool resolvesAttacker = false;
            switch (attacker.type) {
                case -1: resolvesAttacker = true; break; // king is not in check
                case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, pawnOneUp); break;
                case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, pawnOneUp); break;
                case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, pawnOneUp); break;
                case 3: resolvesAttacker = false; break; // pawn moving forward can't capture a knight
            }
            // if a potential attacker has been blocked/captured
            if (resolvesAttacker) {
                bool keepsLine = kingColumn == old_j || kingRow == old_i + forwardY;
                // moving the piece doesn't cause the king to be in check
                if ((!sameDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, pawnOneUp)) &&
                (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, pawnOneUp))) {
                    if (canPromote) {
                        AddAllPromotionTypes(pawnOneUp, whoMoves, legalMoves);
                    } else {
                        legalMoves.Add(pawnOneUp);
                    }
                }
            }

            // pawn can move two squares
            if (old_i == startingRow && boardConfiguration[old_i + 2 * forwardY, old_j] == '-') {
                IndexMove pawnTwoUp = new(old_i, old_j, old_i + 2 * forwardY, old_j);
                resolvesAttacker = false;
                switch (attacker.type) {
                    case -1: resolvesAttacker = true; break;
                    case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, pawnTwoUp); break;
                    case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, pawnTwoUp); break;
                    case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, pawnTwoUp); break;
                    case 3: resolvesAttacker = false; break;
                }
                // if a potential attacker has been blocked/captured
                if (resolvesAttacker) {

                    bool keepsLine = kingColumn == old_j || kingRow == old_i + 2 * forwardY;
                    // moving the piece doesn't cause the king to be in check
                    if ((!sameDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, pawnTwoUp)) &&
                    (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, pawnTwoUp))) {
                        if (canPromote) {
                            AddAllPromotionTypes(pawnTwoUp, whoMoves, legalMoves);
                        } else {
                            legalMoves.Add(pawnTwoUp);
                        }
                    }
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
                bool resolvesAttacker = false;
                switch (attacker.type) {
                    case -1: resolvesAttacker = true; break;
                    case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, captureRight); break;
                    case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, captureRight); break;
                    case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, captureRight); break;
                    case 3: resolvesAttacker = old_i + forwardY == attacker.row && old_j + 1 == attacker.column; break;
                }
                // if a potential attacker has been blocked/captured
                if (resolvesAttacker) {

                    bool keepsDiagonal = Math.Abs(kingRow - old_i - forwardY) == Math.Abs(kingColumn - old_j - 1);
                    bool keepsLine = (sameRank && kingRow == old_i + forwardY) || (sameFile && kingColumn == old_j + 1);
                    // moving the piece doesn't cause the king to be in check
                    if ((!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, captureRight)) &&
                    (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, captureRight))) {
                        if (canPromote) {
                            AddAllPromotionTypes(captureRight, whoMoves, legalMoves);
                        } else {
                            legalMoves.Add(captureRight);
                        }
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
                bool resolvesAttacker = false;
                switch (attacker.type) {
                    case -1: resolvesAttacker = true; break;
                    case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, captureLeft); break;
                    case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, captureLeft); break;
                    case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, captureLeft); break;
                    case 3: resolvesAttacker = old_i + forwardY == attacker.row && old_j - 1 == attacker.column; break;
                }
                // if a potential attacker has been blocked/captured
                if (resolvesAttacker) {

                    bool keepsDiagonal = Math.Abs(kingRow - old_i - forwardY) == Math.Abs(kingColumn - old_j + 1);
                    bool keepsLine = (sameRank && kingRow == old_i + forwardY) || (sameFile && kingColumn == old_j - 1);
                    // moving the piece doesn't cause the king to be in check
                    if ((!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, captureLeft)) &&
                    (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, captureLeft))) {
                        if (canPromote) {
                            AddAllPromotionTypes(captureLeft, whoMoves, legalMoves);
                        } else {
                            legalMoves.Add(captureLeft);
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks2 += stopwatch.ElapsedTicks;
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
    public static void AddLegalBishopMoves(GameState gameState, int old_i, int old_j, Attacker attacker, List<IndexMove> legalMoves) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

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

        bool sameDiagonal = Math.Abs(kingRow - old_i) == Math.Abs(kingColumn - old_j);
        bool sameRank = kingRow == old_i;
        bool sameFile = kingColumn == old_j;
        bool sameLine = sameRank || sameFile;

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
        if (attacker.type == -1) {
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

                    bool resolvesAttacker = false;
                    switch (attacker.type) {
                        case -1: resolvesAttacker = true; break;
                        case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, bishopMove); break;
                        case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, bishopMove); break;
                        case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, bishopMove); break;
                        case 3: resolvesAttacker = new_i == attacker.row && new_j == attacker.column; break;
                    }
                    // if a potential attacker has been blocked/captured
                    if (resolvesAttacker) {
                        bool keepsDiagonal = Math.Abs(kingRow - new_i) == Math.Abs(kingColumn - new_j);
                        bool keepsLine = false; // a bishop can never keep the same line as the king

                        // capturing an enemy piece
                        if (pieceOwner == opponent) {
                            // do final checks and stop search in this quadrant
                            // if 2 safe squares were already found, it means we are on a safe diagonal
                            if (noSafeSquares > 1 ||
                            (!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, bishopMove)) &&
                            (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, bishopMove))) {
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
                        if (noSafeSquares > 1 ||
                            (!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, bishopMove)) &&
                            (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, bishopMove))) {
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
                    } else {
                        ++noUnsafeSquares;
                        if (noSafeSquares > 0) { // the only safe square on this unsafe diagonal was already found
                            ++quadrant; // no point in searching more safe squares on this diagonal
                            break; // of course, not on the quadrant either, since it's part of the diagonal
                        }
                        if (pieceOwner == opponent) {
                            break; // rest of quadrant is blocked
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks3 += stopwatch.ElapsedTicks;
    }
    public static void AddLegalKnightMoves(GameState gameState, int old_i, int old_j, Attacker attacker, List<IndexMove> legalMoves) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        char[,] boardConfiguration = gameState.boardConfiguration;
        int kingRow, kingColumn;
        if (gameState.whoMoves == 'w') {
            kingRow = gameState.whiteKingRow;
            kingColumn = gameState.whiteKingColumn;

        } else {
            kingRow = gameState.blackKingRow;
            kingColumn = gameState.blackKingColumn;
        }

        bool sameDiagonal = Math.Abs(kingRow - old_i) == Math.Abs(kingColumn - old_j);
        bool sameRank = kingRow == old_i;
        bool sameFile = kingColumn == old_j;
        bool sameLine = sameRank || sameFile;

        int[] LshapeXIncrements = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] LshapeYIncrements = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };

        // knight is on the same diagonal as the king and is pinned
        if (sameDiagonal && !IsKingSafeFromDiagonalDiscovery(gameState, new IndexMove(old_i, old_j, -1, -1))) {
            stopwatch.Stop();
            GameStateManager.Instance.numberOfTicks4 += stopwatch.ElapsedTicks;
            return;
        }

        // knight is on the same file/rank as the king and is pinned
        if (sameLine && !IsKingSafeFromLineDiscovery(gameState, new IndexMove(old_i, old_j, -1, -1))) {
            stopwatch.Stop();
            GameStateManager.Instance.numberOfTicks4 += stopwatch.ElapsedTicks;
            return;
        };

        for (int idx = 0; idx < 8; ++idx) {
            int new_i = old_i + LshapeYIncrements[idx];
            int new_j = old_j + LshapeXIncrements[idx];
            // in bounds
            if (new_i >= 0 && new_i <= 7 && new_j >= 0 && new_j <= 7) {
                char pieceOwner = GetPieceOwner(boardConfiguration[new_i, new_j]);
                // knight can't move here, it is blocked by own piece
                if (pieceOwner == gameState.whoMoves) {
                    continue;
                }
                IndexMove knightMove = new(old_i, old_j, new_i, new_j);

                bool resolvesAttacker = false;
                switch (attacker.type) {
                    case -1: resolvesAttacker = true; break;
                    case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, knightMove); break;
                    case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, knightMove); break;
                    case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, knightMove); break;
                    case 3: resolvesAttacker = new_i == attacker.row && new_j == attacker.column; break;
                }

                // if a potential attacker has been blocked/captured
                if (resolvesAttacker) {
                    // we've already checked if the knight was pinned
                    legalMoves.Add(knightMove);
                }
            }
        }
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks4 += stopwatch.ElapsedTicks;
    }
    public static void AddLegalRookMoves(GameState gameState, int old_i, int old_j, Attacker attacker, List<IndexMove> legalMoves) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

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

        bool sameDiagonal = Math.Abs(kingRow - old_i) == Math.Abs(kingColumn - old_j);
        bool sameRank = kingRow == old_i;
        bool sameFile = kingColumn == old_j;
        bool sameLine = sameRank || sameFile;

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
        if (attacker.type == -1) {
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

                    bool resolvesAttacker = false;
                    switch (attacker.type) {
                        case -1: resolvesAttacker = true; break;
                        case 0: resolvesAttacker = IsKingSafeFromPawn(attacker.row, attacker.column, gameState, rookMove); break;
                        case 1: resolvesAttacker = IsKingSafeFromBishop(kingRow, kingColumn, attacker.row, attacker.column, rookMove); break;
                        case 2: resolvesAttacker = IsKingSafeFromRook(kingRow, kingColumn, attacker.row, attacker.column, rookMove); break;
                        case 3: resolvesAttacker = new_i == attacker.row && new_j == attacker.column; break;
                    }

                    // if a potential attacker has been blocked/captured
                    if (resolvesAttacker) {
                        bool keepsDiagonal = false; // rook can never keep the same diagonal with the king 
                        bool keepsLine = (sameRank && kingRow == new_i) || (sameFile && kingColumn == new_j);
                        // capturing an enemy piece
                        if (pieceOwner == opponent) {
                            // do final checks and stop search in this direction
                            // if 2 safe squares were already found, it means we are on a safe line
                            if (noSafeSquares > 1 ||
                            (!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, rookMove)) &&
                            (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, rookMove))) {
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
                        if (noSafeSquares > 1 ||
                        (!sameDiagonal || keepsDiagonal || IsKingSafeFromDiagonalDiscovery(gameState, rookMove)) &&
                        (!sameLine || keepsLine || IsKingSafeFromLineDiscovery(gameState, rookMove))) {
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
                    } else {
                        ++noUnsafeSquares;
                        if (noSafeSquares > 0) { // the only safe square on this unsafe diagonal was already found
                            ++direction; // no point in searching more safe squares on this diagonal
                            break; // of course, not on the quadrant either, since it's part of the diagonal
                        }
                        if (pieceOwner == opponent) {
                            break; // rest of quadrant is blocked
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks5 += stopwatch.ElapsedTicks;
    }

    public static void AddLegalQueenMoves(GameState gameState, int old_i, int old_j, Attacker attacker, List<IndexMove> legalMoves) {
        // queen can move either as a bishop or as a rook
        AddLegalBishopMoves(gameState, old_i, old_j, attacker, legalMoves);
        AddLegalRookMoves(gameState, old_i, old_j, attacker, legalMoves);
    }
    public static void AddLegalKingMoves(GameState gameState, int old_i, int old_j, Attacker attacker1, Attacker attacker2, List<IndexMove> legalMoves) {
        Stopwatch stopwatch = new();
        stopwatch.Start();

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
                // king is blocked by own piece
                if (pieceOwner == gameState.whoMoves) {
                    continue;
                }
                IndexMove kingMove = new(old_i, old_j, new_i, new_j);
                bool resolvesFirstAttacker = false, resolvesSecondAttacker = false;
                switch (attacker1.type) {
                    case -1: resolvesFirstAttacker = true; break;
                    case 0: resolvesFirstAttacker = true; break; // king can't move into the same pawn's check
                    case 1: resolvesFirstAttacker = IsKingSafeFromBishop(new_i, new_j, attacker1.row, attacker1.column); break;
                    case 2: resolvesFirstAttacker = IsKingSafeFromRook(new_i, new_j, attacker1.row, attacker1.column); break;
                    case 3: resolvesFirstAttacker = IsKingSafeFromKnight(new_i, new_j, attacker1.row, attacker1.column); break;
                }
                if (resolvesFirstAttacker) {
                    switch (attacker2.type) {
                        case -1: resolvesSecondAttacker = true; break;
                        case 0: resolvesSecondAttacker = true; break; // king can't move into the same pawn's check
                        case 1: resolvesSecondAttacker = IsKingSafeFromBishop(new_i, new_j, attacker2.row, attacker2.column); break;
                        case 2: resolvesSecondAttacker = IsKingSafeFromRook(new_i, new_j, attacker2.row, attacker2.column); break;
                        case 3: resolvesSecondAttacker = IsKingSafeFromKnight(new_i, new_j, attacker1.row, attacker1.column); break;
                    }
                }
                if (resolvesFirstAttacker && resolvesSecondAttacker && IsKingSafeAt(new_i, new_j, gameState)) {
                    legalMoves.Add(kingMove);
                }
            }
        }
        // if king is in check we cannot castle
        if (attacker1.type != -1) {
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
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks7 += stopwatch.ElapsedTicks;
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
    public static char ColumnToFile(int j) {
        return (char)(j + 'a');
    }
    public static int RowToRank(int i) {
        return 8 - i;
    }
}
