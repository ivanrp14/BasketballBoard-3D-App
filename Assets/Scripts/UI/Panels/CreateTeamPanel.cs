using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreateTeamPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField teamNameInput;
    [SerializeField] private List<Button> colorButtons;
    public Color teamColor;
    public UnityEvent<string> OnCreateTeamSucceded, OnCreateTeamFailed;
    
    void Awake()
    {
        foreach (Button button in colorButtons)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                button.onClick.AddListener(() => SetTeamColor(color));
            }
        }
    }
    void SetTeamColor(Color color)
    {
        teamColor = color;
    }
    public void Create()
    {
        if (teamNameInput.text.Length == 0)
        {
            PopUp.Instance.Alert("Insert a team Name");
        }
        StartCoroutine(TeamService.CreateTeam(teamNameInput.text, teamColor.ToString(), OnCreateTeamComplete));
    }
    void OnCreateTeamComplete(bool success, string message)
    {
        if (success)
        {
            OnCreateTeamSucceded.Invoke("Login succeeded!");

        }
        else
        {
            OnCreateTeamFailed.Invoke(message);
        }
    }
}
