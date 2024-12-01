using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private AudioSource scaredBreathingAudioSource;

    private void Awake()
    {
        Instance = this;
    }
}
