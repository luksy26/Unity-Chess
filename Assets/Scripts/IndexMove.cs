using System;

public class IndexMove {
    public int oldRow, oldColumn, newRow, newColumn;
    public char promotesInto;
    public char oldSquareState; // what was previously on the square we moved to
    public char oldIntermediarySquareState1, oldIntermediarySquareState2; // same as above, used for en-passant and castling
    public int intermediaryRow1, intermediaryColumn1, intermediaryRow2, intermediaryColumn2; // the rows and columns for the above squares
    public int oldMoveCounter50Move;
    public int oldMoveCounterFull;
    public char oldEnPassantFile;
    public int oldEnPassantRank;
    public bool kingMoved; // indicates whether we moved the king
    public int oldKingRow, oldKingColumn; // previous position of the king
    public bool oldCanWhite_O_O, oldCanWhite_O_O_O, oldCanBlack_O_O, oldCanBlack_O_O_O; // previous castling rights

    public IndexMove(Move move) {
        oldRow = 8 - move.oldRank;
        newRow = 8 - move.newRank;
        oldColumn = move.oldFile - 'a';
        newColumn = move.newFile - 'a';
        promotesInto = move.promotesInto;
    }
    public IndexMove(int oldRow, int oldColumn, int newRow, int newColumn, char promotesInto = '-') {
        this.oldRow = oldRow;
        this.oldColumn = oldColumn;
        this.newRow = newRow;
        this.newColumn = newColumn;
        this.promotesInto = promotesInto;
    }
    public IndexMove(IndexMove indexMove) {
        oldRow = indexMove.oldRow;
        newRow = indexMove.newRow;
        oldColumn = indexMove.oldColumn;
        newColumn = indexMove.newColumn;
        promotesInto = indexMove.promotesInto;
    }
}
