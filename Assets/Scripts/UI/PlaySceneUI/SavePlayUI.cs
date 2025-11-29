using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SavePlayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button updateButton;
    
    [Header("References")]
    [SerializeField] private PlayManager playManager;
    
    [Header("Settings")]
    [SerializeField] private int minNameLength = 3;
    
    [Header("Events")]
    public UnityEvent<string> OnPlaySaved;
    public UnityEvent<string> OnError;

    private void Start()
    {
        // Configurar botones
        if (saveButton != null)
            saveButton.onClick.AddListener(ShowSavePopup);
        
        if (updateButton != null)
            updateButton.onClick.AddListener(UpdatePlay);
    }

    // ------------------------------------------------------------
    // SHOW SAVE POPUP
    // ------------------------------------------------------------
    private void ShowSavePopup()
    {
        // Validar que hay una jugada
        if (playManager == null || !playManager.HasPlay())
        {
            ShowError("No play to save");
            return;
        }

        // Obtener nombre por defecto si la jugada ya tiene nombre
        Play currentPlay = playManager.GetCurrentPlay();
        string defaultName = currentPlay != null ? currentPlay.name : "";

        // Mostrar popup de input
        PopUp.Instance.Input(
            title: "Save Play",
            placeholder: "Enter play name...",
            onConfirm: (playName) => SavePlay(playName),
            onCancel: () => Debug.Log("Save cancelled"),
            defaultValue: defaultName
        );
    }

    // ------------------------------------------------------------
    // SAVE PLAY
    // ------------------------------------------------------------
    private void SavePlay(string playName)
    {
        // Validar nombre
        if (!ValidatePlayName(playName))
            return;

        // Verificar que GameManager existe
        if (GameManager.Instance == null)
        {
            ShowError("GameManager not found");
            return;
        }

        // Obtener team ID actual
        int teamId = GameManager.Instance.GetCurrentTeamId();
        
        if (teamId <= 0)
        {
            ShowError("No team selected");
            return;
        }

        // Deshabilitar botones mientras se guarda
        SetButtonsInteractable(false);

        // Guardar en API
        playManager.SavePlayToAPI(teamId, playName, (success, message) =>
        {
            SetButtonsInteractable(true);
            
            if (success)
            {
                Debug.Log($"✅ Play saved to API: {playName}");
                OnPlaySaved?.Invoke(playName);
                
                if (PopUp.Instance != null)
                {
                    PopUp.Instance.Alert($"Play '{playName}' saved successfully!");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to save play: {message}");
                OnError?.Invoke(message);
                
                if (PopUp.Instance != null)
                {
                    PopUp.Instance.Alert($"Error: {message}");
                }
            }
        });
    }

    // ------------------------------------------------------------
    // UPDATE PLAY
    // ------------------------------------------------------------
    public void UpdatePlay()
    {
        if (playManager == null || !playManager.HasPlay())
        {
            ShowError("No play to update");
            return;
        }

        Play currentPlay = playManager.GetCurrentPlay();
        
        if (currentPlay.id <= 0)
        {
            ShowError("Play is not saved in API yet");
            return;
        }

        // Preguntar si quiere cambiar el nombre
        PopUp.Instance.Confirm(
            message: "Update play with current changes?",
            onConfirm: () => PerformUpdate(),
            onCancel: () => Debug.Log("Update cancelled")
        );
    }

    private void PerformUpdate()
    {
        SetButtonsInteractable(false);

        playManager.UpdatePlayInAPI((success, message) =>
        {
            SetButtonsInteractable(true);
            
            if (success)
            {
                Play currentPlay = playManager.GetCurrentPlay();
                Debug.Log($"✅ Play updated: {currentPlay.name}");
                OnPlaySaved?.Invoke(currentPlay.name);
                
                if (PopUp.Instance != null)
                {
                    PopUp.Instance.Alert("Play updated successfully!");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to update play: {message}");
                OnError?.Invoke(message);
                
                if (PopUp.Instance != null)
                {
                    PopUp.Instance.Alert($"Error: {message}");
                }
            }
        });
    }

    // ------------------------------------------------------------
    // VALIDATION
    // ------------------------------------------------------------
    private bool ValidatePlayName(string playName)
    {
        if (string.IsNullOrEmpty(playName))
        {
            ShowError("Please enter a play name");
            return false;
        }

        if (playName.Length < minNameLength)
        {
            ShowError($"Play name must be at least {minNameLength} characters");
            return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // UI HELPERS
    // ------------------------------------------------------------
    private void ShowError(string message)
    {
        Debug.LogWarning($"⚠️ {message}");
        OnError?.Invoke(message);
        
        if (PopUp.Instance != null)
        {
            PopUp.Instance.Alert(message);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (saveButton != null)
            saveButton.interactable = interactable;
        
        if (updateButton != null)
            updateButton.interactable = interactable;
    }

    // ------------------------------------------------------------
    // CLEANUP
    // ------------------------------------------------------------
    private void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveAllListeners();
        
        if (updateButton != null)
            updateButton.onClick.RemoveAllListeners();
    }
}