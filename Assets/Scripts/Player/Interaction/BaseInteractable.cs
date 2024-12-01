using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Base Booleans")]
    [SerializeField] private bool isExaminable;

    [Space(10)]

    [Header("Base UI")]
    [SerializeField] private bool useUI;
    [ConditionalHide(nameof(useUI), true)]
    [SerializeField] private GameObject uiContainer;
    [ConditionalHide(nameof(useUI), true)]
    [SerializeField] private Image itemIdentifierIcon;
    [ConditionalHide(nameof(useUI), true)]
    [SerializeField] private Image itemInteractIcon;

    [Space(10)]

    private bool isVisible;

    public abstract void Interact();

    public virtual Transform GetTransform()
    {
        return transform;
    }

    public virtual Transform GetUIContainerTransform()
    {
        if(!useUI) return null;
        return uiContainer.transform;
    }

    public virtual void HideIdentifierUI()
    {
        if(!useUI) return;
        itemIdentifierIcon.enabled = false;
    }

    public virtual void HideInteractUI()
    {
        if (!useUI) return;
        itemInteractIcon.enabled = false;
    }

    public virtual bool IsItemVisible()
    {
        return isVisible;
    }

    public virtual void ShowIdentifierUI()
    {
        if (!useUI) return;
        itemIdentifierIcon.enabled = true;
    }

    public virtual void ShowInteractUI()
    {
        if (!useUI) return;
        itemInteractIcon.enabled = true;
    }

    public virtual bool IsItemExaminable()
    {
        return isExaminable;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }

    public void ShowHint()
    {
        TryGetComponent<HintMessage>(out HintMessage hintMessage);
        if (hintMessage != null)
        {
            hintMessage.ShowHint();
        }
        else
        {
            Debug.Log("No hint component");
        }
    }
}
