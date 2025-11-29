using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class JoinTeamPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField joinInput;
    public UnityEvent<string> OnJoinTeamSucceded, OnJoinTeamFailed;
    public void Join()
    {
        if (joinInput.text.Length != 8)
        {
            PopUp.Instance.Alert("Enter a valid code.");
            return;
        }
        StartCoroutine(TeamService.JoinTeamWithCode(joinInput.text, OnJoinComplete));
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(gameObject.name);
        }
    }
    void OnJoinComplete(bool success, string message)
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.HideLoading();
        }
        if (success)
        {
            OnJoinTeamSucceded.Invoke("Join succeeded!");

        }
        else
        {
            OnJoinTeamFailed.Invoke(message);
        }
    }

}
