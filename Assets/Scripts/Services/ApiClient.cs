using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public static class ApiClient
{
    public static string baseUrl = "https://api.basketballboard3d.shop";

    private static string GetAuthToken()
    {
        return PlayerPrefs.HasKey("auth_token") ? PlayerPrefs.GetString("auth_token") : null;
    }

    // 👉 GET
    public static IEnumerator Get(string endpoint, Action<string> onSuccess, Action<ApiError> onError)
    {
        string fullUrl = baseUrl + endpoint;
        Debug.Log($"📥 GET -> {fullUrl}");

        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            string token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                www.SetRequestHeader("Authorization", "Bearer " + token);
                Debug.Log($"🔑 Auth token present: {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                Debug.LogWarning("⚠️ No auth token found!");
            }

            yield return www.SendWebRequest();

            // 🔍 DEBUG: Información detallada de la respuesta
            Debug.Log($"📊 Response Code: {www.responseCode}");
            Debug.Log($"📊 Result: {www.result}");
            Debug.Log($"📊 Response Length: {www.downloadHandler?.data?.Length ?? 0} bytes");

            if (www.downloadHandler != null && !string.IsNullOrEmpty(www.downloadHandler.text))
            {
                Debug.Log($"📊 Response Body: {www.downloadHandler.text}");
            }

            HandleResponse(www, endpoint, onSuccess, onError);
        }
    }

    // 👉 POST
    public static IEnumerator Post(string endpoint, string jsonBody, Action<string> onSuccess, Action<ApiError> onError)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        string fullUrl = baseUrl + endpoint;
        Debug.Log($"📤 POST -> {fullUrl}");
        Debug.Log($"📤 Body: {jsonBody}");

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            string token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                www.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return www.SendWebRequest();

            Debug.Log($"📊 Response Code: {www.responseCode}");
            Debug.Log($"📊 Response: {www.downloadHandler?.text}");

            HandleResponse(www, endpoint, onSuccess, onError);
        }
    }

    // 👉 PUT
    public static IEnumerator Put(string endpoint, string jsonBody, Action<string> onSuccess, Action<ApiError> onError)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        string fullUrl = baseUrl + endpoint;
        Debug.Log($"✏️ PUT -> {fullUrl}");

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "PUT"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            string token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                www.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return www.SendWebRequest();

            Debug.Log($"📊 Response Code: {www.responseCode}");
            HandleResponse(www, endpoint, onSuccess, onError);
        }
    }

    // 👉 DELETE
    public static IEnumerator Delete(string endpoint, Action<string> onSuccess, Action<ApiError> onError)
    {
        string fullUrl = baseUrl + endpoint;
        Debug.Log($"🗑️ DELETE -> {fullUrl}");

        using (UnityWebRequest www = UnityWebRequest.Delete(fullUrl))
        {
            www.downloadHandler = new DownloadHandlerBuffer(); // ✅ IMPORTANTE para DELETE

            string token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                www.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return www.SendWebRequest();

            Debug.Log($"📊 Response Code: {www.responseCode}");
            HandleResponse(www, endpoint, onSuccess, onError);
        }
    }

    // 👉 Manejo común de respuestas
    private static void HandleResponse(UnityWebRequest www, string endpoint, Action<string> onSuccess, Action<ApiError> onError)
    {
        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;
            Debug.Log($"✅ Request SUCCESS: {www.method} {endpoint}");
            onSuccess?.Invoke(responseText);
        }
        else
        {
            int errorCode = (int)www.responseCode;
            string errorBody = www.downloadHandler.text;

            Debug.LogError($"❌ Request FAILED: {www.method} {endpoint}");
            Debug.LogError($"❌ Error Code: {errorCode}");
            Debug.LogError($"❌ Error Body: {errorBody}");
            Debug.LogError($"❌ Error Type: {www.result}");

            string detail = errorBody;
            if (!string.IsNullOrEmpty(errorBody))
            {
                int index = errorBody.IndexOf('{');
                if (index >= 0) errorBody = errorBody.Substring(index);

                try
                {
                    ErrorDetail parsed = JsonUtility.FromJson<ErrorDetail>(errorBody);
                    if (!string.IsNullOrEmpty(parsed.detail))
                        detail = parsed.detail;
                }
                catch { /* fallback al texto crudo */ }
            }

            ApiError error = new ApiError(errorCode, detail);
            onError?.Invoke(error);
        }
    }

    [Serializable]
    private class ErrorDetail
    {
        public string detail;
    }
}

// Clase pública para manejar errores en toda la app
[Serializable]
public class ApiError
{
    public int code;
    public string detail;

    public ApiError() { }

    public ApiError(int code, string detail)
    {
        this.code = code;
        this.detail = detail;
    }

    public override string ToString()
    {
        return $"Error {code}: {detail}";
    }
}