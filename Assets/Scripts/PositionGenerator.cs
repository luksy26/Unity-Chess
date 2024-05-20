using UnityEngine;
using UnityEngine.UI;

public class PositionGenerator : MonoBehaviour {
    public InputField inputField;
    public Button generateButton;

    void Start() {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
    }

    void OnGenerateButtonClicked() {
        string inputFEN = inputField.text;
        GameStateManager.Instance.GenerateGameState(inputFEN);
        Game.Instance.DestroyPieces();
        Game.Instance.GeneratePieces();
    }
}
