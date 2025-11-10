using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Screen.fullScreen = true;
    }
    public void StartGame()
    {

        SceneManager.LoadScene("Map_v2");
    }
}
