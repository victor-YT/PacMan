using UnityEngine;

public class PacUI_Move : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 200f;

    private int index = 0;
    private RectTransform rt;
    private Animator anim;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        anim = GetComponent<Animator>();

        rt.position = waypoints[0].position;
        PlayDirAnim(waypoints[0].position, waypoints[1].position);
        index = 1;
    }

    void Update()
    {
        RectTransform target = waypoints[index] as RectTransform;

        Vector3 dir = target.position - rt.position;

        float step = speed * Time.deltaTime;
        if (dir.magnitude <= step)
        {
            rt.position = target.position;
            int prev = index;
            index = (index + 1) % waypoints.Length;
            PlayDirAnim(waypoints[prev].position, waypoints[index].position);
            return;
        }

        rt.position += dir.normalized * step;
    }

    void PlayDirAnim(Vector3 from, Vector3 to)
    {
        if (!anim) return;
        Vector3 dir = to - from;

        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            anim.Play(dir.x >= 0 ? "move_right" : "move_left");
        else
            anim.Play(dir.y >= 0 ? "move_up"    : "move_down");
    }
}