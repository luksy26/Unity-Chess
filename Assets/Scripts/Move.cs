public class Move {
    public char oldFile, newFile;
    public int oldRank, newRank;

    public Move(char oldFile, int oldRank, char newFile, int newRank) {
        this.oldFile = oldFile;
        this.oldRank = oldRank;
        this.newFile = newFile;
        this.newRank = newRank;
    }
    public Move(IndexMove indexMove) {
        oldRank = 8 - indexMove.oldRow;
        newRank = 8 - indexMove.newRow;
        oldFile = (char)(indexMove.oldColumn + 'a');
        newFile = (char)(indexMove.newColumn + 'a');
    }
    public override string ToString() {
        return oldFile.ToString() + oldRank.ToString() + newFile + newRank;
    }
}
