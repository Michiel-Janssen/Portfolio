using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System.Globalization;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private InputManager inputManager;
    private Animator animator;

    [Header("Camera Parameters")]
    [SerializeField] private float RotationSpeed = 20;
    [SerializeField] private float BottomClamp = -90;
    [SerializeField] private float TopClamp = 90;
    [SerializeField] private GameObject FPSCameraRoot;
    [SerializeField] private CinemachineCamera FirstPersonCamera;
    [SerializeField] private Transform HeadPosition;
    [SerializeField] private float CameraAngleOverride;
    private float CinemachineTargetPitch;
    private float CinemachineTargetYaw;
    private const float threshold = 0.01f;
    private float RotationVelocity;
    private bool hasAnimator;

    private bool canMove = true;

    void Start()
    {
        Instance = this;

        inputManager = GetComponent<InputManager>();
        hasAnimator = TryGetComponent(out animator);
    }

    private void LateUpdate()
    {
        if (!canMove) return;
        FirstPersonCameraControl();
    }

    public void CanMove(bool canMoveBool)
    {
        canMove = canMoveBool;
    }

    private void FirstPersonCameraControl()
    {
        if (!hasAnimator) return;

        var Mouse_X = inputManager.GetLook().x;
        var Mouse_Y = inputManager.GetLook().y;


        float deltaTimeMultiplier = Time.smoothDeltaTime;

        CinemachineTargetPitch -= Mouse_Y * RotationSpeed * deltaTimeMultiplier;
        CinemachineTargetPitch = ClampAngle(CinemachineTargetPitch, BottomClamp, TopClamp);

        RotationVelocity = Mouse_X * RotationSpeed * deltaTimeMultiplier;
        transform.rotation = transform.rotation * Quaternion.Euler(0.0f, RotationVelocity, 0.0f);
        transform.Rotate(Vector3.up * RotationVelocity);

        FPSCameraRoot.transform.localRotation = Quaternion.Euler(CinemachineTargetPitch, 0, 0);
        FPSCameraRoot.transform.position = HeadPosition.position;

        if (!hasAnimator) return;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
