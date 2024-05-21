public class GameState {
    public char[,] boardConfiguration;
    public char whoMoves;
    public int moveCounter50Move;
    public int moveCounterFull;
    public char enPassantFile;
    public int enPassantRank;
    public int blackKingRow, blackKingColumn, whiteKingRow, whiteKingColumn;
    public bool white_O_O, white_O_O_O, black_O_O, black_O_O_O;

}
