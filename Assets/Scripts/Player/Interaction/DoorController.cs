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

    #region GenericSettings
    [Header("Generic Settings")]
    [ConditionalHide(nameof(useType), 0)]
    [SerializeField] private bool closesAutomatically = false;
    [SerializeField] private float closingTime = 3f;
    [SerializeField] private float closingDistance = 3f;
    #endregion

    [Space(10)]

    #region SoundSettings
    [Header("Sound Settings")]
    [SerializeField] private AudioClip lockedTrySound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip openDoorFullSound;
    [SerializeField] private AudioClip initOpenDoorSound;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip initCloseDoorSound;
    [SerializeField] private AudioClip closeDoorSound;
    #endregion

    private PlayerController playerController;
    private Animator animator;
    private AudioSource audioSource;
    private bool isLocked;
    private bool canBeInteractedWith = true;
    private bool isHalfOpen;
    private bool isFullOpen;

    private void Awake()
    {
        if (useType == Type_Use.Locked)
        {
            isLocked = true;
        }
    }

    private void Start()
    {
        playerController = PlayerController.Instance;
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
                //InteractWithLockedDoor();
                break;
            case (Type_Use.Jammed):
                InteractWithJammedDoor();
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

    private void InteractWithJammedDoor()
    {
        Debug.Log("Interact with jammed door");
    }

    private void UnlockDoor()
    {
        canBeInteractedWith = false;
        isLocked = false;
        audioSource.PlayOneShot(unlockSound, 1f);
        StartCoroutine(WaitForXSecondsToInteractAgain(1.5f));
    }

    private void OpenDoor()
    {
        if (canBeInteractedWith)
        {
            if (!isHalfOpen && !isFullOpen)
            {
                Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
                Vector3 playerTransformDirection = PlayerController.Instance.transform.position - transform.position;
                float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

                if (dot < 0)
                {
                    isFullOpen = true;
                    animator.SetFloat("Dot", dot);
                    animator.SetBool("IsFullOpen", isFullOpen);

                    audioSource.PlayOneShot(openDoorFullSound, 1f);

                    if (closesAutomatically)
                    {
                        StartCoroutine(AutoCloseFromFullOpen(PlayerController.Instance.transform));
                    }
                }
                else
                {
                    //Go half open
                    isHalfOpen = true;

                    animator.SetFloat("Dot", dot);
                    animator.SetBool("IsHalfOpen", isHalfOpen);

                    audioSource.PlayOneShot(initOpenDoorSound, 1f);

                    StartCoroutine(AutoCloseFromHalfOpen(PlayerController.Instance.transform));
                }
            }
            else if (isHalfOpen && !isFullOpen)
            {
                //Go full open
                isHalfOpen = false;
                isFullOpen = true;

                Vector3 doorTransformDirection = transform.TransformDirection(Vector3.right);
                Vector3 playerTransformDirection = PlayerController.Instance.transform.position - transform.position;
                float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

                animator.SetFloat("Dot", dot);
                animator.SetBool("IsHalfOpen", isHalfOpen);
                animator.SetBool("IsFullOpen", isFullOpen);

                audioSource.PlayOneShot(openDoorSound, 1f);

                if (closesAutomatically)
                {
                    StartCoroutine(AutoCloseFromFullOpen(PlayerController.Instance.transform));
                }
            }
            else
            {
                //Go close
                isHalfOpen = false;
                isFullOpen = false;

                audioSource.PlayOneShot(closeDoorSound, 1f);

                animator.SetFloat("Dot", 0);
                animator.SetBool("IsFullOpen", isFullOpen);
            }
        }
    }

    private IEnumerator WaitForXSecondsToInteractAgain(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        canBeInteractedWith = true;
    }

    private IEnumerator AutoCloseFromHalfOpen(Transform interactorTransform)
    {
        while (isHalfOpen)
        {
            if (isHalfOpen && !isFullOpen)
            {
                yield return new WaitForSeconds(closingTime);

                if (Vector3.Distance(transform.position, interactorTransform.position) > closingDistance && isHalfOpen)
                {
                    isHalfOpen = false;
                    animator.SetFloat("Dot", 0);
                    animator.SetBool("IsHalfOpen", isHalfOpen);
                    audioSource.PlayOneShot(initCloseDoorSound, 1f);
                }
            }
        }
    }

    private IEnumerator AutoCloseFromFullOpen(Transform interactorTransform)
    {
        while (isFullOpen)
        {
            if (!isHalfOpen && isFullOpen)
            {
                yield return new WaitForSeconds(closingTime);

                if (Vector3.Distance(transform.position, interactorTransform.position) > closingDistance)
                {
                    isFullOpen = false;
                    animator.SetFloat("Dot", 0);
                    animator.SetBool("IsFullOpen", isFullOpen);
                    audioSource.PlayOneShot(closeDoorSound, 1f);
                }
            }
        }
    }

    private void Animator_LockInteraction()
    {
        canBeInteractedWith = false;
    }

    private void Animator_UnlockInteraction()
    {
        canBeInteractedWith = true;
    }

    public override void ShowIdentifierUI()
    {
        //Do nothing
    }

    public override void HideIdentifierUI()
    {
        //Do nothing
    }

    public override void ShowInteractUI()
    {
        //Do nothing
    }

    public override void HideInteractUI()
    {
        //Do nothing
    }

    #region Save and load data

    public void Save(ref DoorSaveData data)
    {
        data.IsLocked = isLocked;
        data.IsOpen = isFullOpen;
        data.Dot = animator.GetFloat("Dot");
    }

    public void Load(DoorSaveData data)
    {
        isLocked = data.IsLocked;
        isFullOpen = data.IsOpen;
        if (isFullOpen)
        {
            if (animator != null)
            {
                animator.SetBool("IsFullOpen", isFullOpen);
                animator.SetFloat("Dot", data.Dot);
            }
        }
    }

    #endregion
}


[System.Serializable]
public struct DoorSaveData
{
    public bool IsLocked;
    public bool IsOpen;
    public float Dot;
}