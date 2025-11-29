using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class PlayService
{
    // ------------------------------------------------------------
    // UPLOAD PLAY
    // ------------------------------------------------------------
    public static IEnumerator UploadPlay(int teamId, string playName, PlayData playData, Action<PlayDetail, string> onComplete)
    {
        // Validaciones
        if (playData == null || playData.steps == null || playData.steps.Count == 0)
        {
            onComplete?.Invoke(null, "Empty play data");
            yield break;
        }

        if (teamId <= 0)
        {
            onComplete?.Invoke(null, "Invalid team ID");
            yield break;
        }

        if (string.IsNullOrEmpty(playName))
        {
            onComplete?.Invoke(null, "Play name is required");
            yield break;
        }

        // Preparar request
        string jsonPlayData = JsonUtility.ToJson(playData, true);
        var request = new PlayUploadRequest
        {
            team_id = teamId,
            name = playName,
            data = jsonPlayData
        };

        string body = JsonUtility.ToJson(request, true);

        yield return ApiClient.Post(
            "/plays/",
            body,
            onSuccess: (response) =>
            {
                try
                {
                    PlayDetail detail = JsonUtility.FromJson<PlayDetail>(response);
                    if (detail == null)
                    {
                        onComplete?.Invoke(null, "Failed to parse response");
                        return;
                    }

                    Debug.Log($"✅ Play uploaded: {detail.name} (ID: {detail.id})");
                    onComplete?.Invoke(detail, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"UploadPlay parsing error: {e.Message}");
                    onComplete?.Invoke(null, "Response parsing error");
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "upload play");
                Debug.LogWarning($"Upload play failed: {errorMsg}");
                onComplete?.Invoke(null, errorMsg);
            }
        );
    }

    // Sobrecarga para Play object
    public static IEnumerator UploadPlay(Play play, Action<PlayDetail, string> onComplete)
    {
        if (play == null || !play.IsValid())
        {
            onComplete?.Invoke(null, "Invalid play");
            yield break;
        }

        yield return UploadPlay(play.teamId, play.name, play.ToPlayData(), onComplete);
    }

    // ------------------------------------------------------------
    // GET PLAYS LIST
    // ------------------------------------------------------------
    public static IEnumerator GetPlaysList(int teamId, Action<List<PlaySummary>> onComplete)
    {
        if (teamId <= 0)
        {
            Debug.LogError("Invalid team ID");
            onComplete?.Invoke(new List<PlaySummary>());
            yield break;
        }

        yield return ApiClient.Get(
            $"/plays/{teamId}",
            onSuccess: (response) =>
            {
                try
                {
                    PlaySummary[] plays = JsonHelper.FromJson<PlaySummary>(response);
                    onComplete?.Invoke(new List<PlaySummary>(plays));
                }
                catch (Exception e)
                {
                    Debug.LogError($"GetPlaysList parsing error: {e.Message}");
                    onComplete?.Invoke(new List<PlaySummary>());
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "get plays list");
                Debug.LogError($"Error fetching plays: {errorMsg}");
                onComplete?.Invoke(new List<PlaySummary>());
            }
        );
    }

    // ------------------------------------------------------------
    // GET PLAY DATA
    // ------------------------------------------------------------
    public static IEnumerator GetPlayData(int playId, Action<PlayDetail, string> onComplete)
    {
        if (playId <= 0)
        {
            onComplete?.Invoke(null, "Invalid play ID");
            yield break;
        }

        Debug.Log($"🔵 GetPlayData: Requesting play ID {playId}");

        yield return ApiClient.Get(
            $"/plays/{playId}/data",
            onSuccess: (response) =>
            {
                Debug.Log("🔵 RAW RESPONSE:\n" + response);



                PlayDetail play = JsonUtility.FromJson<PlayDetail>(response);


                if (play == null || play.data == null)
                {
                    onComplete?.Invoke(null, "Invalid play data structure");
                    return;
                }

                onComplete?.Invoke(play, null);

            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "get play data");
                Debug.LogError($"❌ GetPlayData API error: {errorMsg}");
                onComplete?.Invoke(null, errorMsg);
            }
        );
    }

    // Sobrecarga que devuelve Play object
    public static IEnumerator GetPlay(int playId, Action<Play, string> onComplete)
    {
        yield return GetPlayData(playId, (playDetail, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                onComplete?.Invoke(null, error);
                return;
            }

            try
            {
                Play play = Play.FromPlayDetail(playDetail);
                onComplete?.Invoke(play, play != null ? null : "Failed to convert play");
            }
            catch (Exception e)
            {
                Debug.LogError($"GetPlay conversion error: {e.Message}");
                onComplete?.Invoke(null, "Play conversion error");
            }
        });
    }

    // ------------------------------------------------------------
    // UPDATE PLAY
    // ------------------------------------------------------------
    public static IEnumerator UpdatePlay(int playId, string playName, PlayData playData, Action<bool, string> onComplete)
    {
        if (playId <= 0)
        {
            onComplete?.Invoke(false, "Invalid play ID");
            yield break;
        }

        if (playData == null || playData.steps == null || playData.steps.Count == 0)
        {
            onComplete?.Invoke(false, "Empty play data");
            yield break;
        }

        var request = new PlayUpdateRequest
        {
            name = playName,
            data = JsonUtility.ToJson(playData, true)
        };

        yield return ApiClient.Put(
            $"/plays/{playId}",
            JsonUtility.ToJson(request, true),
            onSuccess: (response) =>
            {
                Debug.Log($"✅ Play updated successfully");
                onComplete?.Invoke(true, "Play updated successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "update play");
                Debug.LogWarning($"Update play failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // DELETE PLAY
    // ------------------------------------------------------------
    public static IEnumerator DeletePlay(int playId, Action<bool, string> onComplete)
    {
        if (playId <= 0)
        {
            onComplete?.Invoke(false, "Invalid play ID");
            yield break;
        }

        yield return ApiClient.Delete(
            $"/plays/{playId}",
            onSuccess: (response) =>
            {
                Debug.Log($"✅ Play deleted successfully");
                onComplete?.Invoke(true, "Play deleted successfully");
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "delete play");
                Debug.LogWarning($"Delete play failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // DUPLICATE PLAY
    // ------------------------------------------------------------
    public static IEnumerator DuplicatePlay(int playId, Action<PlayDetail, string> onComplete)
    {
        if (playId <= 0)
        {
            onComplete?.Invoke(null, "Invalid play ID");
            yield break;
        }

        yield return ApiClient.Post(
            $"/plays/{playId}/duplicate",
            "{}",
            onSuccess: (response) =>
            {
                try
                {
                    PlayDetail detail = JsonUtility.FromJson<PlayDetail>(response);
                    if (detail == null)
                    {
                        onComplete?.Invoke(null, "Failed to parse response");
                        return;
                    }

                    Debug.Log($"✅ Play duplicated: {detail.name} (ID: {detail.id})");
                    onComplete?.Invoke(detail, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"DuplicatePlay parsing error: {e.Message}");
                    onComplete?.Invoke(null, "Response parsing error");
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "duplicate play");
                Debug.LogWarning($"Duplicate play failed: {errorMsg}");
                onComplete?.Invoke(null, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // ERROR PARSING
    // ------------------------------------------------------------
    private static string ParseErrorMessage(string errorDetail, string context)
    {
        if (string.IsNullOrEmpty(errorDetail))
            return "Unknown error occurred";

        string lowerError = errorDetail.ToLower();

        // Errores específicos de plays
        if (lowerError.Contains("play") && (lowerError.Contains("not found") || lowerError.Contains("404")))
            return "Play not found";

        if (lowerError.Contains("play") && (lowerError.Contains("already") || lowerError.Contains("exists")))
            return "Play name already exists";

        if (lowerError.Contains("name") && (lowerError.Contains("required") || lowerError.Contains("empty")))
            return "Play name is required";

        if (lowerError.Contains("name") && lowerError.Contains("too short"))
            return "Play name is too short";

        if (lowerError.Contains("name") && lowerError.Contains("too long"))
            return "Play name is too long";

        if (lowerError.Contains("data") && (lowerError.Contains("invalid") || lowerError.Contains("empty")))
            return "Invalid play data";

        if (lowerError.Contains("team") && lowerError.Contains("not found"))
            return "Team not found";

        if (lowerError.Contains("permission") || lowerError.Contains("not allowed"))
            return "You don't have permission for this action";

        // Errores genéricos
        if (lowerError.Contains("401") || lowerError.Contains("unauthorized"))
            return "Unauthorized access";

        if (lowerError.Contains("403") || lowerError.Contains("forbidden"))
            return "Access denied";

        if (lowerError.Contains("404") || lowerError.Contains("not found"))
            return "Resource not found";

        if (lowerError.Contains("422") || lowerError.Contains("validation"))
            return "Invalid input data";

        if (lowerError.Contains("500") || lowerError.Contains("internal server"))
            return "Server error, please try again later";

        if (lowerError.Contains("connection") || lowerError.Contains("timeout") || lowerError.Contains("network"))
            return "Connection error";

        return errorDetail;
    }

    // ------------------------------------------------------------
    // DATA MODELS
    // ------------------------------------------------------------
    [Serializable]
    private class PlayUploadRequest
    {
        public int team_id;
        public string name;
        public string data;
    }

    [Serializable]
    private class PlayUpdateRequest
    {
        public string name;
        public string data;
    }
}

// ================================================================
// ------------------- MODELOS DE RESPUESTA API --------------------
// ================================================================

[Serializable]
public class PlaySummary
{
    public int id;
    public string name;
    public string created_at;

    public override string ToString() => $"{name} (ID: {id})";
}

// ================================================================
// MODELO DE RESPUESTA DE LA API
// ================================================================

[Serializable]
public class PlayDetail
{
    public int id;
    public string name;
    public int team_id;
    public string created_at;
    public PlayData data;

    public override string ToString() => $"Play: {name} (ID: {id}, Team: {team_id}, Steps: {data?.steps?.Count ?? 0})";
}

// ================================================================
// ESTRUCTURA DE PLAYDATA (FORMATO API)
// ================================================================

[Serializable]
public class PlayData
{
    public List<PlayStepData> steps = new List<PlayStepData>();
}

[Serializable]
public class PlayStepData
{
    public float duration;
    public List<PositionData> positions = new List<PositionData>();
    public List<BlockActionData> blockActions = new List<BlockActionData>();
}

[Serializable]
public class PositionData
{
    public int id;
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class BlockActionData
{
    public int id;
    public bool block;
}

// ================================================================
// JSON HELPER
// ================================================================
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrapped = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper?.Items ?? new T[0];
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
