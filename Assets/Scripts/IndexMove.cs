using System;

public class IndexMove {
    public int oldRow, oldColumn, newRow, newColumn;
    public char promotesInto;

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
