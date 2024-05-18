using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    private char file;
    private int rank;
    public void SetGlobalCoords()
    {
        float x = file - 'a';
        float y = rank - 1;

        x *= 2 * 4.375f / 7;
        y *= 2 * 4.352f / 7;

        x += -4.375f;
        y += -4.352f;

        transform.position = new Vector3(x, y, -0.01f);
    }

    public int GetFile()
    {
        return file - 'a';
    }
    public int GetRank()
    {
        return rank - 1;
    }
    public void SetFile(char file)
    {
        this.file = file;
    }
    public void SetRank(int rank)
    {
        this.rank = rank;
    }
}
