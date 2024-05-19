using UnityEngine;

public class MoveValidator : MonoBehaviour
{   
    private char[,] boardConfiguration;
    // Start is called before the first frame update

    public bool IsLegalMove(char old_file, int old_rank, char new_file, int new_rank) {
        boardConfiguration = InitialBoardConfiguration.Instance.boardConfiguration;

        return true;

    }
    private string GetPieceType(char x)
    {
        return x switch
        {
            'r' or 'R' => "rook",
            'n' or 'N' => "knight",
            'b' or 'B' => "bishop",
            'q' or 'Q' => "queen",
            'k' or 'K' => "king",
            'p' or 'P' => "pawn",
            _ => "",
        };
    }
}
