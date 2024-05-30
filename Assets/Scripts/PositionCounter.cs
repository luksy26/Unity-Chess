using System.Collections.Generic;
using static MoveGenerator;

public static class PositionCounter
{   public static int maxDepth;
    public static int SearchPositions(GameState gameState, int depth) {
        if (depth == maxDepth) {
            return 1;
        }
        int sum = 0;
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        foreach (IndexMove move in legalMoves) {
            GameState newGameState = new(gameState);
            newGameState.MovePiece(move);
            sum += SearchPositions(newGameState, depth + 1);
        }
        return sum;
    }
}
