using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PromotionManager : MonoBehaviour {
    public Canvas canvas; // canvas where buttons are generated
    public GameObject promotionPieceButton; // Button Prefab
    public Sprite[] buttonSprites; // Promotion pieces sprites
    private readonly List<GameObject> buttons = new(); // list of created buttons
    private TaskCompletionSource<string> tcs; // task to process which button was pressed
    private int positionYIncrement; // indicates whether we generate the buttons from the top or from the bottom of the screen
    private float firstButtonX, firstButtonY; // placement of the first button, depends on playerPerspective, currentPlayer and piece file

    public Task<string> GeneratePromotionMenu(string playerPerspective, char whoMoves, char file) {
        // values if playing from the perspective of the player who moves
        firstButtonX = -3.5f;
        firstButtonY = 3.5f;
        // if playing from the perspective of the player who moves
        if (playerPerspective[0] == whoMoves) {
            // buttons will generate top-down
            positionYIncrement = -1;
        } else {
            // buttons will generate bottom-up
            positionYIncrement = 1;
            firstButtonY *= -1; // position the first button at the bottom
        }
        // position the buttons on the corresponding file
        if (playerPerspective.Equals("white")) {
            // a is file 0, b is file 1 etc (in terms of coordinates)
            firstButtonX += file - 'a';
        } else {
            // h is file 0, g is file 1 etc (if playing from black's perspective)
            firstButtonX += 8 - (file - 'a');
        }
        // this Task will tell us which piece was chosen for promotion
        tcs = new TaskCompletionSource<string>();

        // generate the buttons and wait for user input
        StartCoroutine(GeneratePromotionMenuButtons(whoMoves));

        // return the pieceName string
        return tcs.Task;
    }

    public IEnumerator GeneratePromotionMenuButtons(char whoMoves) {
        for (int i = 0; i < 4; ++i) {
            // create the button GameObject
            GameObject button = Instantiate(promotionPieceButton, canvas.transform);

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            // position the button
            rectTransform.localPosition = new Vector3(firstButtonX, firstButtonY + i * positionYIncrement, -0.02f);
            Sprite pieceSprite;
            if (whoMoves == 'w') {
                pieceSprite = buttonSprites[i];
            } else { // black pieces sprites are at an offset of 4 in the list
                pieceSprite = buttonSprites[i + 4];
            }
            button.GetComponent<Image>().sprite = pieceSprite;
            // add behaviour when pressing the button
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => HandleButtonClick(index));
            button.name = "PromotionButton_" + buttonSprites[i].name;
            buttons.Add(button);
        }

        // wait until one of the buttons is pressed
        yield return new WaitUntil(() => tcs.Task.IsCompleted);

        // remove buttons from the screen
        foreach (GameObject button in buttons) {
            Destroy(button);
        }
        buttons.Clear();
    }

    public void HandleButtonClick(int index) {
        // store result in the task depending on which button was pressed
        string result = GetResultFromIndex(index);
        tcs.SetResult(result);
    }
    private string GetResultFromIndex(int index) {
        return index switch {
            0 => "queen",
            1 => "knight",
            2 => "rook",
            3 => "bishop",
            _ => "",
        };
    }
}
