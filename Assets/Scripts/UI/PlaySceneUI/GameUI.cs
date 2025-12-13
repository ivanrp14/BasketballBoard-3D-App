using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{
    [Header("UI Dinámica")]
    [SerializeField] private TextMeshProUGUI buttonsUIText;
    [SerializeField] private UIPanel[] allMenus;

    private Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();

    // Eventos
    public UnityEvent OnStartLogged;
    public UnityEvent OnStartNotLogged;
    public UnityEvent OnStart;

    private void Awake()
    {
        foreach (UIPanel panel in allMenus)
        {
            if (!panels.ContainsKey(panel.name))
                panels.Add(panel.name, panel);
        }

        HideAllMenus();
    }

    private void Start()
    {
        // Evento general
        OnStart?.Invoke();

        // Comprobación de login
        GameManager.Instance.CheckSession(OnSessionChecked);
    }

    private void OnSessionChecked(bool isLogged)
    {
        if (isLogged)
        {
            Debug.Log("GameUI → Usuario logueado ✔️");
            OnStartLogged?.Invoke();
        }
        else
        {
            Debug.Log("GameUI → Usuario NO logueado ❌");
            OnStartNotLogged?.Invoke();
        }
    }

    public void ShowPanel(string name)
    {
        HideAllMenus();

        if (panels.TryGetValue(name, out UIPanel panel))
        {
            panel.Show();
        }
        else
        {
            Debug.LogWarning("Panel no encontrado: " + name);
        }
    }

    public void HideAllMenus()
    {
        foreach (var menu in allMenus)
        {
            if (menu != null)
                menu.HideInstant();
        }
    }
}
