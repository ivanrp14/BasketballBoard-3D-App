using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    public static Action onSceneLoaded;


    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Activar la escena cuando esté cargada al 90%
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}
