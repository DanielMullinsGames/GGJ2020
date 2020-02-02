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
        if (audioClip != null && mMusicPlayer.clip != audioClip)
        {
            mMusicPlayer.clip = audioClip;
            mMusicPlayer.Play();
        }
    }

    public void PlayMenuMusic()
    {
        SetMusic(MenuMusic);
    }
}