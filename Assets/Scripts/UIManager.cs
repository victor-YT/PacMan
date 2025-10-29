using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text L1BestScore;
    public TMP_Text L1BestTime;

    void OnEnable()
    {
        Apply();
    }

    void Apply()
    {
        int bestScore = PlayerPrefs.GetInt("BEST_SCORE", 0);
        float bestTime = PlayerPrefs.GetFloat("BEST_TIME", 0f);

        string scoreStr = bestScore.ToString("D6");
        string timeStr  = FormatTime(bestTime);

        if (L1BestScore) L1BestScore.text = $"Best Score: {scoreStr}";
        if (L1BestTime)  L1BestTime.text  = $"Best Time:  {timeStr}";
    }

    static string FormatTime(float t)
    {
        int cs = Mathf.Max(0, Mathf.FloorToInt(t * 100f)); // centiseconds
        int mm = cs / 6000;
        int ss = (cs / 100) % 60;
        int cc = cs % 100;
        return $"{mm:00}:{ss:00}:{cc:00}";
    }
}