using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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



}