using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    // ====== SINGLETON ======
    public static PopUp Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ====== VARIABLES ======
    [Header("Sticky Settings")]
    [SerializeField] private float stickyScale = 1.1f;
    [SerializeField] private float stickyAnimationDuration = 0.5f;
    [SerializeField] private LeanTweenType stickyEase = LeanTweenType.easeOutElastic;

    [Header("Panels")]
    [SerializeField] private GameObject[] panels;
    [SerializeField] private ConfirmationPanel confirmationPanel;
    [SerializeField] private InputPanel inputPanel;

    private GameObject currentPanel;

    // ====== MÉTODOS BÁSICOS ======
    public void Alert(string message) => ShowStickyPanel("Alert", message);

    public void Info(string info) => ShowStickyPanel("Info", info);

    // ====== MÉTODO CONFIRM ======
    public void Confirm(string message, Action onConfirm, Action onCancel = null)
    {
        if (confirmationPanel == null)
        {
            Debug.LogWarning("⚠️ ConfirmationPanel no asignado en PopUp.");
            return;
        }

        // Callback que cierra el panel después de la decisión
        Action<bool> callback = (confirmed) =>
        {
            HideConfirmation();

            if (confirmed)
            {
                onConfirm?.Invoke();
            }
            else
            {
                onCancel?.Invoke();
            }
        };

        // Mostrar y animar
        confirmationPanel.gameObject.SetActive(true);
        confirmationPanel.Show(message, callback);

        AnimateStickyOpen(confirmationPanel.gameObject);
    }

    // ====== MÉTODO INPUT ======
    /// <summary>
    /// Muestra un popup con input de texto
    /// </summary>
    /// <param name="title">Título del popup (ej: "Enter play name")</param>
    /// <param name="placeholder">Placeholder del input (ej: "Play name...")</param>
    /// <param name="onConfirm">Callback con el texto ingresado</param>
    /// <param name="onCancel">Callback si se cancela (opcional)</param>
    /// <param name="defaultValue">Valor por defecto del input (opcional)</param>
    public void Input(string title, string placeholder, Action<string> onConfirm, Action onCancel = null, string defaultValue = "")
    {
        if (inputPanel == null)
        {
            Debug.LogWarning("⚠️ InputPanel no asignado en PopUp.");
            return;
        }

        // Callback que cierra el panel después de la decisión
        Action<bool, string> callback = (confirmed, inputText) =>
        {
            HideInput();

            if (confirmed)
            {
                onConfirm?.Invoke(inputText);
            }
            else
            {
                onCancel?.Invoke();
            }
        };

        // Mostrar y animar
        inputPanel.gameObject.SetActive(true);
        inputPanel.Show(title, placeholder, callback, defaultValue);

        AnimateStickyOpen(inputPanel.gameObject);
    }

    // ====== SHOW STICKY PANEL ======
    private void ShowStickyPanel(string panelName, string text = "")
    {
        GameObject panel = GetPanelByName(panelName);
        if (panel == null) return;

        if (!string.IsNullOrEmpty(text))
        {
            TextMeshProUGUI uiText = panel.GetComponentInChildren<TextMeshProUGUI>();
            if (uiText != null)
                uiText.text = text;
        }

        // Cerrar panel con botón (opcional)
        Button button = panel.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HideStickyPanel(panelName));
        }

        currentPanel = panel;
        AnimateStickyOpen(panel);
    }

    // ====== ANIMACIONES ======
    private void AnimateStickyOpen(GameObject panel)
    {
        panel.SetActive(true);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;

        LeanTween.scale(rect, Vector3.one * stickyScale, stickyAnimationDuration * 0.6f)
            .setEase(stickyEase)
            .setOnComplete(() =>
                LeanTween.scale(rect, Vector3.one, stickyAnimationDuration * 0.4f)
                .setEase(LeanTweenType.easeOutBack)
            );
    }

    private void AnimateStickyClose(GameObject panel, Action onComplete = null)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();

        LeanTween.scale(rect, Vector3.zero, stickyAnimationDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
    }

    // ====== HIDE METHODS ======
    public void HideStickyPanel(string panelName)
    {
        GameObject panel = GetPanelByName(panelName);
        if (panel == null) return;

        AnimateStickyClose(panel);
    }

    public void HideConfirmation()
    {
        if (confirmationPanel != null)
        {
            AnimateStickyClose(confirmationPanel.gameObject);
        }
    }

    public void HideInput()
    {
        if (inputPanel != null)
        {
            AnimateStickyClose(inputPanel.gameObject);
        }
    }

    // ====== UTILIDADES ======
    private GameObject GetPanelByName(string panelName)
    {
        foreach (var panel in panels)
        {
            if (panel.name == panelName)
                return panel;
        }

        Debug.LogWarning("⚠️ Panel no encontrado: " + panelName);
        return null;
    }
}