using UnityEngine;
using System.Collections;

public class PacStudentMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float unitsPerSecond = 3f;
    public Animator animator;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    IEnumerator Start()
    {
        if (waypoints == null || waypoints.Length < 2) yield break;
        transform.position = waypoints[0].position;

        int i = 0;
        while (true)
        {
            int next = (i + 1) % waypoints.Length;
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[next].position;
            Vector3 delta = b - a;
            float dist = delta.magnitude;
            if (dist < 0.0001f) { i = next; yield return null; continue; }

            Vector3 dir = delta.normalized;
            PlayDirAnim(dir);

            float duration = dist / Mathf.Max(0.0001f, unitsPerSecond);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(a, b, Mathf.Clamp01(t));
                yield return null;
            }
            i = next;
        }
    }

    void PlayDirAnim(Vector3 dir)
    {
        if (!animator) return;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            animator.Play(dir.x > 0 ? "player_move_right" : "player_move_left");
        else
            animator.Play(dir.y > 0 ? "player_move_up" : "player_move_down");
    }
}