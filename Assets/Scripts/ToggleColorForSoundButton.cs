using UnityEngine;
using UnityEngine.UI;

public class ToggleColorForSoundButton : MonoBehaviour {
    public Button button;
    public Color normalColor = new(1.0f, 0.9804f, 0.7961f); // FFFACB
    public Color pressedColor = Color.gray;
    private bool isChecked = false;

    public void AddToggleListener() {
        button.onClick.AddListener(Toggle);
        UpdateButtonColor();
    }

    void Toggle() {
        isChecked = !isChecked;
        UpdateButtonColor();
    }

    void UpdateButtonColor() {
        if (isChecked) {
            button.GetComponent<Image>().color = pressedColor;
        } else {
            button.GetComponent<Image>().color = normalColor;
        }
    }
}
