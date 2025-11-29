using System;
using UnityEngine;

public class UIPanel : MonoBehaviour
{
    private Vector3 originalScale;

    [Header("Animación Show/Hide")]
    public float animTime = 0.2f;

    [Header("Idle Animation")]
    public bool idleAnimation = false;       // ✅ activar animación idle
    public float idleScaleAmount = 1.05f;    // 5% más grande
    public float idleSpeed = 1.2f;
    public bool IsHidden { get; private set; } = true;

    private LTDescr idleTween;               // referencia al tween para cancelarlo
    public Action OnHidden;           // evento para avisar al MenuUI
    private void Awake()
    {
        originalScale = Vector3.one;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;

        LeanTween.scale(gameObject, originalScale, animTime)
                 .setEaseOutBack()
                 .setOnComplete(() =>
                 {
                     StartIdleIfEnabled();
                     IsHidden = false;
                 });  // ✅ iniciar idle
    }

    public void Hide()
    {
        StopIdle();

        // ✅ SOLO INVOCAMOS OnHidden *cuando realmente ha terminado el hide*
        LeanTween.scale(gameObject, Vector3.zero, animTime)
                 .setEaseInBack()
                 .setOnComplete(() =>
                 {
                     gameObject.SetActive(false);
                     OnHidden?.Invoke();
                     IsHidden = true;   // ✅ AHORA SÍ: cuando YA desapareció
                 });
    }

    public void HideInstant()
    {
        StopIdle(); // ✅ detener idle
        gameObject.SetActive(false);
        transform.localScale = originalScale;
    }

    // -----------------------------------------------------------
    //                     IDLE ANIMATION
    // -----------------------------------------------------------

    private void StartIdleIfEnabled()
    {
        if (!idleAnimation) return;

        // idle = loop de escala arriba y abajo
        idleTween = LeanTween.scale(gameObject, originalScale * idleScaleAmount, idleSpeed)
                             .setEaseInOutSine()
                             .setLoopPingPong();
    }

    private void StopIdle()
    {
        if (idleTween != null)
        {
            LeanTween.cancel(gameObject);
            idleTween = null;
        }

        // Restaurar escala al estado base
        transform.localScale = originalScale;
    }
}
