using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // -------------------------------
    // ESTADO GLOBAL
    // -------------------------------
    public UserData CurrentUser { get; private set; }
    public List<TeamData> MyTeams { get; private set; } = new List<TeamData>();
    public TeamData CurrentTeam { get; private set; }
    public Play CurrentPlay { get; set; }

    public Color CurrentTeamColor =>
        CurrentTeam != null ? HexToColor(CurrentTeam.team_color) : Color.white;

    // -------------------------------
    // SINGLETON
    // -------------------------------
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

    // ------------------------------------------------
    // USER MANAGEMENT
    // ------------------------------------------------
    public void SetCurrentUser(UserData user)
    {
        CurrentUser = user;
        Debug.Log($"✅ Current user set: {user?.username}");
    }

    public void ClearCurrentUser()
    {
        CurrentUser = null;
        Debug.Log("🔄 Current user cleared");
    }

    public bool IsLoggedIn()
    {
        return CurrentUser != null;
    }

    public string GetCurrentUsername()
    {
        return CurrentUser?.username ?? "Guest";
    }

    // ------------------------------------------------
    // TEAM MANAGEMENT
    // ------------------------------------------------
    public void SetMyTeams(List<TeamData> teams)
    {
        MyTeams = teams ?? new List<TeamData>();
        Debug.Log($"✅ Teams loaded: {MyTeams.Count}");
    }

    public void SetCurrentTeam(int teamId)
    {
        if (MyTeams == null || MyTeams.Count == 0)
        {
            Debug.LogWarning("⚠️ No teams available");
            return;
        }

        foreach (var t in MyTeams)
        {
            if (t.team_id == teamId)
            {
                CurrentTeam = t;
                Debug.Log($"✅ Team selected: {t.team_name} (ID: {t.team_id})");
                return;
            }
        }

        Debug.LogWarning($"⚠️ Team not found with ID: {teamId}");
    }

    public void SetCurrentTeam(string teamId)
    {
        if (int.TryParse(teamId, out int id))
        {
            SetCurrentTeam(id);
        }
        else
        {
            Debug.LogError($"❌ Invalid team ID format: {teamId}");
        }
    }

    public void ClearCurrentTeam()
    {
        CurrentTeam = null;
        Debug.Log("🔄 Current team cleared");
    }

    public int GetCurrentTeamId()
    {
        if (CurrentTeam != null)
            return CurrentTeam.team_id;

        Debug.LogWarning("⚠️ No team selected");
        return -1;
    }

    public string GetCurrentTeamName()
    {
        return CurrentTeam?.team_name ?? "No Team";
    }

    public string GetCurrentTeamRole()
    {
        return CurrentTeam?.role ?? "none";
    }

    public Color GetCurrentTeamColor()
    {
        return CurrentTeamColor;
    }

    public bool IsCurrentTeamAdmin()
    {
        return CurrentTeam != null && CurrentTeam.role == "admin";
    }

    public bool CanShareCurrentTeam()
    {
        return CurrentTeam != null && (CurrentTeam.role == "admin" || CurrentTeam.role == "editor");
    }

    // ------------------------------------------------
    // SESSION MANAGEMENT
    // ------------------------------------------------

    /// <summary>
    /// Verifica si hay una sesión activa cargando el user info
    /// </summary>
    public void CheckSession(Action<bool> onComplete)
    {
        string token = PlayerPrefs.GetString("auth_token", "");

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("⚠️ No auth token found");
            onComplete?.Invoke(false);
            return;
        }

        StartCoroutine(UserService.GetUserInfo((user) =>
        {
            if (user != null)
            {
                SetCurrentUser(user);
                Debug.Log($"✅ Session valid for user: {user.username}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("⚠️ Invalid session");
                ClearSession();
                onComplete?.Invoke(false);
            }
        }));
    }

    /// <summary>
    /// Cierra la sesión y limpia todos los datos
    /// </summary>
    public void Logout(Action onComplete = null)
    {
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.Save();

        ClearSession();

        Debug.Log("🔐 Logged out successfully");
        onComplete?.Invoke();
    }

    /// <summary>
    /// Limpia todos los datos de sesión
    /// </summary>
    private void ClearSession()
    {
        CurrentUser = null;
        CurrentTeam = null;
        MyTeams.Clear();
    }

    // ------------------------------------------------
    // DATA LOADING HELPERS
    // ------------------------------------------------

    /// <summary>
    /// Carga el usuario actual y lo guarda en GameManager
    /// </summary>
    public void LoadCurrentUser(Action<bool> onComplete)
    {
        StartCoroutine(UserService.GetUserInfo((user) =>
        {
            if (user != null)
            {
                SetCurrentUser(user);
                onComplete?.Invoke(true);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }));
    }

    /// <summary>
    /// Carga los equipos del usuario y los guarda en GameManager
    /// </summary>
    public void LoadMyTeams(Action<bool> onComplete)
    {
        StartCoroutine(TeamService.GetMyTeams((teams) =>
        {
            SetMyTeams(teams);
            onComplete?.Invoke(teams != null && teams.Count > 0);
        }));
    }

    /// <summary>
    /// Carga usuario y equipos en una sola llamada
    /// </summary>
    public void LoadUserAndTeams(Action<bool> onComplete)
    {
        LoadCurrentUser((userSuccess) =>
        {
            if (!userSuccess)
            {
                onComplete?.Invoke(false);
                return;
            }

            LoadMyTeams((teamsSuccess) =>
            {
                onComplete?.Invoke(teamsSuccess);
            });
        });
    }

    // ------------------------------------------------
    // HELPERS
    // ------------------------------------------------
    private Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Color.white;

        if (!hex.StartsWith("#"))
            hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            return c;

        return Color.white;
    }

    // ------------------------------------------------
    // DEBUG INFO
    // ------------------------------------------------
    public void PrintSessionInfo()
    {
        Debug.Log("=== SESSION INFO ===");
        Debug.Log($"User: {CurrentUser?.username ?? "None"}");
        Debug.Log($"Teams: {MyTeams.Count}");
        Debug.Log($"Current Team: {CurrentTeam?.team_name ?? "None"}");
        Debug.Log($"Role: {CurrentTeam?.role ?? "None"}");
        Debug.Log("==================");
    }
}