using UnityEngine;

public class Players : MonoBehaviour
{
    [SerializeField] private Color teamColor;
    [SerializeField] private Material teamMaterial;
    void Start()
    {
        teamColor = GameManager.Instance.GetCurrentTeamColor();
        ApplyTeamColor(teamColor);
    }
    public void ApplyTeamColor(Color color)
    {
        teamColor = color;
        if (teamMaterial != null)
        {
            teamMaterial.color = teamColor;
        }
    }


}
