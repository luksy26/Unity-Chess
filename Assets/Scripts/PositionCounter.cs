using System.Collections.Generic;
using static MoveGenerator;
using UnityEngine;
using System.IO;

public static class PositionCounter
{   public static int maxDepth;
    public static int SearchPositions(GameState gameState, int depth, StreamWriter writer = null) {
        if (depth == maxDepth) {
            return 1;
        }
        int sum = 0;
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        foreach (IndexMove move in legalMoves) {
            GameState newGameState = new(gameState);
            newGameState.MovePiece(move);
            int numberPositions = SearchPositions(newGameState, depth + 1, writer);
            // if (depth == maxDepth - 1) {
            //     Debug.Log(new Move(move) + ": " + numberPositions);
            //     writer.WriteLine(new Move(move) + ": " + numberPositions);
            // }
            sum += numberPositions;

        }
        return sum;
    }
}
