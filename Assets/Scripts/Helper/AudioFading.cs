using UnityEngine;
using System.Collections;

public static class AudioFading
{

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime, float maxVolume)
    {
        audioSource.volume = maxVolume;
        float startVolume = maxVolume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float maxVolume)
    {
        audioSource.volume = 0f;
        float targetVolume = maxVolume;

        audioSource.Play();

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += targetVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}