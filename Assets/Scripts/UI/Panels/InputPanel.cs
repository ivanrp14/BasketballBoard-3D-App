using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action<bool, string> currentCallback;

    private void Start()
    {
        // Configurar botones
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);

        // Confirmar con Enter
        if (inputField != null)
        {
            inputField.onSubmit.AddListener((text) => OnConfirm());
        }
    }

    /// <summary>
    /// Muestra el panel de input
    /// </summary>
    public void Show(string title, string placeholder, Action<bool, string> onDecision, string defaultValue = "")
    {
        currentCallback = onDecision;

        // Configurar título
        if (titleText != null)
        {
            titleText.text = title;
        }

        // Configurar input
        if (inputField != null)
        {
            inputField.text = defaultValue;
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholder;

            // Enfocar el input automáticamente
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    private void OnConfirm()
    {
        string inputText = inputField != null ? inputField.text.Trim() : "";

        // Validar que no esté vacío
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("⚠️ Input is empty");

            // Opcional: mostrar feedback visual
            if (inputField != null)
            {
                // Shake animation o cambiar color temporalmente
                LeanTween.cancel(inputField.gameObject);
                RectTransform rect = inputField.GetComponent<RectTransform>();
                Vector3 originalPos = rect.anchoredPosition;

                LeanTween.moveX(rect, originalPos.x + 10f, 0.05f)
                    .setLoopPingPong(4)
                    .setOnComplete(() => rect.anchoredPosition = originalPos);
            }

            return;
        }

        currentCallback?.Invoke(true, inputText);
        currentCallback = null;
    }

    private void OnCancel()
    {
        currentCallback?.Invoke(false, "");
        currentCallback = null;
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancel);

        if (inputField != null)
            inputField.onSubmit.RemoveAllListeners();
    }
}