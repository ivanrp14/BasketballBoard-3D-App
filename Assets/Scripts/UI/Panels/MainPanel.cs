using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainPanel : MonoBehaviour
{
    [Header("Team Management")]
    [SerializeField] private GameObject teamPanelPrefab;
    [SerializeField] private Transform panelContainer;



    private List<TeamData> teams = new List<TeamData>();
    private bool isLoading = false;

    private void Start()
    {

        RefreshTeams();
    }
    void OnEnable()
    {
        RefreshTeams();
    }

    // ------------------------------------------------------------
    // REFRESH TEAMS
    // ------------------------------------------------------------
    public void RefreshTeams()
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(gameObject.name);
        }
        StartCoroutine(TeamService.GetMyTeams(OnTeamsLoaded));
    }

    private void OnTeamsLoaded(List<TeamData> loadedTeams)
    {


        teams = loadedTeams;
        Debug.Log($"✅ Loaded {teams.Count} teams");
        GameManager.Instance.SetMyTeams(teams);
        // Recrear los paneles de equipos
        CreateTeamPanels();
    }

    // ------------------------------------------------------------
    // CREATE TEAM PANELS
    // ------------------------------------------------------------
    private void CreateTeamPanels()
    {
        // Limpiar paneles existentes
        ClearTeamPanels();

        // Crear un panel por cada equipo
        foreach (TeamData team in teams)
        {
            GameObject teamPanelObj = Instantiate(teamPanelPrefab, panelContainer);
            TeamPanel panel = teamPanelObj.GetComponent<TeamPanel>();

            if (panel != null)
            {
                // Determinar si puede compartir (admin o editor)
                bool canShare = team.role == "admin" || team.role == "editor";

                panel.Init(team.team_name, team.role, team.team_id, team.team_color, canShare, team.owner);
            }
            else
            {
                Debug.LogError("❌ TeamPanel component not found on prefab!");
            }
        }
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.HideLoading();
            Debug.Log("Hidding");
        }
    }

    private void ClearTeamPanels()
    {
        // Destruir todos los paneles hijos
        foreach (Transform child in panelContainer)
        {
            Destroy(child.gameObject);
        }
    }



    // ------------------------------------------------------------
    // LOGOUT
    // ------------------------------------------------------------
    public void Logout()
    {
        GameManager.Instance.Logout();

    }


}