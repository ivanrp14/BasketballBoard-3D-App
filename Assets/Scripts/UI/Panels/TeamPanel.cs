using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI teamName;
    public TextMeshProUGUI teamInfo;
    public Button playTeamButton;
    public Button shareTeamButton;
    public Button deleteTeamButton;
    public Image backgroundImage;

    private string _teamName;
    private string _teamRole;
    private int _teamId;
    private string _owner;
    private string _teamColor;
    private bool _canShare;

    private void Awake()
    {
        // Configurar listeners
        if (playTeamButton != null)
            playTeamButton.onClick.AddListener(Play);

        if (shareTeamButton != null)
            shareTeamButton.onClick.AddListener(ShareCode);

        if (deleteTeamButton != null)
            deleteTeamButton.onClick.AddListener(DeleteTeam);
    }

    // ------------------------------------------------------------
    // INITIALIZE
    // ------------------------------------------------------------
    public void Init(string teamName, string teamRole, int teamId, string teamColor, bool canShare, string owner)
    {
        _teamName = teamName;
        _teamRole = teamRole;
        _teamId = teamId;
        _teamColor = teamColor;
        _canShare = canShare;
        _owner = owner;
        UpdateVisuals();
    }

    // ------------------------------------------------------------
    // UPDATE VISUALS
    // ------------------------------------------------------------
    private void UpdateVisuals()
    {
        // Actualizar texto del nombre
        if (teamName != null)
            teamName.text = _teamName.ToUpper();

        // Actualizar info del rol
        if (teamInfo != null)
        {
            teamInfo.text = _canShare ? $"Role: {_teamRole}" : $"Role: {_teamRole}\nOwned by: {_owner}";
        }

        // Mostrar/ocultar botón de compartir según permisos
        if (shareTeamButton != null)
            shareTeamButton.gameObject.SetActive(_canShare);

        // Solo admins pueden borrar equipos
        if (deleteTeamButton != null)
            deleteTeamButton.gameObject.SetActive(_teamRole == "admin");

        // Aplicar color de equipo si está disponible
        if (backgroundImage != null && !string.IsNullOrEmpty(_teamColor))
        {
            if (ColorUtility.TryParseHtmlString(_teamColor, out Color color))
            {
                backgroundImage.color = color;
            }
        }
    }

    // ------------------------------------------------------------
    // PLAY TEAM
    // ------------------------------------------------------------
    private void Play()
    {
        Debug.Log($"🎮 Playing team: {_teamName} (ID: {_teamId})");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentTeam(_teamId);

            // Mostrar loading
            LoadingScreen.Instance?.ShowLoading("");
            // Cargar escena asincrónicamente
            StartCoroutine(LoadSceneAsync(1));
        }
        else
        {
            Debug.LogError("❌ GameManager not found");
            PopUp.Instance?.Alert("GameManager not found");
        }
    }
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // Comienza carga
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneIndex);

        // Evita que la escena cambie sola hasta que tú lo decidas
        asyncOp.allowSceneActivation = false;

        // Espera mientras carga
        while (!asyncOp.isDone)
        {
            // Cuando llegue al 90%, Unity ya terminó de cargar
            if (asyncOp.progress >= 0.9f)
            {
                // Oculta loading después de terminar la carga previa
                LoadingScreen.Instance?.HideLoading();

                // Activar la escena
                asyncOp.allowSceneActivation = true;
            }

            yield return null;
        }
    }



    // ------------------------------------------------------------
    // SHARE CODE
    // ------------------------------------------------------------
    private void ShareCode()
    {
        if (!_canShare)
        {
            PopUp.Instance?.Alert("You don't have permission to share this team");
            return;
        }

        Debug.Log($"📤 Getting invitation code for team: {_teamName}");

        // Deshabilitar botón mientras se obtiene el código
        if (shareTeamButton != null)
            shareTeamButton.interactable = false;

        StartCoroutine(TeamService.GetInvitationCode(_teamId, OnInvitationCodeReceived));
    }

    private void OnInvitationCodeReceived(string code, string error)
    {
        // Rehabilitar botón
        if (shareTeamButton != null)
            shareTeamButton.interactable = true;

        if (error != null)
        {
            Debug.LogWarning($"❌ Error getting invitation code: {error}");
            PopUp.Instance?.Alert(error);
        }
        else
        {
            Debug.Log($"✅ Invitation code: {code}");

            // Copiar al portapapeles
            GUIUtility.systemCopyBuffer = code;

            // Mostrar código con opción de compartir más detalles
            PopUp.Instance?.Info($"Invitation code copied!\n\n{code}\n\nShare this code with others to invite them to '{_teamName}'.");
        }
    }

    // ------------------------------------------------------------
    // DELETE TEAM
    // ------------------------------------------------------------
    private void DeleteTeam()
    {
        if (_teamRole != "admin")
        {
            PopUp.Instance?.Alert("Only admins can delete teams");
            return;
        }

        // Confirmar antes de borrar (usando el nuevo sistema)
        PopUp.Instance.Confirm(
            message: $"Are you sure you want to delete team '{_teamName}'?\n\nThis action cannot be undone.",
            onConfirm: () => PerformDeleteTeam(),
            onCancel: () => Debug.Log("Delete cancelled")
        );
    }

    private void PerformDeleteTeam()
    {
        Debug.Log($"🗑️ Deleting team: {_teamName}");

        // Deshabilitar botón
        if (deleteTeamButton != null)
            deleteTeamButton.interactable = false;

        // Mostrar loading si existe
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading("Deleting team...");
        }

        StartCoroutine(TeamService.DeleteTeam(_teamId, OnTeamDeleted));
    }

    private void OnTeamDeleted(bool success, string message)
    {
        // Ocultar loading
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.HideLoading();
        }

        // Rehabilitar botón
        if (deleteTeamButton != null)
            deleteTeamButton.interactable = true;

        if (success)
        {
            Debug.Log($"✅ {message}");
            PopUp.Instance?.Alert($"Team '{_teamName}' deleted successfully");

            // Buscar el MainPanel y refrescar la lista
            MainPanel mainPanel = FindObjectOfType<MainPanel>();
            if (mainPanel != null)
            {
                mainPanel.RefreshTeams();
            }
            else
            {
                // Si no hay MainPanel, simplemente destruir este panel
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"❌ {message}");
            PopUp.Instance?.Alert($"Error deleting team: {message}");
        }
    }

    // ------------------------------------------------------------
    // CLEANUP
    // ------------------------------------------------------------
    private void OnDestroy()
    {
        if (playTeamButton != null)
            playTeamButton.onClick.RemoveListener(Play);

        if (shareTeamButton != null)
            shareTeamButton.onClick.RemoveListener(ShareCode);

        if (deleteTeamButton != null)
            deleteTeamButton.onClick.RemoveListener(DeleteTeam);
    }
}