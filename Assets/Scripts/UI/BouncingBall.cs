using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    public float speed = 200f;       // velocidad de movimiento
    private Vector2 direction;       // dirección actual
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // Dirección aleatoria
        direction = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        MoveBall();
    }

    void MoveBall()
    {
        // Mover la pelota
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        // Obtener bordes del Canvas
        float halfWidth = rectTransform.rect.width / 2f;
        float halfHeight = rectTransform.rect.height / 2f;

        float left = -canvasRect.rect.width / 2f + halfWidth;
        float right = canvasRect.rect.width / 2f - halfWidth;
        float bottom = -canvasRect.rect.height / 2f + halfHeight;
        float top = canvasRect.rect.height / 2f - halfHeight;

        Vector2 pos = rectTransform.anchoredPosition;

        // Rebote en X
        if (pos.x < left || pos.x > right)
        {
            direction.x *= -1; // invierte dirección
            pos.x = Mathf.Clamp(pos.x, left, right);
        }

        // Rebote en Y
        if (pos.y < bottom || pos.y > top)
        {
            direction.y *= -1; // invierte dirección
            pos.y = Mathf.Clamp(pos.y, bottom, top);
        }

        rectTransform.anchoredPosition = pos;

        // Rotación estética
        rectTransform.Rotate(Vector3.forward * 180f * Time.deltaTime);
    }
}
