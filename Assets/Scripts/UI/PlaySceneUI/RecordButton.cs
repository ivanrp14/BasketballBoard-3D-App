using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordButton : MonoBehaviour
{
    [Header("UI")]
    public Button recordBtn;
    private TextMeshProUGUI buttonText;

    public bool isRecording = false;
    private int stepCount = 0;

    private PlayManager playManager;

    private void Awake()
    {
        playManager = FindFirstObjectByType<PlayManager>();

        recordBtn = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (recordBtn != null)
            recordBtn.onClick.AddListener(OnRecordClick);
    }

    private void OnRecordClick()
    {
        // 👉 Si NO está grabando o no hay Play cargado → empezar nueva grabación
        if (!isRecording || playManager.GetCurrentPlay() == null)
        {
            isRecording = true;
            stepCount = 0;

            // RESET PLAY
            playManager.SetCurrentPlay(null);
            playManager.StartRecording();

            UpdateStepText();
            return;
        }

        // 👉 Si ya está grabando → agregar step
        stepCount++;
        playManager.RecordStep();

        UpdateStepText();
    }

    private void UpdateStepText()
    {
        if (buttonText != null)
            buttonText.text = playManager.stepValue.ToString();
    }
}
