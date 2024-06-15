using System.Collections.Generic;
using static MoveGenerator;

public class MoveEval {
    public IndexMove move;
    public float score;
}
public static class AIv2 {
    public const int MOVE_FIRST_ADVANTAGE = 20;
    public const int SQUARE_CONTROL_BONUS = 1;
    public const int SQUARE_DEFEND_ATTACK_BONUS = 10;
    public const int SQUARE_DEFEND_ATTACK_EQUAL_BONUS = 5;
    public const int SQUARE_DEFEND_ATTACK_HIGHER_BONUS = 15;
    public const int SQUARE_DEFEND_ATTACK_KING = 10;
    public const int PAWN_CHAIN_BONUS = 15;
    public const int ENDGAME_TRANSITION = 10;
    public const int PUNISH_REWARD_FACTOR = 5;
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

        int forwardY = pieceOwner == 'w' ? -1 : 1;
        char friendlyPawn = pieceOwner == 'w' ? 'P' : 'p';
        char enemyKing = pieceOwner == 'w' ? 'k' : 'K';

        if (column - 1 >= 0) {
            // pawn controls a square
            pawnScore += SQUARE_CONTROL_BONUS;
            // square controlled by the pawn
            int pieceIndex = GetPieceIndex(boardConfiguration[row + forwardY, column - 1]);
            char pieceChar;

            // attacking/defending the en-passant target
            if (pieceIndex == -1 &&
            RowToRank(row + forwardY) == gameState.enPassantRank && ColumnToFile(column - 1) == gameState.enPassantFile) {
                pieceIndex = 0;
                pieceChar = gameState.whoMoves == 'w' ? 'p' : 'P';
            } else {
                pieceChar = boardConfiguration[row + forwardY, column - 1];
            }
            // pawn is defending/attacking a piece (also en-passant target)
            if (pieceIndex != -1) {
                pawnScore += SQUARE_DEFEND_ATTACK_BONUS;
                if (pieceChar == friendlyPawn) {
                    pawnScore += PAWN_CHAIN_BONUS;
                } else if (pawnValue < pieceValues[pieceIndex] || pieceChar == enemyKing) {
                    pawnScore += SQUARE_DEFEND_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100;
                }
            }
        }
        if (column + 1 < 8) {
            // pawn controls a square
            pawnScore += SQUARE_CONTROL_BONUS;
            // square controlled by the pawn
            int pieceIndex = GetPieceIndex(boardConfiguration[row + forwardY, column + 1]);
            char pieceChar;

            // attacking/defending the en-passant target
            if (pieceIndex == -1 &&
            RowToRank(row + forwardY) == gameState.enPassantRank && ColumnToFile(column + 1) == gameState.enPassantFile) {
                pieceIndex = 0;
                pieceChar = gameState.whoMoves == 'w' ? 'p' : 'P';
            } else {
                pieceChar = boardConfiguration[row + forwardY, column + 1];
            }
            // pawn is defending/attacking a piece (also en-passant target)
            if (pieceIndex != -1) {
                pawnScore += SQUARE_DEFEND_ATTACK_BONUS;
                if (pieceChar == friendlyPawn) {
                    pawnScore += PAWN_CHAIN_BONUS;
                } else if (pawnValue < pieceValues[pieceIndex] || pieceChar == enemyKing) {
                    pawnScore += SQUARE_DEFEND_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - pawnValue) / 100f;
                }
            }
        }
        return pawnScore;
    }

    public static float GetKnightControlScore(int row, int column, int knightValue, GameState gameState) {
        float knightScore = 0;
        char[,] boardConfiguration = gameState.boardConfiguration;
        char pieceOwner = char.IsLower(boardConfiguration[row, column]) ? 'b' : 'w';
        char enemyKing = pieceOwner == 'w' ? 'k' : 'K';

        int[] LshapeXIncrements = new int[8] { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] LshapeYIncrements = new int[8] { -1, 1, -2, 2, -2, 2, -1, 1 };

        for (int idx = 0; idx < 8; ++idx) {
            int newRow = row + LshapeYIncrements[idx];
            int newColumn = column + LshapeXIncrements[idx];
            // in bounds
            if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8) {
                knightScore += SQUARE_CONTROL_BONUS;
                char pieceChar = boardConfiguration[newRow, newColumn];
                int pieceIndex = GetPieceIndex(pieceChar);
                if (pieceIndex != -1) {
                    if (knightValue == pieceValues[pieceIndex]) {
                        knightScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                    } else if (knightValue < pieceValues[pieceIndex] || pieceChar == enemyKing) {
                        knightScore += SQUARE_DEFEND_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - knightValue) / 100f;
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
        char enemyKing = pieceOwner == 'w' ? 'k' : 'K';

        int[,] diagonalXIncrements = new int[,] { { 1, -1 }, { -1, 1 } };
        int[,] diagonalYIncrements = new int[,] { { -1, 1 }, { -1, 1 } };

        for (int diagonal = 0; diagonal < 2; ++diagonal) {
            for (int quadrant = 0; quadrant < 2; ++quadrant) {
                for (int newRow = row + diagonalYIncrements[diagonal, quadrant], newColumn = column + diagonalXIncrements[diagonal, quadrant];
                    newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8;
                    newRow += diagonalYIncrements[diagonal, quadrant], newColumn += diagonalXIncrements[diagonal, quadrant]) {
                    bishopScore += SQUARE_CONTROL_BONUS;
                    char pieceChar = boardConfiguration[newRow, newColumn];
                    int pieceIndex = GetPieceIndex(pieceChar);
                    if (pieceIndex != -1) {
                        if (bishopValue == pieceValues[pieceIndex]) {
                            bishopScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                        } else if (bishopValue < pieceValues[pieceIndex] || pieceChar == enemyKing) {
                            bishopScore += SQUARE_DEFEND_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - bishopValue) / 100f;
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
        char enemyKing = pieceOwner == 'w' ? 'k' : 'K';

        int[,] lineXIncrements = new int[,] { { 1, -1 }, { 0, 0 } };
        int[,] lineYIncrements = new int[,] { { 0, 0 }, { -1, 1 } };

        for (int line = 0; line < 2; ++line) {
            for (int direction = 0; direction < 2; ++direction) {
                for (int newRow = row + lineYIncrements[line, direction], newColumn = column + lineXIncrements[line, direction];
                    newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8;
                    newRow += lineYIncrements[line, direction], newColumn += lineXIncrements[line, direction]) {
                    rookScore += SQUARE_CONTROL_BONUS;
                    char pieceChar = boardConfiguration[newRow, newColumn];
                    int pieceIndex = GetPieceIndex(pieceChar);
                    if (pieceIndex != -1) {
                        if (rookValue == pieceValues[pieceIndex]) {
                            rookScore += SQUARE_DEFEND_ATTACK_EQUAL_BONUS;
                        } else if (rookValue < pieceValues[pieceIndex] || pieceChar == enemyKing) {
                            rookScore += SQUARE_DEFEND_ATTACK_HIGHER_BONUS * (pieceValues[pieceIndex] - rookValue) / 100f;
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
        int noPieces = gameState.noWhitePieces + gameState.noBlackPieces;

        int[] smallSquareXIncrements = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] smallSquareYIncrements = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };

        float kingMobilityValue = (float)(ENDGAME_TRANSITION - noPieces) / PUNISH_REWARD_FACTOR;

        for (int idx = 0; idx < 8; ++idx) {
            int newRow = row + smallSquareYIncrements[idx];
            int newColumn = column + smallSquareXIncrements[idx];
            if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8) {
                // king mobility is punished at the start of the game and is slowly rewarded as more pieces are traded
                kingScore += SQUARE_CONTROL_BONUS * kingMobilityValue;
                char pieceChar = boardConfiguration[newRow, newColumn];
                int pieceIndex = GetPieceIndex(pieceChar);
                if (pieceIndex != -1) {
                    kingScore += SQUARE_DEFEND_ATTACK_KING * kingMobilityValue;
                }
            }
        }
        return kingScore;
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
    public static MoveEval GetBestMove(GameState gameState, int maxLevel, MoveEval mandatoryMove = null) {
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
                break;
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, 1, alpha, beta);
            gameState.UnmakeMoveNoHashtable(move);
            if (gameState.whoMoves == 'w') {
                if (score > bestMoveEval.score) {
                    bestMoveEval.score = score;
                    bestMoveEval.move = move;
                }
                alpha = System.Math.Max(alpha, score);
            } else {
                if (score < bestMoveEval.score) {
                    bestMoveEval.score = score;
                    bestMoveEval.move = move;
                }
                beta = System.Math.Min(beta, score);
            }
            if (i == 0 && mandatoryMove != null) {
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
                break;
            }
            gameState.MakeMoveNoHashtable(move);
            float score = MiniMax(gameState, depth + 1, alpha, beta);
            gameState.UnmakeMoveNoHashtable(move);
            if (gameState.whoMoves == 'w') {
                bestScore = System.Math.Max(bestScore, score);
                alpha = System.Math.Max(alpha, score);
            } else {
                bestScore = System.Math.Min(bestScore, score);
                beta = System.Math.Min(beta, score);
            }
            // prune the branch
            if (beta <= alpha) {
                break;
            }
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
