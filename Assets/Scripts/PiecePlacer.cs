using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    private char file;
    private int rank;
    private float global_x, global_y;
    private static readonly float xOffset = 4.375f;
    private static readonly float yOffset = 4.352f;

    public void SetGlobalCoords()
    {
        global_x = file - 'a';
        global_y = rank - 1;

        global_x *= 2 * xOffset / 7;
        global_y *= 2 * yOffset / 7;

        global_x -= xOffset;
        global_y -= yOffset;

        transform.position = new Vector3(global_x, global_y, -0.01f);
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
