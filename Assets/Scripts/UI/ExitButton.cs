using UnityEngine;

public class ExitButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        PopUp.Instance.Confirm(
            "Do you want to exit to the main menu?",
            () =>
            {
                // Acción de salir al menú principal
                SceneLoader.Instance.LoadSceneWithTransition(0);
            },
            () =>
            {
                PopUp.Instance.HideConfirmation();
            }
        );
    }
}
