using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroySound;
    public AudioSource backgroundMusic;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                return;
            } else {
                backgroundMusic.Play();
            }
        }
    }

    public void AdjustVolume()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 0;
            }
            else
            {
                backgroundMusic.Play();
                backgroundMusic.volume = .05f;
            }
        } else {
            backgroundMusic.Play();
            backgroundMusic.volume = .05f;
        }
    }

    public void PlayRandomDestroySound()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                int clipToPlay = Random.Range(0, destroySound.Length);
                destroySound[clipToPlay].Play();
            }
        } else {
            int clipToPlay = Random.Range(0, destroySound.Length);
            destroySound[clipToPlay].Play();
        }
        /* int clipToPlay = Random.Range(0, destroySound.Length);
        destroySound[clipToPlay].Play(); */
    }
}
