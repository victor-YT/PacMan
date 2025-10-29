using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase { Countdown, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    public HUDController hud;
    public PacStudentController player;
    public List<GhostController> ghosts;
    public AudioSource bgm;

    [Header("BGM")]
    public AudioClip bgmIntro;
    public AudioClip bgmNormal;
    public AudioClip bgmScared;
    public AudioClip bgmEatGhost;

    [Header("Rules")]
    public int startLives = 3;
    public int pelletScore = 10;
    public int powerScore = 50;
    public int cherryScore = 100;
    public int eatGhostScore = 300;
    public float scaredDuration = 10f;

    [Header("Round")]
    public string startSceneName = "StartScene";

    public GamePhase Phase { get; private set; } = GamePhase.Countdown;
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public float PlayTimer { get; private set; }

    float scaredRemain;
    bool recoveringShown;

    const string PP_BEST_SCORE = "BEST_SCORE";
    const string PP_BEST_TIME  = "BEST_TIME";

    void Start()
    {
        Lives = startLives;
        if (hud)
        {
            hud.SetLives(Lives);
            hud.SetScore(0);
            hud.SetTimer(0);
        }
        FreezeActors(true);
        StartCoroutine(RoundStartRoutine());
    }

    void Update()
    {
        if (Phase != GamePhase.Playing) return;

        PlayTimer += Time.deltaTime;
        if (hud) hud.SetTimer(PlayTimer);

        if (scaredRemain > 0f)
        {
            scaredRemain -= Time.deltaTime;
            int left = Mathf.CeilToInt(scaredRemain);
            if (hud) hud.ShowScaredTimer(left);

            if (scaredRemain <= 3f && !recoveringShown)
            {
                SetGhostsState(GhostState.Recovering);
                recoveringShown = true;
            }

            if (scaredRemain <= 0f)
            {
                if (hud) hud.ShowScaredTimer(0);
                SetGhostsState(GhostState.Normal);
                PlayBgm(bgmNormal, true);
            }
        }
    }

    IEnumerator RoundStartRoutine()
{
    Phase = GamePhase.Countdown;

    hud.ShowGameOver(false);

    if (bgm && bgmIntro)
    {
        bgm.clip = bgmIntro;
        bgm.loop = false;
        bgm.Play();
    }

    hud.ShowCountdown(true, "3"); yield return new WaitForSeconds(1f);
    hud.ShowCountdown(true, "2"); yield return new WaitForSeconds(1f);
    hud.ShowCountdown(true, "1"); yield return new WaitForSeconds(1f);
    hud.ShowCountdown(true, "GO!"); yield return new WaitForSeconds(1f);
    hud.ShowCountdown(false);

    Phase = GamePhase.Playing;
    FreezeActors(false);

    if (bgm && bgmNormal)
    {
        bgm.clip = bgmNormal;
        bgm.loop = true;
        bgm.Play();
    }
}

    void PlayBgm(AudioClip clip, bool loop)
    {
        if (!bgm || !clip) return;
        bgm.loop = loop;
        bgm.clip = clip;
        bgm.Play();
    }

    void FreezeActors(bool stop)
    {
        // Player
        if (player) player.EnableControl(!stop);

        // Ghosts
        if (ghosts == null) return;
        foreach (var g in ghosts)
        {
            if (!g) continue;

            g.Freeze(stop);

            var mv = g.GetComponent<GhostMovement>();
            if (mv) mv.Freeze(stop);
        }
    }

    public void OnPelletEaten()
    {
        AddScore(pelletScore);
        CheckAllPelletsEaten();
    }

    public void OnPowerPelletEaten()
    {
        AddScore(powerScore);
        StartScared(scaredDuration);
        CheckAllPelletsEaten();
    }

    public void OnCherryEaten()
    {
        AddScore(cherryScore);
    }

    public void OnHitGhost(GhostController ghost)
    {
        if (!ghost) return;

        if (ghost.State == GhostState.Normal)
        {
            StartCoroutine(PlayerDeathRoutine());
        }
        else if (ghost.State == GhostState.Scared || ghost.State == GhostState.Recovering)
        {
            ghost.SetState(GhostState.Dead);
            AddScore(eatGhostScore);
            PlayBgm(bgmEatGhost, false);
            StartCoroutine(ReturnToScaredOrNormalAfter(bgmEatGhost ? bgmEatGhost.length : 0.5f));
        }
    }

    IEnumerator ReturnToScaredOrNormalAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (Phase != GamePhase.Playing) yield break;
        if (scaredRemain > 0f) PlayBgm(bgmScared, true);
        else PlayBgm(bgmNormal, true);
    }

    IEnumerator PlayerDeathRoutine()
    {
        FreezeActors(true);
        if (bgm) bgm.Stop();
        if (player) player.PlayDeath();
        yield return new WaitForSeconds(1.2f);

        Lives--;
        if (hud) hud.SetLives(Lives);

        if (Lives <= 0) { GameOver(); yield break; }

        if (ghosts != null) foreach (var g in ghosts) if (g) g.ResetToSpawn();
        PlayBgm(bgmNormal, true);
        FreezeActors(false);
    }

    void StartScared(float duration)
    {
        scaredRemain = duration;
        recoveringShown = false;
        SetGhostsState(GhostState.Scared);
        if (hud) hud.ShowScaredTimer(Mathf.CeilToInt(scaredRemain));
        PlayBgm(bgmScared, true);
    }

    void SetGhostsState(GhostState s)
    {
        if (ghosts != null) foreach (var g in ghosts) if (g) g.SetState(s);
    }

    void AddScore(int add)
    {
        Score += add;
        if (hud) hud.SetScore(Score);
    }

    void CheckAllPelletsEaten()
    {
        if (player && player.RemainingPelletCount() == 0) GameOver();
    }

    void GameOver()
    {
        Phase = GamePhase.GameOver;
        FreezeActors(true);
        if (hud) hud.ShowGameOver(true);
        SaveBestIfBetter();
        StartCoroutine(ReturnToStart());
    }

    void SaveBestIfBetter()
    {
        int bestScore = PlayerPrefs.GetInt(PrefKeys.BestScore, 0);
        float bestTime = PlayerPrefs.GetFloat(PrefKeys.BestTime, 0f);

        bool better = Score > bestScore || (Score == bestScore && (bestTime <= 0f || PlayTimer < bestTime));
        if (better)
        {
            PlayerPrefs.SetInt(PrefKeys.BestScore, Score);
            PlayerPrefs.SetFloat(PrefKeys.BestTime, PlayTimer);
            PlayerPrefs.Save();
            Debug.Log($"Saved Best: score={Score}, time={PlayTimer:0.00}");
        }
    }

    IEnumerator ReturnToStart()
    {
        yield return new WaitForSeconds(3f);
        if (!string.IsNullOrEmpty(startSceneName)) SceneManager.LoadScene(startSceneName);
    }
}