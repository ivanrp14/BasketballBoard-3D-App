using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera sideCamera;
    public Camera topCamera;
    public Camera ballCamera;

    private Camera[] cameras;
    private int currentIndex = 2;
    public static Camera currentCamera;

    void Start()
    {
        cameras = new Camera[] { sideCamera, topCamera, ballCamera };
        ActivateCamera(currentIndex);
    }

    private void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
        }
        currentCamera = cameras[index];
    }

    public void NextCamera()
    {
        currentIndex = (currentIndex + 1) % cameras.Length;
        ActivateCamera(currentIndex);
    }

    public void PreviousCamera()
    {
        currentIndex = (currentIndex - 1 + cameras.Length) % cameras.Length;
        ActivateCamera(currentIndex);
    }
    public void TopCameraView()
    {
        currentIndex = 1; // Índice de la cámara superior
        ActivateCamera(currentIndex);
    }
    public void SideCameraView()
    {
        currentIndex = 0; // Índice de la cámara lateral
        ActivateCamera(currentIndex);
    }
    public void MainCameraView()
    {
        currentIndex = 2; // Índice de la cámara del balón
        ActivateCamera(currentIndex);
    }
}
