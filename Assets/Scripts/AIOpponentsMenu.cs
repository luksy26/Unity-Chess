using UnityEngine;
using UnityEngine.SceneManagement;

public class AIOpponentsMenu : MonoBehaviour
{
    public void ChangeScene() {
        SelectedOption.tutorial = -1;
        SceneManager.LoadScene("BoardScene");
    }
    public void LoadAI1() {
        SelectedOption.AI = 1;
        ChangeScene();
    }
    public void LoadAI2() {
        SelectedOption.AI = 2;
        ChangeScene();
    }
    public void LoadAI3() {
        SelectedOption.AI = 3;
        ChangeScene();
    }
    public void LoadAI4() {
        SelectedOption.AI = 4;
        ChangeScene();
    }
    public void LoadAI5() {
        SelectedOption.AI = 5;
        ChangeScene();
    }
    public void LoadAI6() {
        SelectedOption.AI = 6;
        ChangeScene();
    }
    public void LoadAI7() {
        SelectedOption.AI = 7;
        ChangeScene();
    }
}
