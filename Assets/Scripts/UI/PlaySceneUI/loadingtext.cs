using System.Collections;
using TMPro;
using UnityEngine;

public class loadingtext : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    IEnumerator loadText()
    {
        text.text = "Loading.";
        yield return new WaitForSeconds(0.3f)
        ; text.text = "Loading..";
        yield return new WaitForSeconds(0.3f);
        text.text = "Loading...";
        yield return new WaitForSeconds(0.3f);

    }

}
