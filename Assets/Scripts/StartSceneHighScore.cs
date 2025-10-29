using TMPro;
using UnityEngine;

public class StartSceneHighScore : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text bestTimeText;

    const string PP_BEST_SCORE = "BEST_SCORE";
    const string PP_BEST_TIME  = "BEST_TIME";

    void Start()
    {
        int s = PlayerPrefs.GetInt(PP_BEST_SCORE, 0);
        float t = PlayerPrefs.GetFloat(PP_BEST_TIME, 0f);

        if (bestScoreText) bestScoreText.text = s.ToString("000000");

        int m = Mathf.FloorToInt(t / 60f);
        int ss = Mathf.FloorToInt(t % 60f);
        int f = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);
        if (bestTimeText) bestTimeText.text = $"{m:00}:{ss:00}:{f:00}";
    }
}