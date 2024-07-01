using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuitGame() {
        Application.Quit();
    }
    public void LoadDevCorner() {
        SelectedOption.AI = -1;
        SelectedOption.tutorial = -1;
        SceneManager.LoadScene("BoardScene");
    }
}
