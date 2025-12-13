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
        if (playButton != null)
            playButton.onClick.AddListener(PlayCurrentPlay);

        if (stopButton != null)
            stopButton.onClick.AddListener(() =>
            {
                StopRecording();
                playManager.StopPlay();
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
        if (stopButton != null)
            stopButton.interactable = false;

        UpdatePlayText();
    }

    private void ConfigurePermissions()
    {
        if (GameManager.Instance == null)
            return;

        string role = GameManager.Instance.GetCurrentTeamRole();
        bool canRecord = role == "admin" || role == "editor";

        if (!canRecord)
        {
            recordButton.recordBtn.interactable = false;
            stopButton.interactable = false;

            PopUp.Instance?.Info("You only have view permissions. Recording is disabled.");
        }
    }

    // ------------------------------------------------------------
    // UPDATE BUTTON STATES
    // ------------------------------------------------------------
    private void UpdateButtonStates()
    {
        if (playManager == null || recordButton == null) return;

        // STOP Button → solo activo grabando
        if (stopButton != null)
        {
            stopButton.interactable = recordButton.isRecording;
        }

        // RECORD Button → no grabar mientras reproduce y solo si tiene permisos
        if (recordButton.recordBtn != null)
        {
            bool canRecord = !playManager.isPlaying;

            string role = GameManager.Instance?.GetCurrentTeamRole() ?? "viewer";
            if (role != "admin" && role != "editor")
                canRecord = false;

            recordButton.recordBtn.interactable = canRecord;
        }

        // PLAY Button
        if (playButton != null)
            playButton.interactable = playManager.HasPlay();
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

        playManager.StopRecording();

        // Si NO hay jugada válida (<2 steps)
        if (!playManager.HasPlay())
        {
            PopUp.Instance?.Alert("No play recorded. Record at least 2 steps.");

            // FIX ✔ Reset total
            recordButton.isRecording = false;
            playManager.SetCurrentPlay(null);
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";

            return;
        }

        // Si sí hay jugada → preguntar si la guarda
        PopUp.Instance.Confirm(
            message: "Do you want to save the current play?",
            onConfirm: () => ShowSavePlayPopup(),
            onCancel: () =>
            {
                playManager.SetCurrentPlay(null);
                recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "+";
                playText.text = "No Play Loaded";
            }
        );
    }

    // ------------------------------------------------------------
    // SAVE POPUP
    // ------------------------------------------------------------
    private void ShowSavePlayPopup()
    {
        Play currentPlay = playManager.GetCurrentPlay();
        string suggestedName = currentPlay?.name ?? $"Play {System.DateTime.Now:HH:mm}";

        PopUp.Instance.Input(
            title: "Save Play",
            placeholder: "Enter play name...",
            onConfirm: (playName) => SavePlay(playName),
            onCancel: () => playManager.SetCurrentPlay(null),
            defaultValue: suggestedName
        );
    }

    // ------------------------------------------------------------
    // SAVE PLAY
    // ------------------------------------------------------------
    private void SavePlay(string playName)
    {
        if (string.IsNullOrEmpty(playName) || playName.Length < 3)
        {
            PopUp.Instance?.Alert("Play name must be at least 3 characters");
            return;
        }

        if (GameManager.Instance == null)
        {
            PopUp.Instance?.Alert("GameManager not found");
            return;
        }

        int teamId = GameManager.Instance.GetCurrentTeamId();
        if (teamId <= 0)
        {
            PopUp.Instance?.Alert("No team selected");
            return;
        }

        LoadingScreen.Instance?.ShowLoading("Saving play...");

        playManager.SavePlayToAPI(teamId, playName, (success, message) =>
        {
            LoadingScreen.Instance?.HideLoading();

            if (success)
            {
                PopUp.Instance?.Alert($"Play '{playName}' saved successfully!");
                UpdatePlayText();
            }
            else
            {
                PopUp.Instance?.Alert($"Error: {message}");
            }
        });
    }

    // ------------------------------------------------------------
    // PLAY CURRENT PLAY
    // ------------------------------------------------------------
    public void PlayCurrentPlay()
    {
        if (!playManager.HasPlay())
        {
            PopUp.Instance?.Alert("No play loaded. Load or record a play first.");
            return;
        }

        playManager.PlayCurrentPlay();
    }

    // ------------------------------------------------------------
    // UPDATE PLAY TEXT
    // ------------------------------------------------------------
    private void UpdatePlayText()
    {
        if (playText == null || playManager == null)
            return;

        Play currentPlay = playManager.GetCurrentPlay();
        playText.text = currentPlay != null ? currentPlay.name : "No Play Loaded";
    }

    public void OnPlayLoaded()
    {
        UpdatePlayText();
    }

    private void OnDestroy()
    {
        playButton?.onClick.RemoveListener(PlayCurrentPlay);
        stopButton?.onClick.RemoveAllListeners();
    }
}
