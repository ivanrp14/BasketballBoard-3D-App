using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public RectTransform ballPrefab;
    public int ballCount = 2; // cuántas pelotas quieres al inicio
    public RectTransform canvasRect;

    void Start()
    {
        for (int i = 0; i < ballCount; i++)
        {
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        RectTransform ball = Instantiate(ballPrefab, canvasRect);

        // Posición aleatoria dentro del canvas
        float x = Random.Range(-canvasRect.rect.width / 2f, canvasRect.rect.width / 2f);
        float y = Random.Range(-canvasRect.rect.height / 2f, canvasRect.rect.height / 2f);
        ball.anchoredPosition = new Vector2(y, x);
    }
}
