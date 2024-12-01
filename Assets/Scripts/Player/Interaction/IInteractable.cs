using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact();

    void ShowHint();

    void ShowIdentifierUI();

    void HideIdentifierUI();

    void ShowInteractUI();

    void HideInteractUI();

    bool IsItemVisible();

    bool IsItemExaminable();

    Transform GetTransform();

    Transform GetUIContainerTransform();
}