using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class RegisterPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private Button registerButton;

    public UnityEvent<string> OnRegisterFailed, OnRegisterSucceded;



    private void Start()
    {
        // Configurar listeners
        registerButton.onClick.AddListener(OnRegisterButtonClicked);

    }

    private void OnRegisterButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string email = emailInput.text.Trim();

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
        if (string.IsNullOrEmpty(email))
        {
            PopUp.Instance.Alert("Insert a valid email");
            return;
        }

        // Deshabilitar botón mientras se procesa
        registerButton.interactable = false;


        // Llamar al servicio de login
        StartCoroutine(UserService.Register(email, username, password, OnRegisterComplete));
    }

    private void OnRegisterComplete(bool success, string message)
    {
        // Rehabilitar botón
        registerButton.interactable = true;

        if (success)
        {
            OnRegisterSucceded.Invoke("Register Succeeded!");

        }
        else
        {
            passwordInput.text = "";
            Debug.Log(message);
            OnRegisterFailed.Invoke(message);
        }
    }






}