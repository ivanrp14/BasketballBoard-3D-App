using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action<bool> callback;

    // Mostrar el panel con mensaje y callback
    public void Show(string message, Action<bool> onDecision)
    {
        messageText.text = message;
        callback = onDecision;

        gameObject.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);

        // Animación sticky
        RectTransform rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        LeanTween.scale(rect, Vector3.one * 1.2f, 0.2f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => LeanTween.scale(rect, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBack));
    }

    private void OnYesClicked()
    {
        callback?.Invoke(true);
        Close();
    }

    private void OnNoClicked()
    {
        callback?.Invoke(false);
        Close();
    }

    public void Close()
    {
        // Animación de cierre tipo "despegar"
        RectTransform rect = GetComponent<RectTransform>();
        LeanTween.scale(rect, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => gameObject.SetActive(false));
    }
}
