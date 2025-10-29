using UnityEngine;

public class HeroPreviewCycler : MonoBehaviour
{
    public Animator anim;
    public string[] clips = { "hero_up_ui", "hero_right_ui", "hero_down_ui", "hero_left_ui" };
    public float interval = 1.2f;

    int i;
    float t;

    void Reset() { anim = GetComponent<Animator>(); }
    void Update()
    {
        if (!anim || clips.Length == 0) return;
        t += Time.unscaledDeltaTime;
        if (t >= interval)
        {
            t = 0f;
            i = (i + 1) % clips.Length;
            anim.Play(clips[i], 0, 0f);
        }
    }
}