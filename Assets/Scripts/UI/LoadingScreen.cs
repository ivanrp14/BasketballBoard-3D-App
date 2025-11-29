using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    public TextMeshProUGUI text;
    public Image image;
    public GameUI gameUI;

    public float rotationSpeed = 180f;

    private Coroutine textCoroutine;
    private string panelToReturn;
    public bool isLoading;
    public GameObject loadingPanel;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional si cambia de escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {


    }

    void Update()
    {
        if (gameObject.activeSelf && image != null)
        {
            image.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    // -----------------------------
    // ✔ FUNCIÓN PÚBLICA PARA MOSTRAR EL LOADING
    // -----------------------------
    public void ShowLoading(string returnToPanel)
    {
        isLoading = true;
        panelToReturn = returnToPanel;
        loadingPanel.SetActive(true);
        if (textCoroutine == null)
            textCoroutine = StartCoroutine(LoadingTextAnimation());
    }

    // -----------------------------
    // ✔ FUNCIÓN PÚBLICA PARA OCULTAR Y VOLVER AL PANEL
    // -----------------------------
    public void HideLoading()
    {
        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
            textCoroutine = null;
        }

        loadingPanel.SetActive(false);

        isLoading = false;
    }

    // -----------------------------
    // ✔ ANIMACIÓN DEL TEXTO
    // -----------------------------
    IEnumerator LoadingTextAnimation()
    {
        string baseText = "Loading";
        while (true)
        {
            text.text = baseText + ".";
            yield return new WaitForSeconds(0.4f);

            text.text = baseText + "..";
            yield return new WaitForSeconds(0.4f);

            text.text = baseText + "...";
            yield return new WaitForSeconds(0.4f);
        }
    }
}
