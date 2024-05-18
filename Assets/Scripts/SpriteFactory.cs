using UnityEngine;

public class SpriteFactory : MonoBehaviour
{

    public Sprite black_queen, black_king, black_pawn, black_bishop, black_knight, black_rook, 
        white_queen, white_king, white_pawn, white_bishop, white_knight, white_rook;

    
    public Sprite GetSprite(string name) {

        return name switch
        {
            "black_queen" => black_queen,
            "black_king" => black_king,
            "black_rook" => black_rook,
            "black_bishop" => black_bishop,
            "black_knight" => black_knight,
            "black_pawn" => black_pawn,
            "white_queen" => white_queen,
            "white_king" => white_king,
            "white_rook" => white_rook,
            "white_bishop" => white_bishop,
            "white_knight" => white_knight,
            "white_pawn" => white_pawn,
            _ => null,
        };
    }
}
