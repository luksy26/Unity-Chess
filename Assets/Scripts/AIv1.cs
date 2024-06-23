using System;
using System.Collections;
using System.Collections.Generic;
using static MoveGenerator;

// simple minimax with alpha beta, evaluation based on material count
public static class AIv1 {
    public static int maximumDepth;
    static readonly int[] pieceValues = { 100, 300, 300, 500, 900 };

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

        int scoreWhite = 0, scoreBlack = 0;
        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                char potentialPiece = gameState.boardConfiguration[i, j];
                int index = -1;
                switch (char.ToLower(potentialPiece)) {
                    case 'p': index = 0; break;
                    case 'n': index = 1; break;
                    case 'b': index = 2; break;
                    case 'r': index = 3; break;
                    case 'q': index = 4; break;
                    default: break;
                }
                int pieceValue = 0;
                if (index != -1) {
                    pieceValue = pieceValues[index];
                }
                if (char.IsLetter(potentialPiece)) {
                    if (char.IsUpper(potentialPiece)) {
                        scoreWhite += pieceValue;
                    } else {
                        scoreBlack += pieceValue;
                    }
                }
            }
        }
        if (gameState.whoMoves == 'w') {
            scoreWhite += 30;
        } else {
            scoreBlack += 30;
        }
        return (scoreWhite - scoreBlack) / 100f;
    }
    public static MoveEval GetBestMove(GameState gameState, int maxLevel, MoveEval mandatoryMove = null,
        MoveEval dummyPrevBestMove = null, Hashtable dummyGameStates = null) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
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
                bestMoveEval.score = 10000;
                return bestMoveEval; // time expired before evaluating all moves, disregard whole tree
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, 1, alpha, beta);
            gameState.UnmakeMoveNoHashtable(move);
            if (score == 10000) {
                bestMoveEval.score = score;
                return bestMoveEval; // time expired down the branch, disregard whole tree
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

    public static float MiniMax(GameState gameState, int depth, float alpha, float beta) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
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

        // we have at least one legal move
        float bestScore = gameState.whoMoves == 'w' ? -10000f : 10000f;

        foreach (IndexMove move in legalMoves) {
            if (!Game.Instance.timeNotExpired) {
                // this is where propagating 10000 to the top begins so we know time expired on this branch
                return 10000;
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, depth + 1, alpha, beta);
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
