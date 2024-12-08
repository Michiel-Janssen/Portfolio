using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public enum Type_Use
{
    Normal,
    Locked,
    Jammed,
}

public enum Type_Key
{
    Key,
    Script
}

public class DoorController : BaseInteractable
{
    #region Enums
    [Header("Enums")]
    [SerializeField] private Type_Use useType = Type_Use.Normal;
    [ConditionalHide(nameof(useType), 0)]
    [SerializeField] private Type_Key keyType = Type_Key.Key;
    #endregion

    [Space(10)]

    #region SoundSettings
    [Header("Sound Settings")]
    [SerializeField] private AudioClip lockedTrySound;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip closeDoorSound;
    #endregion

    private Animator animator;
    private AudioSource audioSource;
    private bool isLocked;
    private bool canBeInteractedWith = true;
    private bool isOpen;

    private void Awake()
    {
        if (useType == Type_Use.Locked)
        {
            isLocked = true;
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        switch(useType)
        {
            case (Type_Use.Normal):
                InteractWithNormalDoor();
                break;
            case (Type_Use.Locked):
                InteractWithLockedDoor();
                break;
            default:
                InteractWithNormalDoor();
                break;
        }
    }

    private void InteractWithNormalDoor()
    {
        OpenDoor();
    }

    private void InteractWithLockedDoor()
    {
        TryDoor();
    }

    private void TryDoor()
    {
        if(canBeInteractedWith)
        {
            if(isLocked)
            {
                audioSource.PlayOneShot(lockedTrySound, 1f);
            }
        }
    }

    private void OpenDoor()
    {
        if (canBeInteractedWith)
        {
            if(isOpen)
            {
                isOpen = false;
                audioSource.PlayOneShot(closeDoorSound, 1f);
                animator.SetBool("open", isOpen);
            }
            else
            {
                isOpen = true;
                audioSource.PlayOneShot(openDoorSound, 1f);
                animator.SetBool("open", isOpen);
            }
        }
    }

    private IEnumerator WaitForXSecondsToInteractAgain(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        canBeInteractedWith = true;
    }

    private void Animator_LockInteraction()
    {
        canBeInteractedWith = false;
    }
}