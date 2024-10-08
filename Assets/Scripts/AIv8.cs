using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static MoveGenerator;

/* 
    minimax with alpha beta
    evaluation based on controlled squares (+ distance from center, distance from king),
    partial search at max depth
    3fold detection
    move ordering
*/
public static class AIv8 {
    public const int MOVE_FIRST_ADVANTAGE = 20;
    public const int SQUARE_CONTROL_BONUS = 1;
    public const int SQUARE_DEFEND_ATTACK_BONUS = 2;
    public const int SQUARE_DEFEND_ATTACK_EQUAL_BONUS = 5;
    public const int SQUARE_DEFEND_HIGHER_BONUS = 3;
    public const int SQUARE_ATTACK_HIGHER_BONUS = 10;
    public const int SQUARE_DEFEND_ATTACK_KING = 2;
    public const int PAWN_CHAIN_BONUS = 5;
    public const int ENDGAME_TRANSITION = 6;
    public const int PUNISH_REWARD_FACTOR = 5;
    public const int CENTER_CONTROL_BONUS = 2;
    public const int ENEMY_KING_CONTROL_BONUS = 2;
    static readonly int[] pieceValues = { 100, 320, 330, 500, 900, 0 };

    public static int maximumDepth;

    public static float PositionEvaluator(GameState gameState, int depth, List<IndexMove> legalMoves) {
        GameConclusion conclusion = GameStateManager.Instance.GetDrawConclusion(gameState);
        if (conclusion == GameConclusion.DrawByInsufficientMaterial || conclusion == GameConclusion.DrawBy50MoveRule) {
            return 0;
        }
        conclusion = GameStateManager.Instance.GetMateConclusion(gameState, legalMoves);
        if (conclusion == GameConclusion.Checkmate) {
            if (gameState.whoMoves == 'w') {
                return -1000f + depth;
            }
            return 1000f - depth;
        }
        if (conclusion == GameConclusion.Stalemate) {
            return 0;
        }

        float scoreWhite = 0, scoreBlack = 0;
        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                char potentialPiece = gameState.boardConfiguration[i, j];
                float pieceScore = 0;
                if (potentialPiece != '-') {
                    pieceScore = GetPiecePlacementScore(i, j, gameState);
                }
                if (char.IsLetter(potentialPiece)) {
                    if (char.IsUpper(potentialPiece)) {
                        scoreWhite += pieceScore;
                    } else {
                        scoreBlack += pieceScore;
                    }
                }
            }
        }
        // whoever moves first tends to have a small advantage (except for zugzwang)
        if (gameState.whoMoves == 'w') {
            scoreWhite += MOVE_FIRST_ADVANTAGE;
        } else {
            scoreBlack += MOVE_FIRST_ADVANTAGE;
        }
        return (scoreWhite - scoreBlack) / 100f;
    }

    public static float GetPiecePlacementScore(int row, int column, GameState gameState) {
        char[,] boardConfiguration = gameState.boardConfiguration;
        int pieceIndex = GetPieceIndex(boardConfiguration[row, column]);
        float pieceScore = 0;
        if (pieceIndex != -1) {
            pieceScore += pieceValues[pieceIndex];
        }

        pieceScore += GetPieceSquareControlScore(row, column, pieceIndex, gameState);

        return pieceScore;
    }

    public static float GetPieceSquareControlScore(int row, int column, int pieceIndex, GameState gameState) {
        return pieceIndex switch {
            0 => GetPawnSquareControlScore(row, column, pieceValues[0], gameState),
            1 => GetKnightControlScore(row, column, pieceValues[1], gameState),
            2 => GetBishopSquareControlScore(row, column, pieceValues[2], gameState),
            3 => GetRookSquareControlScore(row, column, pieceValues[3], gameState),
            4 => GetQueenSquareControlScore(row, column, pieceValues[4], gameState),
            5 => GetKingSquareControlScore(row, column, gameState),
            _ => 0,
        };
    }

    public static float GetPawnSquareControlScore(int row, int column, int pawnValue, GameState gameState) {
        float pawnScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        float relativePieceValueFactor = 200 / pawnValue;

        int forwardY = pieceOwner == 'w' ? -1 : 1;
        char friendlyPawn = pieceOwner == 'w' ? 'P' : 'p';
        int enemyKingRow = pieceOwner == 'w' ? gameState.blackKingRow : gameState.whiteKingRow;
        int enemyKingColumn = pieceOwner == 'w' ? gameState.blackKingColumn : gameState.whiteKingColumn;


        if (column - 1 >= 0) {
            // pawn controls a square
            pawnScore += SQUARE_CONTROL_BONUS;
            pawnScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(row, column - 1)), 2)  * CENTER_CONTROL_BONUS * relativePieceValueFactor;
            pawnScore += Math.Max(0, 2 - GetDistanceFromKing(row, column - 1, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
            // square controlled by the pawn
            int pieceIndex = GetPieceIndex(boardConfiguration[row + forwardY, column - 1]);
            char pieceChar;
            pieceChar = boardConfiguration[row + forwardY, column - 1];
            // pawn is defending/attacking a piece
            if (pieceIndex != -1) {
                pawnScore += SQUARE_DEFEND_ATTACK_BONUS;
                if (pieceChar == friendlyPawn) {
                    pawnScore += PAWN_CHAIN_BONUS;
                } else if (pawnValue < pieceValues[pieceIndex]) {
                    char targetPieceOwner = char.IsLower(pieceChar) ? 'b' : 'w';
                    if (targetPieceOwner == pieceOwner) {
                        pawnScore += SQUARE_DEFEND_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100;
                    } else {
                        pawnScore += SQUARE_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100;
                    }
                }
            }
        }
        if (column + 1 < 8) {
            // pawn controls a square
            pawnScore += SQUARE_CONTROL_BONUS;
            pawnScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(row, column + 1)), 2) * CENTER_CONTROL_BONUS * relativePieceValueFactor;
            pawnScore += Math.Max(0, 2 - GetDistanceFromKing(row, column + 1, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
            // square controlled by the pawn
            int pieceIndex = GetPieceIndex(boardConfiguration[row + forwardY, column + 1]);
            char pieceChar;
            pieceChar = boardConfiguration[row + forwardY, column + 1];
            // pawn is defending/attacking a piece
            if (pieceIndex != -1) {
                pawnScore += SQUARE_DEFEND_ATTACK_BONUS;
                if (pieceChar == friendlyPawn) {
                    pawnScore += PAWN_CHAIN_BONUS;
                } else if (pawnValue < pieceValues[pieceIndex]) {
                    char targetPieceOwner = char.IsLower(pieceChar) ? 'b' : 'w';
                    if (targetPieceOwner == pieceOwner) {
                        pawnScore += SQUARE_DEFEND_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100;
                    } else {
                        pawnScore += SQUARE_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100;
                    }
                }
            }
        }
        return pawnScore;
    }

    public static float GetKnightControlScore(int row, int column, int knightValue, GameState gameState) {
        float knightScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        int enemyKingRow = pieceOwner == 'w' ? gameState.blackKingRow : gameState.whiteKingRow;
        int enemyKingColumn = pieceOwner == 'w' ? gameState.blackKingColumn : gameState.whiteKingColumn;
        float relativePieceValueFactor = 200f / knightValue;

        int[] LshapeXIncrements = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] LshapeYIncrements = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };

        for (int idx = 0; idx < 8; ++idx) {
            int newRow = row + LshapeYIncrements[idx];
            int newColumn = column + LshapeXIncrements[idx];
            // in bounds
            if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8) {
                knightScore += SQUARE_CONTROL_BONUS;
                knightScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(newRow, newColumn)), 2) * CENTER_CONTROL_BONUS * relativePieceValueFactor;
                knightScore += Math.Max(0, 2 - GetDistanceFromKing(newRow, newColumn, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
                char pieceChar = boardConfiguration[newRow, newColumn];
                int pieceIndex = GetPieceIndex(pieceChar);
                if (pieceIndex != -1) {
                    if (knightValue == pieceValues[pieceIndex]) {
                        knightScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                    } else if (knightValue < pieceValues[pieceIndex]) {
                        char targetPieceOwner = char.IsLower(pieceChar) ? 'b' : 'w';
                        if (targetPieceOwner == pieceOwner) {
                            knightScore += SQUARE_DEFEND_HIGHER_BONUS * (pieceValues[pieceIndex] - knightValue) / 100f;
                        } else {
                            knightScore += SQUARE_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - knightValue) / 100f;
                        }
                    }
                }
            }
        }
        return knightScore;
    }

    public static float GetBishopSquareControlScore(int row, int column, int bishopValue, GameState gameState) {
        float bishopScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        int enemyKingRow = pieceOwner == 'w' ? gameState.blackKingRow : gameState.whiteKingRow;
        int enemyKingColumn = pieceOwner == 'w' ? gameState.blackKingColumn : gameState.whiteKingColumn;
        float relativePieceValueFactor = 200f / bishopValue;

        int[,] diagonalXIncrements = new int[,] { { 1, -1 }, { -1, 1 } };
        int[,] diagonalYIncrements = new int[,] { { -1, 1 }, { -1, 1 } };

        for (int diagonal = 0; diagonal < 2; ++diagonal) {
            for (int quadrant = 0; quadrant < 2; ++quadrant) {
                for (int newRow = row + diagonalYIncrements[diagonal, quadrant], newColumn = column + diagonalXIncrements[diagonal, quadrant];
                    newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8;
                    newRow += diagonalYIncrements[diagonal, quadrant], newColumn += diagonalXIncrements[diagonal, quadrant]) {
                    bishopScore += SQUARE_CONTROL_BONUS;
                    bishopScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(newRow, newColumn)), 2) * CENTER_CONTROL_BONUS * relativePieceValueFactor;
                    bishopScore += Math.Max(0, 2 - GetDistanceFromKing(newRow, newColumn, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
                    char pieceChar = boardConfiguration[newRow, newColumn];
                    int pieceIndex = GetPieceIndex(pieceChar);
                    if (pieceIndex != -1) {
                        if (bishopValue == pieceValues[pieceIndex]) {
                            bishopScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                        } else if (bishopValue < pieceValues[pieceIndex]) {
                            char targetPieceOwner = char.IsLower(pieceChar) ? 'b' : 'w';
                            if (targetPieceOwner == pieceOwner) {
                                bishopScore += SQUARE_DEFEND_HIGHER_BONUS * (pieceValues[pieceIndex] - bishopValue) / 100f;
                            } else {
                                bishopScore += SQUARE_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - bishopValue) / 100f;
                            }
                        }
                        break;
                    }
                }
            }
        }
        return bishopScore;
    }

    public static float GetRookSquareControlScore(int row, int column, int rookValue, GameState gameState) {
        float rookScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        int enemyKingRow = pieceOwner == 'w' ? gameState.blackKingRow : gameState.whiteKingRow;
        int enemyKingColumn = pieceOwner == 'w' ? gameState.blackKingColumn : gameState.whiteKingColumn;
        float relativePieceValueFactor = 200 / rookValue;

        int[,] lineXIncrements = new int[,] { { 1, -1 }, { 0, 0 } };
        int[,] lineYIncrements = new int[,] { { 0, 0 }, { -1, 1 } };

        for (int line = 0; line < 2; ++line) {
            for (int direction = 0; direction < 2; ++direction) {
                for (int newRow = row + lineYIncrements[line, direction], newColumn = column + lineXIncrements[line, direction];
                    newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8;
                    newRow += lineYIncrements[line, direction], newColumn += lineXIncrements[line, direction]) {
                    rookScore += SQUARE_CONTROL_BONUS;
                    rookScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(newRow, newColumn)), 2) * CENTER_CONTROL_BONUS * relativePieceValueFactor;
                    rookScore += Math.Max(0, 2 - GetDistanceFromKing(newRow, newColumn, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
                    char pieceChar = boardConfiguration[newRow, newColumn];
                    int pieceIndex = GetPieceIndex(pieceChar);
                    if (pieceIndex != -1) {
                        if (rookValue == pieceValues[pieceIndex]) {
                            rookScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                        } else if (rookValue < pieceValues[pieceIndex]) {
                            char targetPieceOwner = char.IsLower(pieceChar) ? 'b' : 'w';
                            if (targetPieceOwner == pieceOwner) {
                                rookScore += SQUARE_DEFEND_HIGHER_BONUS * (pieceValues[pieceIndex] - rookValue) / 100f;
                            } else {
                                rookScore += SQUARE_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - rookValue) / 100f;
                            }
                        }
                        break;
                    }
                }
            }
        }
        return rookScore;
    }

    public static float GetQueenSquareControlScore(int row, int column, int queenValue, GameState gameState) {
        return GetRookSquareControlScore(row, column, queenValue, gameState) + GetBishopSquareControlScore(row, column, queenValue, gameState);
    }

    public static float GetKingSquareControlScore(int row, int column, GameState gameState) {
        float kingScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        int noPieces = gameState.noWhitePieces + gameState.noBlackPieces;
        int enemyKingRow = pieceOwner == 'w' ? gameState.blackKingRow : gameState.whiteKingRow;
        int enemyKingColumn = pieceOwner == 'w' ? gameState.blackKingColumn : gameState.whiteKingColumn;

        int[] smallSquareXIncrements = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] smallSquareYIncrements = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };

        float kingMobilityValue = (float)(ENDGAME_TRANSITION - noPieces) / PUNISH_REWARD_FACTOR;

        for (int idx = 0; idx < 8; ++idx) {
            int newRow = row + smallSquareYIncrements[idx];
            int newColumn = column + smallSquareXIncrements[idx];
            if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8) {
                // king mobility is punished at the start of the game and is slowly rewarded as more pieces are traded
                kingScore += SQUARE_CONTROL_BONUS * kingMobilityValue;
                kingScore += math.pow(Math.Max(0, 2 - GetDistanceFromCenter(newRow, newColumn)), 2) * CENTER_CONTROL_BONUS * kingMobilityValue;
                kingScore += Math.Max(0, 2 - GetDistanceFromKing(newRow, newColumn, enemyKingRow, enemyKingColumn)) * ENEMY_KING_CONTROL_BONUS;
                char pieceChar = boardConfiguration[newRow, newColumn];
                int pieceIndex = GetPieceIndex(pieceChar);
                if (pieceIndex != -1) {
                    kingScore += SQUARE_DEFEND_ATTACK_KING * kingMobilityValue;
                }
            }
        }
        return kingScore;
    }

    public static int GetDistanceFromCenter(int row, int column) {
        return Math.Min(Math.Abs(row - 3), Math.Abs(row - 4)) + Math.Min(Math.Abs(column - 3), Math.Abs(column - 4));
    }

    public static int GetDistanceFromKing(int row, int column, int kingRow, int kingColumn) {
        return Math.Abs(row - kingRow) + Math.Abs(column - kingColumn);
    }

    public static int GetPieceIndex(char pieceChar) {
        if (pieceChar == '-') {
            return -1;
        }
        return char.ToLower(pieceChar) switch {
            'p' => 0,
            'n' => 1,
            'b' => 2,
            'r' => 3,
            'q' => 4,
            'k' => 5,
            _ => -1,
        };
    }

    public static void OrderMoves(List<IndexMove> legalMoves, GameState gameState) {
        List<Tuple<IndexMove, int>> movesWithScore = new();
        foreach (IndexMove move in legalMoves) {
            int movescore = 0;
            int capturedPieceIndex = GetPieceIndex(gameState.boardConfiguration[move.newRow, move.newColumn]);
            int movingPieceIndex = GetPieceIndex(gameState.boardConfiguration[move.oldRow, move.oldColumn]);

            if (capturedPieceIndex != -1 && capturedPieceIndex > movingPieceIndex) {
                // value of captured piece relative to value of moving piece
                movescore += pieceValues[capturedPieceIndex] - pieceValues[movingPieceIndex];
            }
            if (movingPieceIndex > 0) { // higher value piece attacked by a pawn
                if (gameState.whoMoves == 'w') {
                    if (move.newRow - 1 > 0) {
                        if (move.newColumn - 1 >= 0) {
                            if (GetPieceIndex(gameState.boardConfiguration[move.newRow - 1, move.newColumn - 1]) == 0) {
                                movescore -= pieceValues[movingPieceIndex];
                            }
                        }
                        if (move.newColumn + 1 < 8) {
                            if (GetPieceIndex(gameState.boardConfiguration[move.newRow - 1, move.newColumn + 1]) == 0) {
                                movescore -= pieceValues[movingPieceIndex];
                            }
                        }
                    }
                } else {
                    if (move.newRow + 1 < 7) {
                        if (move.newColumn - 1 >= 0) {
                            if (GetPieceIndex(gameState.boardConfiguration[move.newRow + 1, move.newColumn - 1]) == 0) {
                                movescore -= pieceValues[movingPieceIndex];
                            }
                        }
                        if (move.newColumn + 1 < 8) {
                            if (GetPieceIndex(gameState.boardConfiguration[move.newRow + 1, move.newColumn + 1]) == 0) {
                                movescore -= pieceValues[movingPieceIndex];
                            }
                        }
                    }
                }
            }
            if (movingPieceIndex == 0 && move.newRow == 0 || move.newRow == 7) {
                movescore += pieceValues[4];
            }
            movesWithScore.Add(new Tuple<IndexMove, int>(move, movescore));
        }
        // sort descending by move score
        movesWithScore.Sort((x, y) => y.Item2.CompareTo(x.Item2));
        legalMoves.Clear();
        // add the moves back to the original list
        foreach (Tuple<IndexMove, int> moveWithScore in movesWithScore) {
            legalMoves.Add(moveWithScore.Item1);
        }
    }

    public static MoveEval GetBestMove(GameState gameState, int maxLevel, MoveEval mandatoryMove = null,
        MoveEval prevBestMove = null, Hashtable gameStates = null) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        OrderMoves(legalMoves, gameState);
        if (prevBestMove != null) {
            int index = legalMoves.IndexOf(prevBestMove.move);
            // put the previously best move first (or second if we also have a mandatory move) to maximize pruning
            legalMoves.RemoveAt(index);
            legalMoves.Insert(0, prevBestMove.move);
        }
        if (mandatoryMove != null) {
            int index = legalMoves.IndexOf(mandatoryMove.move);
            // put the mandatory move first so its branch is not pruned
            legalMoves.RemoveAt(index);
            legalMoves.Insert(0, mandatoryMove.move);
        }
        MoveEval bestMoveEval = new() {
            score = gameState.whoMoves == 'w' ? -10000f : 10000f
        };
        maximumDepth = maxLevel;
        float alpha = -10000f, beta = 10000f;
        for (int i = 0; i < legalMoves.Count; ++i) {
            IndexMove move = legalMoves[i];
           if (!Game.Instance.timeNotExpired) {
                break; // only got a partial result for this iteration
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, 1, alpha, beta, gameStates);
            gameState.UnmakeMoveNoHashtable(move);
            if (score == 10000) {
                break; // time expired down the branch, we can't consider this move
            }
            if (gameState.whoMoves == 'w') {
                if (score > bestMoveEval.score) {
                    bestMoveEval.score = score;
                    bestMoveEval.move = move;
                }
                alpha = Math.Max(alpha, score);
            } else {
                if (score < bestMoveEval.score) {
                    bestMoveEval.score = score;
                    bestMoveEval.move = move;
                }
                beta = Math.Min(beta, score);
            }
            if (i == 0 && mandatoryMove != null) { // evaluation for our mandatory move
                mandatoryMove.score = score;
            }
            // prune the branch
            if (beta <= alpha) {
                break;
            }
        }
        return bestMoveEval;
    }

    public static float MiniMax(GameState gameState, int depth, float alpha, float beta, Hashtable gameStates = null) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        OrderMoves(legalMoves, gameState);
        if (depth == maximumDepth) {
            return PositionEvaluator(gameState, depth, legalMoves);
        }
        GameConclusion conclusion = GameStateManager.Instance.GetDrawConclusion(gameState);
        if (conclusion == GameConclusion.DrawByInsufficientMaterial || conclusion == GameConclusion.DrawBy50MoveRule) {
            return 0;
        }
        conclusion = GameStateManager.Instance.GetMateConclusion(gameState, legalMoves);
        if (conclusion == GameConclusion.Checkmate) {
            if (gameState.whoMoves == 'w') {
                return -1000f + depth;
            }
            return 1000f - depth;
        }
        if (conclusion == GameConclusion.Stalemate) {
            return 0;
        }
        if (gameStates != null) {
            if (gameStates.ContainsKey(gameState)) {
                // making this move may cause a 3-fold repetition
                // if we're winning we don't mind waiting until the last possible moment
                // if we're losing we want to "believe" this can be a draw
                return 0;
            }
        }

        // we have at least one legal move
        float bestScore = gameState.whoMoves == 'w' ? -10000f : 10000f;

        foreach (IndexMove move in legalMoves) {
            if (!Game.Instance.timeNotExpired) {
                // this is where propagating 10000 to the top begins so we know time expired on this branch
                return 10000;
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, depth + 1, alpha, beta, gameStates);
            gameState.UnmakeMoveNoHashtable(move);
            if (score == 10000) {
                return score; // time expired down the branch, propagate 10000 to the top
            }
            if (gameState.whoMoves == 'w') {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, score);
            } else {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, score);
            }
            // prune the branch
            if (beta <= alpha) {
                break;
            }
        }
        return bestScore;
    }
}
