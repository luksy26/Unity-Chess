using System;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;
public static class AIv1
{   
    public static int maximumDepth;
    static int[] pieceValues = {100, 300, 300, 500, 900};
    public struct MoveEval {
        public IndexMove move;
        public int score;
    }
    public static int PositionEvaluator(GameState gameState) {
        GameConclusion conclusion = GameStateManager.Instance.GetGameConclusion(gameState);
        if (conclusion == GameConclusion.Stalemate || conclusion == GameConclusion.DrawByInsufficientMaterial ||
        conclusion == GameConclusion.DrawBy50MoveRule) {
            return 0;
        }
        if (conclusion == GameConclusion.Checkmate) {
            if (gameState.whoMoves == 'w') {
                return -100000;
            }
            return 100000;
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
                    default : break;
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
        return scoreWhite - scoreBlack;
    }
    public static MoveEval GetBestMove(GameState gameState) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        MoveEval maxMoveEval = new() {
            score = -1000000,
        }, minMoveEval = new() {
            score = 1000000
        };
        maximumDepth = 3;
        foreach (IndexMove move in legalMoves) {
            gameState.MakeMove(move);
            int score = MiniMax(gameState, 1);
            gameState.UnmakeMove(move);
            if (gameState.whoMoves == 'w' && score > maxMoveEval.score) {
                maxMoveEval.score = score;
                maxMoveEval.move = move;
            }
            if (gameState.whoMoves == 'b' && score < minMoveEval.score) {
                minMoveEval.score = score;
                minMoveEval.move = move;
            }
        }
        if (gameState.whoMoves == 'w') {
            return maxMoveEval;
        }
        return minMoveEval;
    }

    public static int MiniMax(GameState gameState, int depth) {
        if (depth == maximumDepth) {
            return PositionEvaluator(gameState);
        }
        
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        int maxScore = -1000000, minScore = 1000000;

        foreach (IndexMove move in legalMoves) {
            gameState.MakeMove(move);
            int score = MiniMax(gameState, depth + 1);
            gameState.UnmakeMove(move);
            if (gameState.whoMoves == 'w' && score > maxScore) {
                maxScore = score;
            }
            if (gameState.whoMoves == 'b' && score < minScore) {
                minScore = score;
            }
        }
        if (gameState.whoMoves == 'w') {
            return maxScore;
        }
        return minScore;
    }
}
