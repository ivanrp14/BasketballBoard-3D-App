using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class TeamService
{
    // ------------------------------------------------------------
    // GET MY TEAMS
    // ------------------------------------------------------------
    public static IEnumerator GetMyTeams(Action<List<TeamData>> onComplete)
    {
        yield return ApiClient.Get(
            "/teams/me",
            onSuccess: (response) =>
            {
                try
                {
                    TeamListWrapper wrapper =
                        JsonUtility.FromJson<TeamListWrapper>("{\"teams\":" + response + "}");
                    onComplete?.Invoke(wrapper.teams);
                }
                catch (Exception e)
                {
                    Debug.LogError($"GetMyTeams parsing error: {e.Message}");
                    onComplete?.Invoke(new List<TeamData>());
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "get teams");
                Debug.LogError($"Error fetching teams: {errorMsg}");
                onComplete?.Invoke(new List<TeamData>());
            }
        );
    }

    // ------------------------------------------------------------
    // TEAMS YOU CAN SHARE
    // ------------------------------------------------------------
    public static IEnumerator GetMyShareableTeams(Action<List<TeamData>> onComplete)
    {
        yield return GetMyTeams((teams) =>
        {
            List<TeamData> filtered = teams.FindAll(t => t.role == "admin" || t.role == "editor");
            onComplete?.Invoke(filtered);
        });
    }

    // ------------------------------------------------------------
    // CREATE TEAM
    // ------------------------------------------------------------
    public static IEnumerator CreateTeam(string teamName, string color, Action<bool, string> onComplete)
    {
        string json = $"{{\"name\":\"{teamName}\",\"color\":\"{color}\"}}";
        yield return ApiClient.Post(
            "/teams",
            json,
            onSuccess: (response) =>
            {
                onComplete?.Invoke(true, "Team created successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "create team");
                Debug.LogWarning($"Create team failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // DELETE TEAM
    // ------------------------------------------------------------
    public static IEnumerator DeleteTeam(int teamId, Action<bool, string> onComplete)
    {
        yield return ApiClient.Delete(
            $"/teams/{teamId}",
            onSuccess: (response) =>
            {
                onComplete?.Invoke(true, "Team deleted successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "delete team");
                Debug.LogWarning($"Delete team failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // JOIN TEAM WITH CODE
    // ------------------------------------------------------------
    public static IEnumerator JoinTeamWithCode(string invitationCode, Action<bool, string> onComplete)
    {
        string json = $"{{\"invitation_code\":\"{invitationCode}\"}}";
        yield return ApiClient.Post(
            $"/teams/join/{invitationCode}",
            json,
            onSuccess: (response) =>
            {
                onComplete?.Invoke(true, "Joined team successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "join team");
                Debug.LogWarning($"Join team failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // GET INVITATION CODE
    // ------------------------------------------------------------
    public static IEnumerator GetInvitationCode(int teamId, Action<string, string> onComplete)
    {
        yield return ApiClient.Get(
            $"/teams/{teamId}/invitation-code",
            onSuccess: (response) =>
            {
                try
                {
                    InvitationWrapper w = JsonUtility.FromJson<InvitationWrapper>(response);
                    onComplete?.Invoke(w.invitation_code, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"GetInvitationCode parsing error: {e.Message}");
                    onComplete?.Invoke("", "Response parsing error");
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "get invitation code");
                Debug.LogError($"Error fetching invitation code: {errorMsg}");
                onComplete?.Invoke("", errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // LEAVE TEAM
    // ------------------------------------------------------------
    public static IEnumerator LeaveTeam(int teamId, Action<bool, string> onComplete)
    {
        yield return ApiClient.Post(
            $"/teams/{teamId}/leave",
            "{}",
            onSuccess: (response) =>
            {
                onComplete?.Invoke(true, "Left team successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "leave team");
                Debug.LogWarning($"Leave team failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // ERROR PARSING
    // ------------------------------------------------------------
    private static string ParseErrorMessage(string errorDetail, string context)
    {
        // Si el error está vacío
        if (string.IsNullOrEmpty(errorDetail))
            return "Unknown error occurred";

        // Convertir a minúsculas para comparación
        string lowerError = errorDetail.ToLower();

        // Errores específicos de equipos
        if (lowerError.Contains("team") && (lowerError.Contains("not found") || lowerError.Contains("404")))
            return "Team not found";

        if (lowerError.Contains("team") && (lowerError.Contains("already") || lowerError.Contains("exists")))
            return "Team name already taken";

        if (lowerError.Contains("already") && lowerError.Contains("member"))
            return "Already a member of this team";

        if (lowerError.Contains("invitation") && lowerError.Contains("invalid"))
            return "Invalid invitation code";

        if (lowerError.Contains("invitation") && lowerError.Contains("expired"))
            return "Invitation code expired";

        if (lowerError.Contains("permission") || lowerError.Contains("not allowed") || lowerError.Contains("forbidden"))
            return "You don't have permission for this action";

        if (lowerError.Contains("admin") || lowerError.Contains("owner"))
            return "Only team admins can perform this action";

        if (lowerError.Contains("last") && lowerError.Contains("admin"))
            return "Cannot leave, you are the last admin";

        // Errores de validación
        if (lowerError.Contains("name") && (lowerError.Contains("required") || lowerError.Contains("empty")))
            return "Team name is required";

        if (lowerError.Contains("name") && lowerError.Contains("too short"))
            return "Team name is too short";

        if (lowerError.Contains("name") && lowerError.Contains("too long"))
            return "Team name is too long";

        // Error 401 (no autorizado)
        if (lowerError.Contains("401") || lowerError.Contains("unauthorized"))
            return "Unauthorized access";

        // Error 403 (prohibido)
        if (lowerError.Contains("403") || lowerError.Contains("forbidden"))
            return "Access denied";

        // Error 404 (no encontrado)
        if (lowerError.Contains("404") || lowerError.Contains("not found"))
            return "Resource not found";

        // Error 422 (validación)
        if (lowerError.Contains("422") || lowerError.Contains("validation"))
            return "Invalid input data";

        // Error 500 (servidor)
        if (lowerError.Contains("500") || lowerError.Contains("internal server"))
            return "Server error, please try again later";

        // Error de conexión
        if (lowerError.Contains("connection") || lowerError.Contains("timeout") || lowerError.Contains("network"))
            return "Connection error";

        // Si no coincide con ningún patrón, devolver el error original
        return errorDetail;
    }

    // ------------------------------------------------------------
    // DATA MODELS
    // ------------------------------------------------------------
    [Serializable]
    private class TeamListWrapper
    {
        public List<TeamData> teams;
    }

    [Serializable]
    private class InvitationWrapper
    {
        public string invitation_code;
    }
}