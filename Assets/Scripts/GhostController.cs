using UnityEngine;

public class GhostController : MonoBehaviour
{
    public Animator animator;
    public string animNormal = "normal";
    public string animScared = "scared";
    public string animRecover = "recover";
    public string animDead = "dead";

    public GhostState State { get; private set; } = GhostState.Normal;

    Vector3 spawnPos;
    bool frozen;

    void Awake()
    {
        spawnPos = transform.position;
        PlayStateAnim(State);
    }

    public void Freeze(bool stop)
    {
        frozen = stop;
        if (animator) animator.speed = stop ? 0f : 1f;
    }

    public void SetState(GhostState s)
    {
        if (State == s) return;
        State = s;
        PlayStateAnim(State);
    }

    public void ResetToSpawn()
    {
        transform.position = spawnPos;
        SetState(GhostState.Normal);
        Freeze(false);
    }

    void PlayStateAnim(GhostState s)
    {
        if (!animator || animator.runtimeAnimatorController == null) return;
        switch (s)
        {
            case GhostState.Normal:     animator.Play(animNormal, 0, 0f); break;
            case GhostState.Scared:     animator.Play(animScared, 0, 0f); break;
            case GhostState.Recovering: animator.Play(animRecover, 0, 0f); break;
            case GhostState.Dead:       animator.Play(animDead, 0, 0f); break;
        }
    }
}