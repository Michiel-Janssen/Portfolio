using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Serializable]
    public struct GamePanels
    {
        public GameObject HintMessagePanel;
        public GameObject PaperReadPanel;
        public GameObject ExaminePanel;
        public GameObject InventoryPanel;
    }

    [Serializable]
    public struct UserInterface
    {
        public TMP_Text HintMessage;
        public TMP_Text PaperText;
        public TMP_Text ExamineText;
    }

    [Header("UI References")]
    public GamePanels gamePanels = new GamePanels();
    public UserInterface userInterface = new UserInterface();

    private PlayerController playerController;
    public PlayerController PlayerController => playerController;

    private CameraManager cameraManager;

    private bool isPlayerMovementFrozen;

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

    private void Start()
    {
        playerController = PlayerController.Instance;
        cameraManager = CameraManager.Instance;
    }

    public void ShowHint(string hintMessage, float timeThatItShows)
    {
        gamePanels.HintMessagePanel.SetActive(true);
        userInterface.HintMessage.text = hintMessage;
        StartCoroutine(DisablePanelAfterXSeconds(gamePanels.HintMessagePanel, timeThatItShows));
    }

    private IEnumerator DisablePanelAfterXSeconds(GameObject panel, float timeThatItShows)
    {
        yield return new WaitForSeconds(timeThatItShows);
        panel.SetActive(false);
    }

    public void TogglePaperTextUI(string text)
    {
        if (gamePanels.PaperReadPanel.activeSelf)
        {
            HidePaperTextUI();
        }
        else
        {
            ShowPaperTextUI(text);
        }
    }

    public void ShowPaperTextUI(string text)
    {
        gamePanels.PaperReadPanel.SetActive(true);
        userInterface.PaperText.text = text;
    }

    public void HidePaperTextUI()
    {
        gamePanels.PaperReadPanel.SetActive(false);
    }

    public void ShowExamineObjectName(string objectName)
    {
        gamePanels.ExaminePanel.SetActive(true);
        userInterface.ExamineText.text = objectName;
    }

    public void HideExamineObjectName()
    {
        gamePanels.ExaminePanel.SetActive(false);
    }

    public void ToggleInventoryPanel(bool isOpen)
    {
        gamePanels.InventoryPanel.SetActive(isOpen);
    }

    public void FreezePlayerMovement(bool unlockCursor)
    {
        if(isPlayerMovementFrozen) return;
        isPlayerMovementFrozen = true;

        if(unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        playerController.CanMove(false);
        cameraManager.CanMove(false);
    }

    public void UnFreezePlayerMovement()
    {
        if (!isPlayerMovementFrozen) return;
        isPlayerMovementFrozen = false;

        Cursor.lockState = CursorLockMode.Locked;

        playerController.CanMove(true);
        cameraManager.CanMove(true);
    }
}
