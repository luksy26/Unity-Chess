using UnityEngine;
using UnityEngine.UI;

public class GeneratePosition : MonoBehaviour
{
    public InputField inputField;
    public Button generateButton;

    void Start()
    {
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
    }

    void OnGenerateButtonClicked()
    {
        string inputFEN = inputField.text;
        InitialBoardConfiguration.Instance.GenerateboardConfiguration(inputFEN);
        Game.Instance.DestroyPieces();
        Game.Instance.GeneratePieces();
    }
}
