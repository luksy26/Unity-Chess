using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePlacer : MonoBehaviour
{
    public GameObject controller;
    public GameObject moveSquare;

    private char file;
    private int rank;

    private string playerToMove;

    public Sprite black_queen, black_king, black_pawn, black_bishop, black_knight, black_rook, 
        white_queen, white_king, white_pawn, white_bishop, white_knight, white_rook;

    
    public void Activate() {
        controller = GameObject.FindGameObjectWithTag("GameController");

        SetGlobalCoords();

        switch (name) {
            case "black_queen": GetComponent<SpriteRenderer>().sprite = black_queen; break;
            case "black_king": GetComponent<SpriteRenderer>().sprite = black_king; break;
            case "black_rook": GetComponent<SpriteRenderer>().sprite = black_rook; break;
            case "black_bishop": GetComponent<SpriteRenderer>().sprite = black_bishop; break;
            case "black_knight": GetComponent<SpriteRenderer>().sprite = black_knight; break;
            case "black_pawn": GetComponent<SpriteRenderer>().sprite = black_pawn; break;

            case "white_queen": GetComponent<SpriteRenderer>().sprite = white_queen; break;
            case "white_king": GetComponent<SpriteRenderer>().sprite = white_king; break;
            case "white_rook": GetComponent<SpriteRenderer>().sprite = white_rook; break;
            case "white_bishop": GetComponent<SpriteRenderer>().sprite = white_bishop; break;
            case "white_knight": GetComponent<SpriteRenderer>().sprite = white_knight; break;
            case "white_pawn": GetComponent<SpriteRenderer>().sprite = white_pawn; break;
        }
    }
    public void SetGlobalCoords() {
        float x = file - 'a';
        float y = rank - 1;

        x *= 2 * 4.375f / 7;
        y *= 2 * 4.352f / 7;

        x += -4.375f;
        y += -4.352f;

        this.transform.position = new Vector3(x, y, -0.01f);
    }

    public int GetFile() {
        return file - 'a';
    }
    public int GetRank() {
        return rank - 1;
    }
    public void SetFile(char file) {
        this.file = file;
    }
    public void SetRank(int rank) {
        this.rank = rank;
    }
}
