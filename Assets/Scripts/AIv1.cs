using System.Collections.Generic;
using static MoveGenerator;
public static class AIv1 {
    public static int maximumDepth;
    static readonly int[] pieceValues = { 100, 300, 300, 500, 900 };
    public struct MoveEval {
        public IndexMove move;
        public float score;
    }
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
    public static MoveEval GetBestMove(GameState gameState) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        MoveEval maxMoveEval = new() {
            score = -10000f,
        }, minMoveEval = new() {
            score = 10000f
        };
        maximumDepth = 3;
        foreach (IndexMove move in legalMoves) {
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, 1);
            gameState.UnmakeMoveNoHashtable(move);
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

    public static float MiniMax(GameState gameState, int depth) {
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
        float maxScore = -10000f, minScore = 10000f;

        foreach (IndexMove move in legalMoves) {
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, depth + 1);
            gameState.UnmakeMoveNoHashtable(move);
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
