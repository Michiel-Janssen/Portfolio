using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class InteractionManagerRay : MonoBehaviour
{
    public static InteractionManagerRay Instance { get; private set; }

    [Space(10)]
    [SerializeField] private float showItemIdentifierIconDistance = 4f;
    [SerializeField] private float raycastLength = 2f;

    [Space(10)]

    [Header("Examine")]
    [SerializeField] private GameObject examinePoint;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationThreshold = 5f;
    [SerializeField] private AudioClip examineBeep;
    [SerializeField] private Camera cam;

    private CameraManager cameraManager;
    private PlayerController playerController;
    private InputManager inputManager;
    private GameManager gameManager;
    private AudioSource audioSource;

    private List<IInteractable> interactableList = new List<IInteractable>();
    private List<IInteractable> previouslyVisibleInteractables = new List<IInteractable>();

    private bool isExamining = false; 
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private IInteractable currentExamineObject;
    private ExamineObject examineObject;

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

        UpdateUIForInteractables();

        previouslyVisibleInteractables = interactableList;

        if (isExamining && Input.GetMouseButton(0))
        {
            RotateObject();
        }

        if (isExamining && Input.GetMouseButtonDown(1))
        {
            StopExamine();
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

    private void RotateObject()
    {
        if (currentExamineObject == null) return;

        Transform currentExamineObjectTransform = currentExamineObject.GetTransform();

        Vector2 look = inputManager.GetLook();

        float rotationX = look.x * rotationSpeed * Time.deltaTime;
        float rotationY = look.y * rotationSpeed * Time.deltaTime;

        currentExamineObjectTransform.Rotate(Vector3.up, -rotationX, Space.Self);
        currentExamineObjectTransform.Rotate(Vector3.right, rotationY, Space.Self);

        if (!objectNameShown && Quaternion.Angle(initialRotation, currentExamineObjectTransform.rotation) >= rotationThreshold)
        {
            objectNameShown = true;
            if (currentExamineObject is IExaminable examinable)
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

        gameManager.HidePaperTextUI();
        gameManager.HideExamineObjectName();

        isExamining = false;

        gameManager.UnFreezePlayerMovement();
        currentExamineObject.ShowIdentifierUI();

        currentExamineObject = null;
    }

    public void HandleInteraction()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastLength))
        {
            if(hit.transform.TryGetComponent(out IInteractable interactable))
            {
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
        }
    }

    private void StartExamine(IInteractable interactable)
    {
        interactable.HideIdentifierUI();
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

    public List<IInteractable> ShowItemIdentifierIcon()
    {
        List<IInteractable> interactableList = new List<IInteractable>();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, showItemIdentifierIconDistance);
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
            if (previousInteractable != null)
            {
                if (!interactableList.Contains(previousInteractable))
                {
                    previousInteractable.HideIdentifierUI();
                }
            }
        }
    }

    private void UpdateUIForInteractables()
    {
        if (isExamining) return;
        foreach (IInteractable interactable in interactableList)
        {
            if (interactable != null)
            {
                if (interactable.IsItemVisible())
                {
                    interactable.ShowIdentifierUI();

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
}
