using UnityEngine;
using System;
using System.Collections;

public static class UserService
{
    // ------------------------------------------------------------
    // LOGIN
    // ------------------------------------------------------------
    public static IEnumerator Login(string username, string password, Action<bool, string> onComplete)
    {
        string json = $"{{\"username\":\"{username}\",\"password\":\"{password}\"}}";
        yield return ApiClient.Post(
            "/auth/login",
            json,
            onSuccess: (response) =>
            {
                try
                {
                    LoginResponse data = JsonUtility.FromJson<LoginResponse>(response);
                    if (string.IsNullOrEmpty(data.access_token))
                    {
                        onComplete?.Invoke(false, "Invalid credentials");
                        return;
                    }
                    PlayerPrefs.SetString("auth_token", data.access_token);
                    PlayerPrefs.Save();
                    onComplete?.Invoke(true, "Login successful");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Login parsing error: {e.Message}");
                    onComplete?.Invoke(false, "Response parsing error");
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "login");
                Debug.LogWarning($"Login failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // REGISTER
    // ------------------------------------------------------------
    public static IEnumerator Register(string email, string username, string password, Action<bool, string> onComplete)
    {
        string json = $"{{\"email\":\"{email}\",\"username\":\"{username}\",\"password\":\"{password}\"}}";
        Debug.Log("Sending json: " + json);
        yield return ApiClient.Post(
            "/auth/register",
            json,
            onSuccess: (response) =>
            {
                try
                {
                    RegisterResponse data = JsonUtility.FromJson<RegisterResponse>(response);
                    if (string.IsNullOrEmpty(data.access_token))
                    {
                        string errorMsg = !string.IsNullOrEmpty(data.message) ? data.message : "Registration error";
                        onComplete?.Invoke(false, errorMsg);
                        return;
                    }
                    PlayerPrefs.SetString("auth_token", data.access_token);
                    PlayerPrefs.Save();
                    onComplete?.Invoke(true, "Registration successful");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Register parsing error: {e.Message}");
                    onComplete?.Invoke(false, "Response parsing error");
                }
            },
            onError: (err) =>
            {
                string errorMsg = ParseErrorMessage(err.detail, "register");
                Debug.LogWarning($"Register failed: {errorMsg}");
                onComplete?.Invoke(false, errorMsg);
            }
        );
    }

    // ------------------------------------------------------------
    // GET USER INFO
    // ------------------------------------------------------------
    public static IEnumerator GetUserInfo(Action<UserData> onComplete)
    {
        yield return ApiClient.Get(
            "/auth/me",
            onSuccess: (response) =>
            {
                try
                {
                    UserData user = JsonUtility.FromJson<UserData>(response);
                    onComplete?.Invoke(user);
                }
                catch (Exception e)
                {
                    Debug.LogError($"GetUserInfo parsing error: {e.Message}");
                    onComplete?.Invoke(null);
                }
            },
            onError: (err) =>
            {
                Debug.LogError($"Error fetching user info: {err.detail}");
                onComplete?.Invoke(null);
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

        // Errores comunes de validación de email
        if (lowerError.Contains("email") && lowerError.Contains("@"))
            return "Invalid email format";

        if (lowerError.Contains("email") && lowerError.Contains("valid"))
            return "Invalid email address";

        // Errores de usuario duplicado
        if (lowerError.Contains("username") && (lowerError.Contains("already") || lowerError.Contains("exists")))
            return "Username already taken";

        if (lowerError.Contains("email") && (lowerError.Contains("already") || lowerError.Contains("exists")))
            return "Email already registered";

        // Errores de credenciales
        if (lowerError.Contains("credential") || lowerError.Contains("password") || lowerError.Contains("incorrect"))
            return "Invalid username or password";

        // Errores de campos requeridos
        if (lowerError.Contains("required") || lowerError.Contains("missing"))
            return "Missing required fields";

        // Errores de longitud
        if (lowerError.Contains("too short") || lowerError.Contains("minimum"))
            return "Field does not meet minimum requirements";

        if (lowerError.Contains("too long") || lowerError.Contains("maximum"))
            return "Field exceeds maximum length";

        // Error 422 genérico (validación)
        if (lowerError.Contains("422") || lowerError.Contains("validation"))
            return "Invalid input data";

        // Error 401 (no autorizado)
        if (lowerError.Contains("401") || lowerError.Contains("unauthorized"))
            return "Invalid credentials";

        // Error 403 (prohibido)
        if (lowerError.Contains("403") || lowerError.Contains("forbidden"))
            return "Access denied";

        // Error 404 (no encontrado)
        if (lowerError.Contains("404") || lowerError.Contains("not found"))
            return "Resource not found";

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
    private class LoginResponse
    {
        public string access_token;
    }

    [Serializable]
    private class RegisterResponse
    {
        public string access_token;
        public string message;
    }
}