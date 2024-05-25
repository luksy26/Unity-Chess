public class IndexMove {
    public int oldRow, oldColumn, newRow, newColumn;

    public IndexMove(Move move) {
        oldRow = 8 - move.oldRank;
        newRow = 8 - move.newRank;
        oldColumn = move.oldFile - 'a';
        newColumn = move.newFile - 'a';
    }
}
