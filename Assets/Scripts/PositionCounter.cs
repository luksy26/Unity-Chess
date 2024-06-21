using System.Collections.Generic;
using static MoveGenerator;
using System.IO;
using System.Diagnostics;

public static class PositionCounter {
    public static int maxDepth;
    public static long SearchPositions(GameState gameState, int depth, StreamWriter writer = null) {
        if (depth == maxDepth) {
            return 1;
        }
        long sum = 0;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        stopwatch.Stop();
        GameStateManager.Instance.numberOfTicks1 += stopwatch.ElapsedTicks;
        foreach (IndexMove move in legalMoves) {
            gameState.MakeMoveNoHashtable(move);
            long numberPositions = SearchPositions(gameState, depth + 1, writer);
            if (depth == 0 && maxDepth == 1) {
                writer?.WriteLine(new Move(move) + ": " + numberPositions);
            }
            sum += numberPositions;
            gameState.UnmakeMoveNoHashtable(move);
        }
        return sum;
    }
}
