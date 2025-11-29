using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{


    [Header("UI Dinámica")]
    [SerializeField] private TextMeshProUGUI buttonsUIText;
    [SerializeField]
    private UIPanel[] allMenus;
    private Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();
    public UnityEvent OnStart;
    private void Awake()
    {
        foreach (UIPanel panel in allMenus)
        {
            panels.Add(panel.name, panel);
        }

        HideAllMenus();

    }
    void Start()
    {
        OnStart.Invoke();
    }
    public void ShowPanel(string name)
    {
        HideAllMenus();
        UIPanel panel;
        panels.TryGetValue(name, out panel);
        if (panel != null) panel.Show();
    }

    // -------------------------------------------------------
    //                     MENÚS
    // -------------------------------------------------------



    public void HideAllMenus()
    {
        foreach (var menu in allMenus)
        {
            if (menu != null)
                menu.HideInstant();
        }
    }


}
