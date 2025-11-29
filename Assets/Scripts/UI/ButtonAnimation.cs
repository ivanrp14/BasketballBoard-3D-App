using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    private Vector3 originalScale;

    [Header("Escala")]
    public float pressedScale = 0.9f;
    public float animSpeed = 0.1f;
    public float bounceSpeed = 0.2f;

    [Header("Hover PC")]
    public bool enableHover = true;
    public float hoverScale = 1.05f;

    [Header("Selección con mando")]
    public float selectedScale = 1.1f;   // tamaño al estar seleccionado con gamepad

    private void Start()
    {
        originalScale = transform.localScale;
    }

    // ---------------------------
    //      CLICK / TOUCH
    // ---------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        LeanTween.scale(gameObject, originalScale * pressedScale, animSpeed).setEaseInOutSine();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LeanTween.scale(gameObject, originalScale, bounceSpeed).setEaseOutBounce();
    }

    // ---------------------------
    //        MOUSE HOVER
    // ---------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enableHover)
            LeanTween.scale(gameObject, originalScale * hoverScale, animSpeed).setEaseOutSine();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enableHover)
            LeanTween.scale(gameObject, originalScale, animSpeed).setEaseOutSine();
    }

    // ---------------------------
    //   GAMEPAD SELECT / DESELECT
    // ---------------------------
    public void OnSelect(BaseEventData eventData)
    {
        LeanTween.scale(gameObject, originalScale * selectedScale, animSpeed).setEaseOutSine();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        LeanTween.scale(gameObject, originalScale, animSpeed).setEaseOutSine();
    }
}
