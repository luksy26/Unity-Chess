using System.Collections.Generic;
using static MoveGenerator;
using System.IO;
using UnityEngine;

public static class PositionCounter {
    public static int maxDepth;
    public static int SearchPositions(GameState gameState, int depth, StreamWriter writer = null) {
        if (depth == maxDepth) {
            return 1;
        }
        int sum = 0;
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        foreach (IndexMove move in legalMoves) {
            gameState.MakeMove(move);
            int numberPositions = SearchPositions(gameState, depth + 1, writer);
            if (depth == 0 && writer != null) {
                writer.WriteLine(new Move(move) + ": " + numberPositions);
            }
            sum += numberPositions;
            gameState.UnmakeMove(move);
        }
        return sum;
    }
}
