using Unity.Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputControls))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("General Parameters")]
    [SerializeField] private LayerMask playerLayer;

    [Space(10)]

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float speedChangeRate;
    [SerializeField] private float stopSmooth;
    [SerializeField] private float moveSmooth;
    [SerializeField] private bool isStopped;
    private float smoothTime;
    private float targetSpeed;
    private Vector2 currentVelocity;

    [Space(10)]

    [Header("Footsteps")]
    [SerializeField] private float footstepCooldown = 0.1f;
    [SerializeField] private float footstepVolume = 0.3f;
    private float lastFootstepTime;

    [Space(10)]

    [Header("Breathing")]
    [SerializeField] private float sprintBreathingDelay = 1f;
    [SerializeField] private float postSprintBreathingTime = 3f;
    [SerializeField] private float breathingAudioFadeIn = 2f;
    [SerializeField] private float breathingAudioFadeOut = 2f;
    [SerializeField] private float maxBreathingVolume = 0.8f;
    [Tooltip("Needs to be higher then the sprintBreathingDelay")]
    [SerializeField] private float maxSprintTimer = 10f;
    private bool isBreathing = false;
    private float sprintTimer = 0f;
    private float stopSprintingTime = 0f;
    private Coroutine currentBreathingCoroutine = null;

    [Space(10)]

    [Header("Physics Parameters")]
    [SerializeField] private float gravity = -15.0f;
    private float terminalVelocity = 53f;

    [Space(10)]

    [Header("Audio Parameters")]
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private AudioSource footstepAudioSource;

    private InputManager inputManager;
    private Animator animator;
    private CharacterController characterController;

    private int X_VelocityHash;
    private int Y_VelocityHash;

    private Vector3 rootMotion;

    private float verticalVelocity;
    private bool hasAnimator;
    private bool duringCrouchAnimation;
    private float standingControllerHeight = 1.7f;
    private float crouchedControllerHeight = 1.2f;
    private Vector3 standingControllerCenter = new Vector3(0f, 0.85f, 0f);
    private Vector3 crouchedControllerCenter = new Vector3(0f, 0.55f, 0f);
    private bool isCrouching = false;

    private List<AudioClip> footstepSounds = new List<AudioClip>();
    private FootstepSwapper footstepSwapper;

    private bool canMove = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        inputManager = GetComponent<InputManager>();
        hasAnimator = TryGetComponent(out animator);
        characterController = GetComponent<CharacterController>();
        footstepSwapper = GetComponent<FootstepSwapper>();

        X_VelocityHash = Animator.StringToHash("Velocity_x");
        Y_VelocityHash = Animator.StringToHash("Velocity_y");
    }

    void Update()
    {
        ApplyGravity();
        MoveWithRootMotion();
        HandleCrouching();
        HandleBreathing();
    }

    public void CanMove(bool canMoveBool)
    {
        canMove = canMoveBool;
    }

    public void SwapFootsteps(FootstepCollection collection)
    {
        footstepSounds.Clear();
        for (int i = 0; i < collection.footstepSounds.Count; i++)
        {
            footstepSounds.Add(collection.footstepSounds[i]);
        }
    }

    private void HandleCrouching()
    {
        if (!canMove) return;

        if (isCrouching && Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 2f, ~playerLayer))
        {
            Debug.Log($"I hit this {hit.collider.name}");
            return;
        }


        if (inputManager.GetIsCrouching())
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
            characterController.height = crouchedControllerHeight;
            characterController.center = crouchedControllerCenter;
        }
        else
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
            characterController.height = standingControllerHeight;
            characterController.center = standingControllerCenter;
        }
    }

    private void MoveWithRootMotion()
    {
        if (!canMove)
        {
            DOTween.To(() => currentVelocity.x, x => currentVelocity.x = x, 0, smoothTime);
            DOTween.To(() => currentVelocity.y, x => currentVelocity.y = x, 0, smoothTime);

            animator.SetFloat(X_VelocityHash, 0);
            animator.SetFloat(Y_VelocityHash, 0);

            return;
        }

        float deltaTimeMultiplier = Time.deltaTime;

        Vector2 MoveInput = inputManager.GetMovement().normalized;

        if (MoveInput == Vector2.zero)
        {
            DOTween.To(() => targetSpeed, x => targetSpeed = x, 0, smoothTime);

            isStopped = (targetSpeed > 0);
        }
        else
        {
            isStopped = false;

            if (inputManager.GetIsSprinting())
            {

                DOTween.To(() => targetSpeed, x => targetSpeed = x, runSpeed, smoothTime);
            }
            else
            {
                DOTween.To(() => targetSpeed, x => targetSpeed = x, walkSpeed, smoothTime);
            }

        }

        if (targetSpeed <= 0.5f) { targetSpeed = 0; }

        smoothTime = isStopped ? stopSmooth : moveSmooth;

        DOTween.To(() => currentVelocity.x, x => currentVelocity.x = x, targetSpeed * MoveInput.x, smoothTime);
        DOTween.To(() => currentVelocity.y, x => currentVelocity.y = x, targetSpeed * MoveInput.y, smoothTime);

        if (Mathf.Abs(currentVelocity.x) < 0.01f) { currentVelocity.x = 0; }
        if (Mathf.Abs(currentVelocity.y) < 0.01f) { currentVelocity.y = 0; }

        animator.SetFloat(X_VelocityHash, currentVelocity.x);
        animator.SetFloat(Y_VelocityHash, currentVelocity.y);

        characterController.Move(rootMotion);
        rootMotion = Vector3.zero;
    }

    private void OnAnimatorMove()
    {
        rootMotion += animator.deltaPosition;
        rootMotion.y = verticalVelocity * Time.deltaTime;
    }

    private void ApplyGravity()
    {
        if (!canMove) return;

        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (verticalVelocity < 0.0f)
        {
            verticalVelocity = -2f;
        }
    }

    public void PlayFootStep()
    {
        footstepSwapper.CheckLayers();

        if (Time.time >= lastFootstepTime + footstepCooldown)
        {
            if (Mathf.Clamp(footstepVolume * characterController.velocity.magnitude, 0f, 0.3f) < 0.05f) return;
            footstepAudioSource.PlayOneShot(footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Count - 1)], Mathf.Clamp(footstepVolume * characterController.velocity.magnitude, 0f, 0.3f));
            lastFootstepTime = Time.time;
        }
    }

    private void HandleBreathing()
    {
        if (inputManager.GetIsSprinting())
        {
            if (isBreathing)
            {
                stopSprintingTime = 0f;
            }
            if (sprintTimer <= maxSprintTimer)
            {
                sprintTimer += Time.deltaTime;
            }

            if (!isBreathing && sprintTimer >= sprintBreathingDelay)
            {
                StartBreathing();
            }
        }
        else
        {
            if (sprintTimer > 0)
            {
                sprintTimer -= Time.deltaTime;
            }
            if (isBreathing)
            {
                stopSprintingTime += Time.deltaTime;

                if (stopSprintingTime >= postSprintBreathingTime)
                {
                    StopBreathing();
                }
            }
        }
    }

    private void StartBreathing()
    {
        isBreathing = true;
        if (currentBreathingCoroutine != null)
        {
            StopCoroutine(currentBreathingCoroutine);
        }
        currentBreathingCoroutine = StartCoroutine(AudioFading.FadeIn(breathingAudioSource, breathingAudioFadeIn, maxBreathingVolume));
    }

    private void StopBreathing()
    {
        isBreathing = false;
        if (currentBreathingCoroutine != null)
        {
            StopCoroutine(currentBreathingCoroutine);
        }
        currentBreathingCoroutine = StartCoroutine(AudioFading.FadeOut(breathingAudioSource, breathingAudioFadeOut, maxBreathingVolume));
    }

    #region Save and load data

    public void Save(ref PlayerSaveData data)
    {
        data.Position = transform.position;
    }

    public void Load(PlayerSaveData data)
    {
        canMove = false;
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        animator.applyRootMotion = false;

        transform.position = data.Position;

        if (characterController != null)
        {
            characterController.enabled = true;
        }
        animator.applyRootMotion = true;
        canMove = true;
    }

    #endregion
}

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 Position;
}