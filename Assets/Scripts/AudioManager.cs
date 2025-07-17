using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages background music and sound effects throughout the game,
/// using an audio source pool for efficient SFX playback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    #region Music Settings
    [Header("Music Settings")]
    public AudioSource bgmSource;
    public AudioClip mainBackgroundMusic;
    public AudioClip ClickSound;
    public AudioClip countDown;
    #endregion

    #region SFX Settings
    [Header("SFX Settings")]
    public AudioSource sfxSourcePrefab;
    public int sfxPoolSize = 10;
    public float sfxFadeInDuration = 0.1f;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    #endregion

    #region Volume Settings
    [Header("Volumes")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSFXPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }
    #endregion

    #region BGM Methods
    public void PlayBackgroundMusic(AudioClip clip = null)
    {
        if (bgmSource == null) return;

        bgmSource.clip = clip ?? mainBackgroundMusic;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = volume;
    }
    #endregion

    #region SFX Methods
    private void InitSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfx = Instantiate(sfxSourcePrefab, transform);
            sfx.playOnAwake = false;
            sfx.volume = sfxVolume;
            sfxPool.Add(sfx);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSFXSource();
        if (availableSource != null)
        {
            StartCoroutine(FadeInAndPlay(availableSource, clip));
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        foreach (var sfx in sfxPool)
        {
            sfx.volume = volume;
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
                return source;
        }
        return sfxPool[0];
    }

    private IEnumerator FadeInAndPlay(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        float elapsed = 0f;
        while (elapsed < sfxFadeInDuration)
        {
            source.volume = Mathf.Lerp(0f, sfxVolume, elapsed / sfxFadeInDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        source.volume = sfxVolume;
    }
    #endregion
}
