using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text scaredTimerText;
    public TMP_Text levelNameText;

    [Header("Lives")]
    public Transform livesRoot;
    public Image[] lifeIcons;

    [Header("Overlays")]
    public CanvasGroup countdownGroup;
    public TMP_Text countdownLabel;
    public CanvasGroup gameOverGroup;

    [Header("Buttons")]
    public Button exitButton;

    void Awake()
    {
        SetScore(0);
        SetTimer(0f);
        ShowScaredTimer(false);
        if (levelNameText) levelNameText.text = "Level 1";
        ShowCountdown(false);
        ShowGameOver(false);
        if (exitButton) exitButton.onClick.AddListener(ExitToStart);
    }

    public void SetScore(int v)
    {
        if (scoreText) scoreText.text = v.ToString("D6");
    }

    public void SetTimer(float t)
    {
        if (!timerText) return;
        int cs = Mathf.Max(0, Mathf.FloorToInt(t * 100f));
        int mm = cs / 6000;
        int ss = (cs / 100) % 60;
        int cc = cs % 100;
        timerText.text = $"{mm:00}:{ss:00}:{cc:00}";
    }

    // NEW: overload to match GameManager.ShowScaredTimer(true, secondsLeft)
    public void ShowScaredTimer(bool show, int secondsLeft)
    {
        if (!scaredTimerText) return;
        scaredTimerText.gameObject.SetActive(show && secondsLeft > 0);
        if (show && secondsLeft > 0)
            scaredTimerText.text = secondsLeft.ToString();
    }

    // NEW: overload to match GameManager.ShowScaredTimer(false)
    public void ShowScaredTimer(bool show)
    {
        if (!scaredTimerText) return;
        scaredTimerText.gameObject.SetActive(show);
    }

    // still keep the simple setter if you want to call with just seconds
    public void ShowScaredTimer(int secondsLeft)
    {
        ShowScaredTimer(secondsLeft > 0, secondsLeft);
    }

    public void SetLives(int lives)
    {
        if (lifeIcons != null && lifeIcons.Length > 0)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
                if (lifeIcons[i]) lifeIcons[i].gameObject.SetActive(i < lives);
            return;
        }

        if (!livesRoot) return;
        for (int i = 0; i < livesRoot.childCount; i++)
            livesRoot.GetChild(i).gameObject.SetActive(i < lives);
    }

    public void SetLevelName(string nameStr)
    {
        if (levelNameText) levelNameText.text = nameStr;
    }

    public void ShowCountdown(bool show, string label = "")
    {
        if (!countdownGroup) return;
        countdownGroup.alpha = show ? 1f : 0f;
        countdownGroup.blocksRaycasts = show;
        countdownGroup.interactable = show;
        if (countdownLabel) countdownLabel.text = label;
    }

    public void ShowGameOver(bool show)
    {
        if (!gameOverGroup) return;
        gameOverGroup.alpha = show ? 1f : 0f;
        gameOverGroup.blocksRaycasts = show;
        gameOverGroup.interactable = show;
    }

    public void ExitToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}