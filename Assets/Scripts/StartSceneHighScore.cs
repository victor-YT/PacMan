// Assets/Scripts/StartSceneHighScore.cs
using UnityEngine;
using TMPro;

public class StartSceneHighScore : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text bestTimeText;

    void Awake()
    {
        int bestScore = PlayerPrefs.GetInt(PrefKeys.BestScore, 0);
        float bestTime = PlayerPrefs.GetFloat(PrefKeys.BestTime, 0f);

        if (bestScoreText) bestScoreText.text = $"Best Score: {bestScore.ToString("D6")}";
        if (bestTimeText)  bestTimeText.text  = $"Best Time:  {FormatTime(bestTime)}";

        Debug.Log($"Loaded Best: score={bestScore}, time={bestTime:0.00}");
    }

    string FormatTime(float t)
    {
        if (t <= 0f) return "00:00:00";
        int cs = Mathf.FloorToInt(t * 100f);
        int mm = cs / 6000;
        int ss = (cs / 100) % 60;
        int cc = cs % 100;
        return $"{mm:00}:{ss:00}:{cc:00}";
    }
}