using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordButton : MonoBehaviour
{
    [Header("UI")]
    public Button recordBtn;
    private TextMeshProUGUI buttonText; // El texto que muestra el número de pasos

    public bool isRecording = false;
    private int stepCount = 0;

    private PlayManager playManager;

    private void Awake()
    {
        playManager = FindFirstObjectByType<PlayManager>();


        recordBtn = GetComponent<Button>();
        if (recordBtn != null)
            recordBtn.onClick.AddListener(OnRecordClick);
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

    }

    private void OnRecordClick()
    {
        if (!isRecording || playManager.GetCurrentPlay() == null)
        {
            // Primer click: iniciar grabación
            isRecording = true;
            stepCount = 0;
            playManager.StartRecording();
        }
        else
        {
            // Clicks siguientes: grabar un step
            stepCount++;
            playManager.RecordStep();
        }

        UpdateStepText();
    }

    private void UpdateStepText()
    {
        if (buttonText != null)
            buttonText.text = playManager.stepValue.ToString();
    }
}
