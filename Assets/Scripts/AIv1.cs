using System;
using System.Collections.Generic;
using static MoveGenerator;
public static class AIv1
{
    public static Move GetBestMove(GameState gameState) {
        List<IndexMove> legalMoves = GetLegalMoves(gameState);
        Random random = new();
        return new Move(legalMoves[random.Next(legalMoves.Count)]);
    }
    
}
