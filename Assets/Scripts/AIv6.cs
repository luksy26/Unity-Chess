using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

/*
    minimax with alpha beta, partial search at max depth, evaluation function piece square tables,
    3fold detection, transposition table, move ordering
*/
public static class AIv6 {
    public const int MOVE_FIRST_ADVANTAGE = 20;
    public const int ENDGAME_TRANSITION = 6;
    static readonly int[] pieceValues = { 100, 320, 330, 500, 900, 0 };
    public static Hashtable transpositionTable = new();
    public static int tableHits;

    static readonly int[,] pawnST = {
        {0,  0,  0,  0,  0,  0,  0,  0},
        {50, 50, 50, 50, 50, 50, 50, 50},
        {10, 10, 20, 30, 30, 20, 10, 10},
        {5,  5, 10, 25, 25, 10,  5,  5},
        {0,  0,  0, 20, 20,  0,  0,  0},
        {5, -5,-10,  0,  0,-10, -5,  5},
        {5, 10, 10,-20,-20, 10, 10,  5},
        {0,  0,  0,  0,  0,  0,  0,  0}
    };

    static readonly int[,] knightST = {
        {-50,-40,-30,-30,-30,-30,-40, -50},
        {-40,-20,  0,  0,  0,  0,-20, -40},
        {-30,  0, 10, 15, 15, 10,  0, -30},
        {-30,  5, 15, 20, 20, 15,  5, -30},
        {-30,  0, 15, 20, 20, 15,  0, -30},
        {-30,  5, 10, 15, 15, 10,  5, -30},
        {-40,-20,  0,  5,  5,  0,-20, -40},
        {-50,-40,-30,-30,-30,-30,-40, -50}
    };

    static readonly int[,] bishopST = {
        {-20,-10,-10,-10,-10,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5, 10, 10,  5,  0,-10},
        {-10,  5,  5, 10, 10,  5,  5,-10},
        {-10,  0, 10, 10, 10, 10,  0,-10,},
        {-10, 10, 10, 10, 10, 10, 10,-10},
        {-10,  5,  0,  0,  0,  0,  5,-10},
        {-20,-10,-10,-10,-10,-10,-10,-20}
    };

    static readonly int[,] rookST = {
        {0,  0,  0,  0,  0,  0,  0,  0,},
        {5, 10, 10, 10, 10, 10, 10,  5 },
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        { -5, 0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {-5,  0,  0,  0,  0,  0,  0, -5},
        {0,  0,  0,  5,  5,  0,  0,  0 }
    };

    static readonly int[,] queenST = {
        {-20,-10,-10, -5, -5,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5,  5,  5,  5,  0,-10},
        {-5,  0,  5,  5,  5,  5,  0, -5},
        { 0,  0,  5,  5,  5,  5,  0, -5},
        {-10,  5,  5,  5,  5,  5,  0,-10},
        {-10,  0,  5,  0,  0,  0,  0,-10},
        {-20,-10,-10, -5, -5,-10,-10,-20}
    };

    static readonly int[,] kingOpeningST = {
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-20,-30,-30,-40,-40,-30,-30,-20},
        {-10,-20,-20,-20,-20,-20,-20,-10},
        { 20, 20,  0,  0,  0,  0, 20, 20},
        { 20, 30, 10,  0,  0, 10, 30, 20}
    };

    static readonly int[,] kingEndgameST = {
        {-50,-40,-30,-20,-20,-30,-40,-50},
        {-30,-20,-10,  0,  0,-10,-20,-30},
        {-30,-10, 20, 30, 30, 20,-10,-30},
        {-30,-10, 30, 40, 40, 30,-10,-30},
        {-30,-10, 30, 40, 40, 30,-10,-30},
        {-30,-10, 20, 30, 30, 20,-10,-30},
        {-30,-30,  0,  0,  0,  0,-30,-30},
        {-50,-30,-30,-30,-30,-30,-30,-50}
    };

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
        char owner;
        if (pieceIndex != -1 && char.IsUpper(gameState.boardConfiguration[row, column])) {
            owner = 'w';
        } else {
            owner = 'b';
        }
        return pieceIndex switch {
            0 => GetPawnSquareControlScore(row, column, owner),
            1 => GetKnightSquareControlScore(row, column, owner),
            2 => GetBishopSquareControlScore(row, column, owner),
            3 => GetRookSquareControlScore(row, column, owner),
            4 => GetQueenSquareControlScore(row, column, owner),
            5 => GetKingSquareControlScore(row, column, owner, gameState),
            _ => 0,
        };
    }

    public static float GetPawnSquareControlScore(int row, int column, char owner) {
        if (owner == 'w') {
            return pawnST[row, column];
        }
        return pawnST[7 - row, 7 - column];
    }

    public static float GetKnightSquareControlScore(int row, int column, char owner) {
        if (owner == 'w') {
            return knightST[row, column];
        }
        return knightST[7 - row, 7 - column];
    }

    public static float GetBishopSquareControlScore(int row, int column, char owner) {
        if (owner == 'w') {
            return bishopST[row, column];
        }
        return bishopST[7 - row, 7 - column];
    }

    public static float GetRookSquareControlScore(int row, int column, char owner) {
        if (owner == 'w') {
            return rookST[row, column];
        }
        return rookST[7 - row, 7 - column];
    }

    public static float GetQueenSquareControlScore(int row, int column, char owner) {
        if (owner == 'w') {
            return queenST[row, column];
        }
        return queenST[7 - row, 7 - column];
    }

    public static float GetKingSquareControlScore(int row, int column, char owner, GameState gameState) {
        int piecesLeft = 32 - gameState.noBlackPieces - gameState.noWhitePieces;
        float endgameStage = 1.0f * Math.Max(0, piecesLeft - ENDGAME_TRANSITION) / (32 - ENDGAME_TRANSITION);
        if (owner == 'w') {
            return kingOpeningST[row, column] * (1 - endgameStage) + kingEndgameST[row, column] * endgameStage;
        }
        return kingOpeningST[7 - row, 7 - column] * (1 - endgameStage) + kingEndgameST[7 - row, 7 - column] * endgameStage;
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
        transpositionTable.Clear();
        tableHits = 0;
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
                break;
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, 1, alpha, beta, gameStates);
            gameState.UnmakeMoveNoHashtable(move);
            if (Math.Abs(score) == 10000) {
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
        int hashcode = gameState.GetHashCodeTranspo();
        if (transpositionTable.ContainsKey(hashcode)) {
            ++tableHits;
            return (float)transpositionTable[hashcode];
        }
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        OrderMoves(legalMoves, gameState);
        if (depth == maximumDepth) {
            float score = PositionEvaluator(gameState, depth, legalMoves);
            transpositionTable.Add(hashcode, score);
            return score;
        }
        GameConclusion conclusion = GameStateManager.Instance.GetDrawConclusion(gameState, gameStates);
        if (conclusion == GameConclusion.DrawByInsufficientMaterial || conclusion == GameConclusion.DrawBy50MoveRule) {
            transpositionTable.Add(hashcode, 0f);
            return 0;
        }
        conclusion = GameStateManager.Instance.GetMateConclusion(gameState, legalMoves);
        if (conclusion == GameConclusion.Checkmate) {
            if (gameState.whoMoves == 'w') {
                transpositionTable.Add(hashcode, -1000f + depth);
                return -1000f + depth;
            }
            transpositionTable.Add(hashcode, 1000f - depth);
            return 1000f - depth;
            /*  
                We don't add mates to the tranposition table since we won't be able to tell the mate value, since
                there's no distinction to return values from MiniMax that are newly added or taken from the transposition table.
            */
        }
        if (conclusion == GameConclusion.Stalemate) {
            transpositionTable.Add(hashcode, 0f);
            return 0;
        }
        if (gameStates != null) {
            if (gameStates.ContainsKey(gameState)) {
                // making this move may cause a 3-fold repetition
                // if we're winning we don't mind waiting until the last possible moment
                // if we're losing we want to "believe" this can be a draw
                transpositionTable.Add(hashcode, 0f);
                return 0;
            }
        }

        // we have at least one legal move
        float bestScore = gameState.whoMoves == 'w' ? -10000f : 10000f;

        foreach (IndexMove move in legalMoves) {
            if (!Game.Instance.timeNotExpired) {
                // propagate 10000 to the top so we know time expired on this branch
                if (gameState.whoMoves == 'w') {
                    return -10000;
                } else {
                    return 10000;
                }
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, depth + 1, alpha, beta);
            gameState.UnmakeMoveNoHashtable(move);
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
        // We only add leaf node mates to the tranposition table ,since they are more likely to be transposed into
        if (Math.Abs(bestScore) < 950) {
            transpositionTable.Add(hashcode, bestScore);
        }
        return bestScore;
    }

    public static char ColumnToFile(int j) {
        return (char)(j + 'a');
    }
    public static int RowToRank(int i) {
        return 8 - i;
    }
}
