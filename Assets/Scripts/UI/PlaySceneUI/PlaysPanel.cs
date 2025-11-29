using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlaysPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private PlayManager playManager;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button refreshButton;

    [Header("Events")]
    public UnityEvent OnPlaySelected;
    public UnityEvent<string> OnError;

    private List<PlaySummary> plays = new List<PlaySummary>();
    private List<GameObject> buttons = new List<GameObject>();
    private bool isLoading = false;

    [SerializeField] public GameObject loadingPanel;
    [SerializeField] public TextMeshProUGUI playtext;

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(Refresh);

        LoadPlays();
    }

    // ------------------------------------------------------------
    // LOAD PLAYS (solo API)
    // ------------------------------------------------------------
    public void LoadPlays()
    {
        Debug.Log("📋 LoadPlays() called");

        if (isLoading)
        {
            Debug.LogWarning("⚠️ Already loading plays...");
            return;
        }

        // 👉 Activar loading ANTES de iniciar la carga
        SetLoading(true);
        loadingPanel.SetActive(true);

        LoadPlaysFromAPI();
    }

    // ------------------------------------------------------------
    // LOAD FROM API
    // ------------------------------------------------------------
    private void LoadPlaysFromAPI()
    {
        Debug.Log("📋 === LoadPlaysFromAPI START ===");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager not found");
            OnError?.Invoke("GameManager not found");
            return;
        }

        int teamId = 3;//GameManager.Instance.GetCurrentTeamId();

        if (teamId <= 0)
        {
            Debug.LogWarning("⚠️ No team selected");
            OnError?.Invoke("No team selected");
            ClearButtons();
            SetLoading(false);
            loadingPanel.SetActive(false);
            return;
        }

        // Limpiar antes de cargar
        ClearButtons();
        plays.Clear();

        Debug.Log($"🚀 Loading plays for team {teamId}...");

        try
        {
            StartCoroutine(PlayService.GetPlaysList(teamId, (playsList) =>
            {
                Debug.Log("📥 API Callback received");

                // 👉 Desactivar loading al terminar
                SetLoading(false);
                loadingPanel.SetActive(false);

                if (playsList == null)
                {
                    Debug.LogError("❌ playsList is NULL");
                    return;
                }

                if (playsList.Count == 0)
                {
                    Debug.Log("📭 No plays found");
                    return;
                }

                plays = playsList;

                // Crear botones si el panel sigue activo
                if (this != null && gameObject != null && gameObject.activeInHierarchy)
                {
                    foreach (var playSummary in plays)
                        AddPlayButton(playSummary);
                }
                else
                {
                    Debug.LogWarning("⚠️ Panel not active. Skipping button creation.");
                }
            }));
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ EXCEPTION: {e.Message}");
        }

        Debug.Log("📋 === LoadPlaysFromAPI END ===");
    }

    // ------------------------------------------------------------
    // ADD BUTTON
    // ------------------------------------------------------------
    private void AddPlayButton(PlaySummary playSummary)
    {
        GameObject button = Instantiate(buttonPrefab, parentTransform);

        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = playSummary.name;

        var btn = button.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
                LoadPlayFromAPI(playSummary.id, playSummary.name));
        }

        buttons.Add(button);
    }

    // ------------------------------------------------------------
    // LOAD PLAY FROM API
    // ------------------------------------------------------------
    private void LoadPlayFromAPI(int playId, string playName)
    {
        if (playManager == null)
        {
            Debug.LogError("❌ PlayManager not assigned");
            OnError?.Invoke("PlayManager not assigned");
            return;
        }

        Debug.Log($"⏳ Loading play: {playName}");

        SetLoading(true);
        loadingPanel.SetActive(true);

        playManager.LoadPlayFromAPI(playId, (success, message) =>
        {
            SetLoading(false);
            loadingPanel.SetActive(false);

            if (success)
            {
                Debug.Log($"✅ Play loaded: {playName}");
                playtext.text = playName;
                PopUp.Instance.Info("Play loaded succesfully");
                OnPlaySelected?.Invoke();

            }
            else
            {
                Debug.LogError($"❌ Failed to load play: {message}");
                OnError?.Invoke(message);

                if (PopUp.Instance != null)
                    PopUp.Instance.Alert($"Error loading play: {message}");
            }
        });
    }

    // ------------------------------------------------------------
    // CLEAR BUTTONS
    // ------------------------------------------------------------
    private void ClearButtons()
    {
        foreach (var button in buttons)
        {
            if (button != null)
                Destroy(button);
        }
        buttons.Clear();
    }

    // ------------------------------------------------------------
    // LOADING STATE
    // ------------------------------------------------------------
    private void SetLoading(bool loading)
    {
        isLoading = loading;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(loading);

        if (refreshButton != null)
            refreshButton.interactable = !loading;
    }

    // ------------------------------------------------------------
    // PUBLIC METHODS
    // ------------------------------------------------------------

    public void Refresh()
    {
        Debug.Log("🔄 Refreshing plays...");
        LoadPlays();
    }

    // ------------------------------------------------------------
    // CLEANUP
    // ------------------------------------------------------------
    private void OnDestroy()
    {
        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(Refresh);

        ClearButtons();
    }
}
