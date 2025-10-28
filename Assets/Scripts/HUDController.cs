using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Lives")]
    public Transform livesRoot;
    public Image[] lifeIcons;

    [Header("Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scaredText; 
    public TextMeshProUGUI levelName;

    [Header("Buttons")]
    public Button exitButton;

    void Awake()
    {
        SetScore(0);
        SetTimer(0f);
        ShowScaredTimer(0);
        if (levelName) levelName.text = "Level 1";

        if (exitButton) exitButton.onClick.AddListener(ExitToStart);
    }

    public void SetScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString("D6");
    }

    public void SetTimer(float seconds)
    {
        if (!timerText) return;
        int totalCs = Mathf.Max(0, Mathf.FloorToInt(seconds * 100f));
        int mm = totalCs / 6000;
        int ss = (totalCs / 100) % 60;
        int cs = totalCs % 100;
        timerText.text = $"{mm:00}:{ss:00}:{cs:00}";
    }

    public void ShowScaredTimer(int secondsLeft)
    {
        if (!scaredText) return;
        if (secondsLeft <= 0)
        {
            scaredText.gameObject.SetActive(false);
        }
        else
        {
            scaredText.gameObject.SetActive(true);
            scaredText.text = secondsLeft.ToString();
        }
    }

    public void ExitToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}