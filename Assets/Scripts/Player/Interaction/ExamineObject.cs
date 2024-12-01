using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType 
{
    Object,
    Paper,
}

public class ExamineObject : BaseInteractable, IExaminable
{
    [SerializeField] private ItemType itemType = ItemType.Object;
    [SerializeField] private string itemName;
    [Multiline, SerializeField] private string paperText;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    public override void Interact()
    {
        //Do nothing
    }

    public void ShowPaperText()
    {
        if (itemType == ItemType.Paper)
        {
            gameManager.TogglePaperTextUI(paperText);
        }
    }

    public ItemType GetItemType()
    {
        return itemType;
    }

    public string GetItemName()
    {
        return itemName;
    }
}
