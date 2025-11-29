using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoginPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;

    public UnityEvent<string> OnLoginFailed, OnLoginSucceded;
    string username;


    private void Start()
    {
        // Configurar listeners
        loginButton.onClick.AddListener(OnLoginButtonClicked);

    }

    private void OnLoginButtonClicked()
    {
        username = usernameInput.text.Trim();
        string password = passwordInput.text;

        // Validaciones
        if (string.IsNullOrEmpty(username))
        {
            PopUp.Instance.Alert("Insert a username!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            PopUp.Instance.Alert("Insert a password");
            return;
        }

        // Deshabilitar botón mientras se procesa
        loginButton.interactable = false;


        // Llamar al servicio de login
        StartCoroutine(UserService.Login(username, password, OnLoginComplete));
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(gameObject.name);
        }
    }

    private void OnLoginComplete(bool success, string message)
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.HideLoading();
        }
        // Rehabilitar botón
        loginButton.interactable = true;

        if (success)
        {
            OnLoginSucceded.Invoke(message);

        }
        else
        {
            passwordInput.text = "";
            OnLoginFailed.Invoke(message);
        }
    }






}