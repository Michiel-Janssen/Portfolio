using UnityEngine;
using UnityEngine.Audio;

public class Laptop : BaseInteractable
{
    [SerializeField] private GameObject laptopUI;

    private bool isOpen;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        if (!isOpen)
        {
            isOpen = true;
            animator.SetBool("open", isOpen);
        }
        else
        {
            laptopUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
