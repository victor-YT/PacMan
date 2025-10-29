using UnityEngine;
using System.Collections;

public class GhostController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("State Name Prefix (must match your Animator)")]
    public string normalPrefix  = "Walk_";
    public string scaredPrefix  = "Scared_";
    public string recoverPrefix = "";       // leave empty to use flicker (Scared <-> Normal)
    public string deadPrefix    = "Dead_";

    [Header("Direction Suffix (must match your Animator)")]
    public string dirUp    = "Up";
    public string dirDown  = "Down";
    public string dirLeft  = "Left";
    public string dirRight = "Right";

    [Header("Recovering (Blink/Flicker)")]
    public bool   useFlickerForRecover = true;  // if true OR recoverPrefix is empty -> flicker
    public float  recoverFlickerInterval = 0.25f;

    public GhostState State { get; private set; } = GhostState.Normal;

    string currentDirSuffix = "Up";
    Coroutine recoverFlashRoutine;
    string lastPlayedStateName = null;  // throttle redundant Animator.Play

    // ---- Public API ----
    public void SetState(GhostState s)
    {
        // stop any recovering flicker when leaving that state
        if (recoverFlashRoutine != null)
        {
            StopCoroutine(recoverFlashRoutine);
            recoverFlashRoutine = null;
        }

        State = s;

        // Recovering: either play dedicated animation or start flicker
        if (State == GhostState.Recovering && (useFlickerForRecover || string.IsNullOrEmpty(recoverPrefix)))
        {
            recoverFlashRoutine = StartCoroutine(RecoverFlash());
        }
        else
        {
            PlayForStateAndDir();
        }
    }

    public void SetFacing(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            currentDirSuffix = dir.x >= 0 ? dirRight : dirLeft;
        else
            currentDirSuffix = dir.y >= 0 ? dirUp : dirDown;

        // if we are flickering, let the coroutine control animation, otherwise update now
        if (!(State == GhostState.Recovering && (useFlickerForRecover || string.IsNullOrEmpty(recoverPrefix))))
            PlayForStateAndDir();
    }

    public void Freeze(bool stop)
    {
        if (animator) animator.speed = stop ? 0f : 1f;
    }

    public void ResetToSpawn()
    {
        // stop flicker
        if (recoverFlashRoutine != null)
        {
            StopCoroutine(recoverFlashRoutine);
            recoverFlashRoutine = null;
        }

        State = GhostState.Normal;
        currentDirSuffix = dirUp;
        lastPlayedStateName = null;
        PlayForStateAndDir();
    }

    // ---- Internal ----
    void PlayForStateAndDir()
    {
        if (!animator) return;

        string prefix;
        switch (State)
        {
            default:
            case GhostState.Normal:     prefix = normalPrefix;  break;
            case GhostState.Scared:     prefix = scaredPrefix;  break;
            case GhostState.Recovering: prefix = string.IsNullOrEmpty(recoverPrefix) ? scaredPrefix : recoverPrefix; break;
            case GhostState.Dead:       prefix = deadPrefix;    break;
        }

        string stateName = prefix + currentDirSuffix;

        // Avoid spamming Animator.Play on the same state every frame
        if (stateName == lastPlayedStateName) return;

        // Optional safety: only play if exists; otherwise fall back to prefix + "Up"
        int hash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, hash))
        {
            string fallback = prefix + dirUp;
            if (fallback != lastPlayedStateName && animator.HasState(0, Animator.StringToHash(fallback)))
            {
                animator.Play(fallback, 0, 0f);
                lastPlayedStateName = fallback;
                return;
            }
        }

        animator.Play(stateName, 0, 0f);
        lastPlayedStateName = stateName;
    }

    IEnumerator RecoverFlash()
    {
        // Alternate between Scared and Normal animations until state changes
        while (State == GhostState.Recovering)
        {
            // scared frame
            if (!string.IsNullOrEmpty(scaredPrefix))
            {
                string s = scaredPrefix + currentDirSuffix;
                if (animator) animator.Play(s, 0, 0f);
                lastPlayedStateName = s;
            }
            yield return new WaitForSeconds(recoverFlickerInterval);

            // normal frame
            if (State != GhostState.Recovering) break;
            if (!string.IsNullOrEmpty(normalPrefix))
            {
                string n = normalPrefix + currentDirSuffix;
                if (animator) animator.Play(n, 0, 0f);
                lastPlayedStateName = n;
            }
            yield return new WaitForSeconds(recoverFlickerInterval);
        }
        recoverFlashRoutine = null;
        // When we leave recovering, ensure we show the new state's proper clip
        PlayForStateAndDir();
    }
}