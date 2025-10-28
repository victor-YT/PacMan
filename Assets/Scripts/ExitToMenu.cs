using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToMenu : MonoBehaviour
{
    public void GoToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}