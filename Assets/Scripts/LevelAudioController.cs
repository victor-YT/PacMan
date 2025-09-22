using UnityEngine;
using System.Collections;

public class LevelAudioController : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip bgmIntro_LevelStart;
    public AudioClip bgmGhosts_Normal;

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
        if (bgmIntro_LevelStart != null && musicSource != null)
        {
            musicSource.loop = false;
            musicSource.clip = bgmIntro_LevelStart;
            musicSource.Play();
            
            float waitTime = Mathf.Min(bgmIntro_LevelStart.length, 3f);
            yield return new WaitForSeconds(waitTime);
        }
        
        PlayNormalBGM();
    }

    public void PlayNormalBGM()
    {
        if (bgmGhosts_Normal == null || musicSource == null) return;
        musicSource.loop = true;
        musicSource.clip = bgmGhosts_Normal;
        musicSource.Play();
    }
    
    public void PlayScaredBGM() { if (bgmGhosts_Scared && musicSource){ musicSource.loop = true; musicSource.clip = bgmGhosts_Scared; musicSource.Play(); } }
    public void PlayDeadBGM()   { if (bgmGhosts_Dead && musicSource)  { musicSource.loop = true; musicSource.clip = bgmGhosts_Dead;   musicSource.Play(); } }
    
    public void PlayMoveSFX()   { if (sfxMove   && sfxSource) sfxSource.PlayOneShot(sfxMove); }
    public void PlayPelletSFX() { if (sfxPellet && sfxSource) sfxSource.PlayOneShot(sfxPellet); }
    public void PlayWallHitSFX(){ if (sfxWallHit&& sfxSource) sfxSource.PlayOneShot(sfxWallHit); }
    public void PlayDeathSFX()  { if (sfxDeath  && sfxSource) sfxSource.PlayOneShot(sfxDeath); }
}