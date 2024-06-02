public class Move {
    public char oldFile, newFile;
    public int oldRank, newRank;
    public char promotesInto;

    public Move(char oldFile, int oldRank, char newFile, int newRank, char promotesInto = '-') {
        this.oldFile = oldFile;
        this.oldRank = oldRank;
        this.newFile = newFile;
        this.newRank = newRank;
        this.promotesInto = promotesInto;
    }
    public Move(IndexMove indexMove) {
        oldRank = 8 - indexMove.oldRow;
        newRank = 8 - indexMove.newRow;
        oldFile = (char)(indexMove.oldColumn + 'a');
        newFile = (char)(indexMove.newColumn + 'a');
        promotesInto = indexMove.promotesInto;
    }
    public override string ToString() {
        return oldFile.ToString() + oldRank.ToString() + newFile + newRank + (promotesInto != '-' ? promotesInto : "");
    }
}
