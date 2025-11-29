using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private RecordButton recordButton;

    [Header("References")]
    [SerializeField] private PlayManager playManager;

    private void Awake()
    {
        // Configurar listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayCurrentPlay);

        if (stopButton != null)
            stopButton.onClick.AddListener(() =>
                {
                    StopRecording();   // si estabas grabando
                    playManager.StopPlay(); // detener reproducción también
                });


    }

    private void Start()
    {
        InitializeUI();
        ConfigurePermissions();
    }

    private void Update()
    {
        UpdateButtonStates();
    }

    // ------------------------------------------------------------
    // INITIALIZATION
    // ------------------------------------------------------------
    private void InitializeUI()
    {
        // Inicializar estado de botones
        if (stopButton != null)
            stopButton.interactable = false;

        // Actualizar texto del play actual
        UpdatePlayText();
    }

    private void ConfigurePermissions()
    {
        // Verificar permisos del usuario
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager not found");
            return;
        }

        string role = GameManager.Instance.GetCurrentTeamRole();

        // Solo admins y editors pueden grabar
        bool canRecord = role == "admin" || role == "editor";

        if (!canRecord)
        {
            if (recordButton != null && recordButton.recordBtn != null)
                recordButton.recordBtn.interactable = false;

            if (stopButton != null)
                stopButton.interactable = false;

            Debug.Log($"ℹ️ Recording disabled for role: {role}");
        }
    }

    // ------------------------------------------------------------
    // UPDATE BUTTON STATES
    // ------------------------------------------------------------
    private void UpdateButtonStates()
    {
        if (playManager == null || recordButton == null) return;

        // Stop button solo activo mientras está grabando
        if (stopButton != null)
        {
            stopButton.interactable = recordButton.isRecording;
            stopButton.interactable = playManager.GetCurrentPlay() != null;
        }

        // Record button deshabilitado mientras reproduce
        if (recordButton.recordBtn != null)
        {
            bool canRecord = !playManager.isPlaying;

            // Solo habilitar si tiene permisos
            string role = GameManager.Instance?.GetCurrentTeamRole() ?? "viewer";
            if (role != "admin" && role != "editor")
            {
                canRecord = false;
            }

            recordButton.recordBtn.interactable = canRecord;
        }

        // Play button deshabilitado mientras graba
        if (playButton != null)
        {
            playButton.interactable = playManager.HasPlay();
        }

    }

    // ------------------------------------------------------------
    // STOP RECORDING
    // ------------------------------------------------------------
    public void StopRecording()
    {
        if (playManager == null)
        {
            Debug.LogError("❌ PlayManager not assigned");
            return;
        }

        // Detener grabación
        playManager.StopRecording();

        // Verificar que hay steps grabados
        if (!playManager.HasPlay())
        {
            Debug.LogWarning("⚠️ No play recorded");
            PopUp.Instance?.Alert("No play recorded. Record at least 2 steps.");
            return;
        }

        // Preguntar si quiere guardar
        PopUp.Instance.Confirm(
            message: "Do you want to save the current play?",
            onConfirm: () => ShowSavePlayPopup(),
            onCancel: () =>
            {
                Debug.Log("Play discarded");
                playManager.SetCurrentPlay(null);
                recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            }
        );
    }

    // ------------------------------------------------------------
    // SHOW SAVE PLAY POPUP
    // ------------------------------------------------------------
    private void ShowSavePlayPopup()
    {
        // Obtener nombre sugerido
        Play currentPlay = playManager.GetCurrentPlay();
        string suggestedName = currentPlay?.name ?? $"Play {System.DateTime.Now:HH:mm}";

        // Mostrar popup de input
        PopUp.Instance.Input(
            title: "Save Play",
            placeholder: "Enter play name...",
            onConfirm: (playName) => SavePlay(playName),
            onCancel: () =>
            {
                Debug.Log("Save cancelled, play discarded");
                playManager.SetCurrentPlay(null);
            },
            defaultValue: suggestedName
        );
    }

    // ------------------------------------------------------------
    // SAVE PLAY
    // ------------------------------------------------------------
    private void SavePlay(string playName)
    {
        // Validar nombre
        if (string.IsNullOrEmpty(playName) || playName.Length < 3)
        {
            PopUp.Instance?.Alert("Play name must be at least 3 characters");
            return;
        }

        // Verificar GameManager
        if (GameManager.Instance == null)
        {
            PopUp.Instance?.Alert("GameManager not found");
            return;
        }

        // Obtener team ID
        int teamId = GameManager.Instance.GetCurrentTeamId();
        if (teamId <= 0)
        {
            PopUp.Instance?.Alert("No team selected");
            return;
        }

        // Mostrar loading
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading("Saving play...");
        }

        // Guardar en API
        playManager.SavePlayToAPI(teamId, playName, (success, message) =>
        {
            // Ocultar loading
            if (LoadingScreen.Instance != null)
            {
                LoadingScreen.Instance.HideLoading();
            }

            if (success)
            {
                Debug.Log($"✅ Play saved: {playName}");
                PopUp.Instance?.Alert($"Play '{playName}' saved successfully!");

                // Actualizar texto del HUD
                UpdatePlayText();
            }
            else
            {
                Debug.LogError($"❌ Save failed: {message}");
                PopUp.Instance?.Alert($"Error: {message}");
            }
        });
    }

    // ------------------------------------------------------------
    // PLAY CURRENT PLAY
    // ------------------------------------------------------------
    public void PlayCurrentPlay()
    {
        if (playManager == null)
        {
            Debug.LogError("❌ PlayManager not assigned");
            return;
        }

        if (!playManager.HasPlay())
        {
            PopUp.Instance?.Alert("No play loaded. Load or record a play first.");
            return;
        }

        Debug.Log("▶️ Playing current play");
        playManager.PlayCurrentPlay();
    }

    // ------------------------------------------------------------
    // UPDATE PLAY TEXT
    // ------------------------------------------------------------
    private void UpdatePlayText()
    {
        if (playText == null || playManager == null) return;

        Play currentPlay = playManager.GetCurrentPlay();

        if (currentPlay != null && !string.IsNullOrEmpty(currentPlay.name))
        {
            playText.text = currentPlay.name;
        }
        else
        {
            playText.text = "No Play Loaded";
        }
    }
    public void SetPlayText(string text)
    {
        playText.text = text;
    }

    /// <summary>
    /// Llamar esto cuando se cargue una nueva jugada
    /// </summary>
    public void OnPlayLoaded()
    {
        UpdatePlayText();
    }

    // ------------------------------------------------------------
    // CLEANUP
    // ------------------------------------------------------------
    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(PlayCurrentPlay);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(StopRecording);
    }
}