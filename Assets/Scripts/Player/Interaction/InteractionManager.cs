using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening.Core.Easing;


public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [SerializeField] private float ShowItemIdentifierIconDistance = 4f;
    [SerializeField] private float ShowItemInteractIconDistance = 2f;

    [Space(10)]

    [Header("Examine")]
    [SerializeField] private GameObject examinePoint;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationThreshold = 5f;
    [SerializeField] private AudioClip examineBeep;

    private CameraManager cameraManager;
    private PlayerController playerController;
    private InputManager inputManager;
    private AudioSource audioSource;

    private List<IInteractable> interactableList = new List<IInteractable>();
    private List<IInteractable> previouslyVisibleInteractables = new List<IInteractable>();

    private bool isExamining = false;
    private bool isReading = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private IInteractable currentExamineObject;
    private ExamineObject examineObject;
    private GameManager gameManager;

    private Quaternion initialRotation;
    private bool objectNameShown = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        cameraManager = GetComponent<CameraManager>();
        playerController = GetComponent<PlayerController>();
        inputManager = GetComponent<InputManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        interactableList = ShowItemIdentifierIcon();

        HideUINotVisibleAnymore(interactableList);

        if (interactableList.Count == 0) return;

        IInteractable closestInteractable = GetClosestInteractable();

        UpdateUIForInteractables(closestInteractable);

        previouslyVisibleInteractables = interactableList;

        if (isExamining && Input.GetMouseButton(0) && !isReading)
        {
            RotateObject();
        }

        if (isExamining && Input.GetMouseButtonDown(1))
        {
            StopExamine();
        }

        if (isExamining && Input.GetKeyDown(KeyCode.Space))
        {
            if (currentExamineObject is ExamineObject examineObject)
            {
                if (examineObject.GetItemType() == ItemType.Paper)
                {
                    isReading = !isReading;
                    gameManager.HideExamineObjectName();
                    examineObject.ShowPaperText();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (currentExamineObject == null || !isExamining) return;
        Transform examineObjectTransform = currentExamineObject.GetTransform();
        if (examineObjectTransform != null)
        {
            examineObjectTransform.position = Vector3.Lerp(
                examineObjectTransform.position,
                examinePoint.transform.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    public List<IInteractable> ShowItemIdentifierIcon()
    {
        List<IInteractable> interactableList = new List<IInteractable>();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, ShowItemIdentifierIconDistance);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                interactableList.Add(interactable);
            }
        }
        return interactableList;
    }

    private void HideUINotVisibleAnymore(List<IInteractable> interactableList)
    {
        foreach (IInteractable previousInteractable in previouslyVisibleInteractables)
        {
            if(previousInteractable != null)
            {
                if (!interactableList.Contains(previousInteractable))
                {
                    previousInteractable.HideIdentifierUI();
                }
            }
        }
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (IInteractable interactable in interactableList)
        {
            if (interactable.IsItemVisible() && interactable != null)
            {
                float distance = Vector3.Distance(interactable.GetTransform().position, transform.position);
                if (distance <= ShowItemInteractIconDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        return closestInteractable;
    }

    private void UpdateUIForInteractables(IInteractable closestInteractable)
    {
        if (isExamining) return;
        foreach (IInteractable interactable in interactableList)
        {
            if (interactable != null)
            {
                if (interactable.IsItemVisible())
                {
                    if (interactable == closestInteractable)
                    {
                        interactable.HideIdentifierUI();
                        interactable.ShowInteractUI();
                    }
                    else
                    {
                        interactable.HideInteractUI();
                        interactable.ShowIdentifierUI();
                    }

                    Transform uiTransform = interactable.GetUIContainerTransform();
                    if (uiTransform != null)
                    {
                        uiTransform.LookAt(transform.position + new Vector3(0, 1.65f, 0));
                    }
                }
                else
                {
                    interactable.HideIdentifierUI();
                }
            }
        }
    }

    public List<IInteractable> GetInteractableList()
    {
        return interactableList;
    }

    public void HandleInteraction()
    {
        IInteractable interactable = GetClosestInteractable();
        if (interactable == null) return;
        if (interactable.IsItemExaminable() && !isExamining)
        {
            StartExamine(interactable);
        }
        else
        {
            if (isExamining && interactable == currentExamineObject)
            {
                StopExamine();
            }
            interactable.Interact();
        }
    }

    private void StartExamine(IInteractable interactable)
    {
        interactable.HideInteractUI();
        gameManager.FreezePlayerMovement(false);

        currentExamineObject = interactable;
        Transform currentExamineObjectTransform = currentExamineObject.GetTransform();

        originalPosition = currentExamineObjectTransform.position;
        originalRotation = currentExamineObjectTransform.rotation;
        originalParent = currentExamineObjectTransform.parent;

        initialRotation = currentExamineObjectTransform.rotation;
        objectNameShown = false;

        currentExamineObjectTransform.SetParent(examinePoint.transform);
        isExamining = true;
    }

    private void RotateObject()
    {
        if (currentExamineObject == null) return;

        Transform currentExamineObjectTransform = currentExamineObject.GetTransform();

        Vector2 look = inputManager.GetLook();

        float rotationX = look.x * rotationSpeed * Time.deltaTime;
        float rotationY = look.y * rotationSpeed * Time.deltaTime;

        currentExamineObjectTransform.Rotate(Vector3.up, -rotationX, Space.World);
        currentExamineObjectTransform.Rotate(Vector3.right, rotationY, Space.World);

        if (!objectNameShown && Quaternion.Angle(initialRotation, currentExamineObjectTransform.rotation) >= rotationThreshold)
        {
            objectNameShown = true;
            if(currentExamineObject is IExaminable examinable)
            {
                ShowObjectName(examinable.GetItemName());
            }
        }
    }

    private void ShowObjectName(string objectName)
    {
        audioSource.PlayOneShot(examineBeep);
        gameManager.ShowExamineObjectName(objectName);
    }

    private void StopExamine()
    {
        if (currentExamineObject == null) return;

        Transform currentExamineObjectTransform = currentExamineObject.GetTransform();

        currentExamineObjectTransform.SetParent(originalParent);
        currentExamineObjectTransform.position = originalPosition;
        currentExamineObjectTransform.rotation = originalRotation;

        isReading = false;
        gameManager.HidePaperTextUI();
        gameManager.HideExamineObjectName();

        isExamining = false;

        gameManager.UnFreezePlayerMovement();
        currentExamineObject.ShowInteractUI();

        currentExamineObject = null;
    }
}
