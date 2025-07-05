using UnityEngine;
using UnityEngine.EventSystems;

public class HoverPlayAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private Animator animator_self;

    void Start()
    {
        animator_self = GetComponent<Animator>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        animator_self.Play("Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator_self.Play("Idle");
    }
}
