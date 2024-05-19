using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    private char file;
    private int rank;
    private float global_x, global_y;
    private static readonly float offset = 3.5f; // -8 / 2 + 1 / 2
    private static readonly float multiplier = 1; // 8 / 8
    public void SetGlobalCoords()
    {
        global_x = file - 'a';
        global_y = rank - 1;

        global_x *= multiplier;
        global_y *= multiplier;

        global_x -= offset;
        global_y -= offset;

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
