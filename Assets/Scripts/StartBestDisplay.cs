using UnityEngine;
using TMPro;

public class StartBestDisplay : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text bestTimeText;

    const string PP_BEST_SCORE = "BEST_SCORE";
    const string PP_BEST_TIME  = "BEST_TIME";

    void Start()
    {
        int s = PlayerPrefs.GetInt(PP_BEST_SCORE, 0);
        float t = PlayerPrefs.GetFloat(PP_BEST_TIME, 0f);
        if (bestScoreText) bestScoreText.text = s.ToString("D6");
        if (bestTimeText)
        {
            int cs = Mathf.Max(0, Mathf.FloorToInt(t * 100f));
            int mm = cs / 6000;
            int ss = (cs / 100) % 60;
            int cc = cs % 100;
            bestTimeText.text = $"{mm:00}:{ss:00}:{cc:00}";
        }
    }
}