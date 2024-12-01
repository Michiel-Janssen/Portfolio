using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private bool cursorLocked = true;

    private InputControls playerControls;
    private InteractionManager interactionManager;
    private PlayerController playerController;
    private InputAction movement;
    private InputAction look;

    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isControllable = true;
    private bool isPaused = false;

    private void Awake()
    {
        interactionManager = GetComponent<InteractionManager>();
        playerControls = new InputControls();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        SetCursorState(!cursorLocked);
    }

    private void OnEnable()
    {
        if (isControllable)
        {
            movement = playerControls.Player.Movement;
            movement.Enable();

            look = playerControls.Player.Look;
            look.Enable();

            playerControls.Player.Sprint.performed += SetIsSprinting;
            playerControls.Player.Sprint.canceled += SetIsSprinting;
            playerControls.Player.Sprint.Enable();

            playerControls.Player.Crouch.performed += SetIsCrouching;
            playerControls.Player.Crouch.canceled += SetIsCrouching;
            playerControls.Player.Crouch.Enable();

            playerControls.Player.Interact.performed += HandleInteraction;
            playerControls.Player.Interact.Enable();
        }

        playerControls.Player.Escape.performed += HandleEscape;
        playerControls.Player.Escape.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        look.Disable();

        playerControls.Player.Sprint.Disable();
        playerControls.Player.Crouch.Disable();
        playerControls.Player.Interact.Disable();
        playerControls.Player.Escape.Disable();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        //SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleEscape(InputAction.CallbackContext context)
    {
        /*        if(isPaused)
                {
                    gameManager.UnPause()
        ;       } 
                else
                {
                    gameManager.OnPause();
                }*/
    }

    private void HandleInteraction(InputAction.CallbackContext context)
    {
        if (!isControllable) return;
        interactionManager.HandleInteraction();
    }

    public Vector2 GetMovement()
    {
        if (!isControllable) return Vector2.zero;
        return movement.ReadValue<Vector2>();
    }

    public Vector2 GetLook()
    {
        if (!isControllable) return Vector2.zero;
        return look.ReadValue<Vector2>();
    }

    private void SetIsSprinting(InputAction.CallbackContext context)
    {
        isSprinting = !isSprinting;
    }

    public bool GetIsSprinting()
    {
        return isSprinting;
    }

    private void SetIsCrouching(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
    }

    public bool GetIsCrouching()
    {
        return isCrouching;
    }

    public bool GetIsControllable()
    {
        return isControllable;
    }

    public void SetIsControllable(bool value)
    {
        isControllable = value;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }

    public void SetIsPaused()
    {
        isPaused = !isPaused;
        SetCursorState(isPaused);
    }
}

