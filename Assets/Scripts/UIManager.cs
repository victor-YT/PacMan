using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text L1BestScore;
    public TMP_Text L2BestScore;

    public TMP_Text L1BestTime;
    public TMP_Text L2BestTime;


    void Start()
    {
        L1BestScore.text = "Best Score: 00000";
        L1BestTime.text = "Best Time: 00:00";
        L2BestScore.text = "Best Score: 00000";
        L2BestTime.text = "Best Time: 00:00";
    }
}