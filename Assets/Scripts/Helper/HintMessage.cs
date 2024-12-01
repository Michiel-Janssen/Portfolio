using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class HintMessage : MonoBehaviour
{
    [SerializeField] private string hintMessage;
    [SerializeField] private float timeShow = 3f;
    [SerializeField] private float showAfter = 0f;
    [SerializeField] private AudioClip hintSound;

    private GameManager gameManager;
    private AudioSource audioSource;
    private bool isShownAlready = false;
    private bool timedShow;
    private float timer;

    private void Start()
    {
        gameManager = GameManager.Instance;
        audioSource = GetComponent<AudioSource>() ? GetComponent<AudioSource>() : null;

        if (hintSound && !audioSource)
        {
            Debug.LogError("HintSound require an audioSource component!");
        }

        if (audioSource)
        {
            audioSource.spatialBlend = 0f;
        }
    }

    public void ShowHint()
    {
        if(!isShownAlready)
        {
            if (showAfter > 0)
            {
                timedShow = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(hintMessage))
                {
                    gameManager.ShowHint(hintMessage, timeShow);
                }

                if (hintSound && audioSource)
                {
                    audioSource.clip = hintSound;
                    audioSource.Play();
                }
                isShownAlready = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            ShowHint();
        }
    }

    private void Update()
    {
        if (!isShownAlready && timedShow)
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= showAfter)
            {
                if (!string.IsNullOrEmpty(hintMessage))
                {
                    gameManager.ShowHint(hintMessage, timeShow);
                }

                if (hintSound && audioSource)
                {
                    audioSource.clip = hintSound;
                    audioSource.Play();
                }
                isShownAlready = true;
            }
        }
    }
}
