using UnityEngine;
using System.Collections;

public class LevelAudioController : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource musicSource;   // 拖 MusicSource 上的 AudioSource 进来
    public AudioSource sfxSource;     // 拖 SFXSource 上的 AudioSource 进来（可选）

    [Header("BGM Clips")]
    public AudioClip bgmIntro_LevelStart;
    public AudioClip bgmGhosts_Normal;
    // 预留：后面可以拖入被吃/死亡等状态的BGM
    public AudioClip bgmGhosts_Scared;
    public AudioClip bgmGhosts_Dead;

    [Header("SFX Clips (optional)")]
    public AudioClip sfxMove;
    public AudioClip sfxPellet;
    public AudioClip sfxWallHit;
    public AudioClip sfxDeath;

    void Start()
    {
        StartCoroutine(PlayIntroThenNormal());
    }

    IEnumerator PlayIntroThenNormal()
    {
        // 先播 Intro（不循环）
        if (bgmIntro_LevelStart != null && musicSource != null)
        {
            musicSource.loop = false;
            musicSource.clip = bgmIntro_LevelStart;
            musicSource.Play();

            // 关键：等待 Intro 时长与 3 秒中更短的那个
            float waitTime = Mathf.Min(bgmIntro_LevelStart.length, 3f);
            yield return new WaitForSeconds(waitTime);
        }

        // 切换到 Normal（循环）
        PlayNormalBGM();
    }

    public void PlayNormalBGM()
    {
        if (bgmGhosts_Normal == null || musicSource == null) return;
        musicSource.loop = true;
        musicSource.clip = bgmGhosts_Normal;
        musicSource.Play();
    }

    // 预留接口：后面做状态切换时直接调用
    public void PlayScaredBGM() { if (bgmGhosts_Scared && musicSource){ musicSource.loop = true; musicSource.clip = bgmGhosts_Scared; musicSource.Play(); } }
    public void PlayDeadBGM()   { if (bgmGhosts_Dead && musicSource)  { musicSource.loop = true; musicSource.clip = bgmGhosts_Dead;   musicSource.Play(); } }

    // 示例：后面可用来触发音效
    public void PlayMoveSFX()   { if (sfxMove   && sfxSource) sfxSource.PlayOneShot(sfxMove); }
    public void PlayPelletSFX() { if (sfxPellet && sfxSource) sfxSource.PlayOneShot(sfxPellet); }
    public void PlayWallHitSFX(){ if (sfxWallHit&& sfxSource) sfxSource.PlayOneShot(sfxWallHit); }
    public void PlayDeathSFX()  { if (sfxDeath  && sfxSource) sfxSource.PlayOneShot(sfxDeath); }
}