using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Carga una escena mostrando la escena "LoadingIntermedia" primero.
    /// </summary>
    public void LoadSceneWithTransition(int targetScene)
    {
        StartCoroutine(LoadProcess(targetScene));
    }

    private IEnumerator LoadProcess(int sceneToLoad)
    {
        // 1️⃣ Cargar escena intermedia
        yield return SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Single);

        // 2️⃣ Esperar a que Unity haga el primer render REAL
        yield return new WaitForEndOfFrame();

        // 3️⃣ Mostrar la UI del loading SIN que desaparezca
        LoadingScreen.Instance?.ShowLoading("Loading...");

        // 4️⃣ Comenzar a cargar la escena destino
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncOp.allowSceneActivation = false;

        // 5️⃣ Esperar que termine la carga
        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        // (opcional) pequeño delay estético
        yield return new WaitForSeconds(0.2f);


        asyncOp.allowSceneActivation = true;
    }

    public void LoadSceneWithTransition(string targetScene)
    {
        StartCoroutine(LoadProcess(targetScene));
    }

    private IEnumerator LoadProcess(string sceneToLoad)
    {
        // 1️⃣ Cargar escena intermedia
        yield return SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Single);

        // 2️⃣ Esperar a que Unity haga el primer render REAL
        yield return new WaitForEndOfFrame();

        // 3️⃣ Mostrar la UI del loading SIN que desaparezca
        LoadingScreen.Instance?.ShowLoading("Loading...");

        // 4️⃣ Comenzar a cargar la escena destino
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncOp.allowSceneActivation = false;

        // 5️⃣ Esperar que termine la carga
        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        // (opcional) pequeño delay estético
        yield return new WaitForSeconds(0.2f);


        asyncOp.allowSceneActivation = true;
    }
}
