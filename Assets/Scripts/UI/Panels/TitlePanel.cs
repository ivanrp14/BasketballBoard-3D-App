using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TitlePanel : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnUserLogged;
    public UnityEvent OnUserNotLogged;

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 5f; // Tiempo máximo de espera por LoadingScreen

    private void Start()
    {
        StartCoroutine(InitializePanel());
    }

    private IEnumerator InitializePanel()
    {
        // Esperar a que LoadingScreen exista (sin bloquear)
        yield return StartCoroutine(WaitForLoadingScreen());

        // Mostrar loading
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(gameObject.name);
        }

        // Verificar sesión
        GameManager.Instance.CheckSession(OnCheckedSession);
    }

    /// <summary>
    /// Espera no bloqueante por LoadingScreen
    /// </summary>
    private IEnumerator WaitForLoadingScreen()
    {
        float elapsedTime = 0f;

        while (LoadingScreen.Instance == null && elapsedTime < maxWaitTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // Espera un frame
        }

        if (LoadingScreen.Instance == null)
        {
            Debug.LogWarning("⚠️ LoadingScreen not found after waiting");
        }
    }

    private void OnCheckedSession(bool isLogged)
    {
        if (isLogged)
        {
            Debug.Log("✅ User is logged in");
            OnUserLogged?.Invoke();
        }
        else
        {
            Debug.Log("ℹ️ User is not logged in");
            OnUserNotLogged?.Invoke();
        }

        // Ocultar loading
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.HideLoading();
        }
    }
}