using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private Animator animator;
    private bool isPointerInside;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Safety net: if mouse button is not held, never stay pressed
        if (!Input.GetMouseButton(0))
        {
            animator.SetBool("IsPressed", false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        animator.SetBool("IsHover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        animator.SetBool("IsHover", false);
        animator.SetBool("IsPressed", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("IsPressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator.SetBool("IsPressed", false);
        animator.SetBool("IsHover", isPointerInside);
    }
}
