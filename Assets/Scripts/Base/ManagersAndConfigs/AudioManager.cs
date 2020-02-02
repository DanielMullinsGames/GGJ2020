using UnityEngine;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip MenuMusic;

    private AudioSource mMusicPlayer;

    private void Awake()
    {
        if (Instance  == null)
        {
            mMusicPlayer = GetComponent<AudioSource>();
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            mMusicPlayer.Play();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    internal void SetMusic(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            StartCoroutine(TransitionToClip(audioClip));
        }
    }

    private IEnumerator TransitionToClip(AudioClip audioClip)
    {
        if (mMusicPlayer.clip != audioClip)
        {
            yield return FadeOut();
        }
        mMusicPlayer.clip = audioClip;
        mMusicPlayer.Play();
        yield return FadeIn();
    }

    private IEnumerator FadeOut()
    {
        while (mMusicPlayer.volume > 0f)
        {
            mMusicPlayer.volume -= Time.deltaTime * 2f;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FadeIn()
    {
        while (mMusicPlayer.volume < 1f)
        {
            mMusicPlayer.volume += Time.deltaTime * 2f;
            yield return new WaitForEndOfFrame();
        }
    }

    public void PlayMenuMusic()
    {
        SetMusic(MenuMusic);
    }
}