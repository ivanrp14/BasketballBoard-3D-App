using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayActor))]
public class Draggable : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem dragParticles;

    [Header("Character Settings")]
    [SerializeField] private float visualHeight = 1f;
    [SerializeField] private float groundY = 0f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Movement Limits")]
    [SerializeField] private bool useLimits = true;
    private static Vector2 minBounds = new Vector2(-50f, -30f); // X, Z mínimos
    private static Vector2 maxBounds = new Vector2(0f, 30f);   // X, Z máximos
    [SerializeField] private bool visualizeBounds = true;

    [Header("Mobile Settings")]
    [SerializeField] private float touchSensitivity = 1f;
    [SerializeField] private float minDragDistance = 0.01f; // Distancia mínima para considerar drag

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool dragging = false;
    private Vector3 offset;
    private Camera cam;
    private PlayManager playManager;
    private PlayActor actor;
    private int currentTouchId = -1;
    private Vector2 lastTouchPosition;
    private bool isMobile = false;

    public Action OnDraggingStarted, OnDraggingEnded;

    private void Awake()
    {
        actor = GetComponent<PlayActor>();
        playManager = FindFirstObjectByType<PlayManager>();

        if (dragParticles == null)
            dragParticles = GetComponentInChildren<ParticleSystem>(true);

        if (dragParticles != null)
            dragParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Detectar si es móvil
        isMobile = Application.isMobilePlatform;
        if (debugLogs) Debug.Log($"Platform detected: {(isMobile ? "Mobile" : "Desktop")}");
    }

    private void Update()
    {
        cam = CameraSwitcher.currentCamera;
        if (cam == null || (playManager != null && playManager.isPlaying)) return;

        if (isMobile)
        {
            HandleMobileInput();
        }
        else
        {
            HandleDesktopInput();
        }
    }

    // ------------------------------------------------------------
    // DESKTOP INPUT (Mouse)
    // ------------------------------------------------------------
    private void HandleDesktopInput()
    {
        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        bool pressDown = Mouse.current.leftButton.wasPressedThisFrame;
        bool isHeld = Mouse.current.leftButton.isPressed;
        bool release = Mouse.current.leftButton.wasReleasedThisFrame;

        Ray ray = cam.ScreenPointToRay(screenPos);

        // START DRAG
        if (pressDown)
        {
            if (CheckRaycastHit(ray, out Vector3 targetPivot))
            {
                offset = actor.transform.position - targetPivot;
                dragging = true;
                OnDraggingStarted?.Invoke();
                dragParticles?.Play();
                if (debugLogs) Debug.Log($"🖱️ Desktop drag started");
            }
        }

        // END DRAG
        if (release && dragging)
        {
            EndDrag();
        }

        // DRAG MOVEMENT
        if (isHeld && dragging)
        {
            UpdateDragPosition(ray);
        }
    }

    // ------------------------------------------------------------
    // MOBILE INPUT (Touch)
    // ------------------------------------------------------------
    private void HandleMobileInput()
    {
        if (Touchscreen.current == null) return;

        // Procesar solo el primer toque activo
        foreach (var touch in Touchscreen.current.touches)
        {
            int touchId = touch.touchId.ReadValue();
            bool isPressed = touch.press.isPressed;
            bool justPressed = touch.press.wasPressedThisFrame;
            bool justReleased = touch.press.wasReleasedThisFrame;

            // Si no estamos draggeando y hay un nuevo toque
            if (!dragging && justPressed)
            {
                Vector2 touchPos = touch.position.ReadValue();
                Ray ray = cam.ScreenPointToRay(touchPos);

                if (CheckRaycastHit(ray, out Vector3 targetPivot))
                {
                    currentTouchId = touchId;
                    lastTouchPosition = touchPos;
                    offset = actor.transform.position - targetPivot;
                    dragging = true;
                    OnDraggingStarted?.Invoke();
                    dragParticles?.Play();
                    if (debugLogs) Debug.Log($"📱 Touch drag started (ID: {touchId})");
                    return; // Solo procesamos un toque
                }
            }

            // Si estamos draggeando con este toque
            if (dragging && touchId == currentTouchId)
            {
                if (justReleased)
                {
                    EndDrag();
                    currentTouchId = -1;
                    return;
                }

                if (isPressed)
                {
                    Vector2 touchPos = touch.position.ReadValue();

                    // Verificar que se movió lo suficiente
                    float dragDistance = Vector2.Distance(touchPos, lastTouchPosition);
                    if (dragDistance < minDragDistance)
                        continue;

                    lastTouchPosition = touchPos;
                    Ray ray = cam.ScreenPointToRay(touchPos);
                    UpdateDragPosition(ray);
                    return;
                }
            }
        }
    }

    // ------------------------------------------------------------
    // RAYCAST CHECK
    // ------------------------------------------------------------
    private bool CheckRaycastHit(Ray ray, out Vector3 targetPivot)
    {
        targetPivot = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactMask))
        {
            // Verificar que tocamos este objeto o uno de sus hijos
            if (hit.collider != null &&
                (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
            {
                // Proyectar en el plano del suelo
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 centerClick = ray.GetPoint(enter);
                    targetPivot = centerClick - Vector3.up * (visualHeight * 0.5f);
                    return true;
                }
            }
        }

        return false;
    }

    // ------------------------------------------------------------
    // UPDATE DRAG POSITION
    // ------------------------------------------------------------
    private void UpdateDragPosition(Ray ray)
    {
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 centerClick = ray.GetPoint(enter);
            Vector3 pivotTarget = centerClick - Vector3.up * (visualHeight * 0.5f);
            Vector3 finalPivot = pivotTarget + offset;

            // Aplicar sensibilidad en móvil
            if (isMobile && touchSensitivity != 1f)
            {
                Vector3 currentPos = actor.transform.position;
                Vector3 delta = (finalPivot - currentPos) * touchSensitivity;
                finalPivot = currentPos + delta;
            }

            // Aplicar límites
            if (useLimits)
            {
                finalPivot = ClampToBounds(finalPivot);
            }

            // Mover el actor
            actor.SetPosition(finalPivot);

            if (debugLogs)
                Debug.Log($"Dragging to: {finalPivot}");
        }
    }

    // ------------------------------------------------------------
    // CLAMP TO BOUNDS
    // ------------------------------------------------------------
    private Vector3 ClampToBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.z = Mathf.Clamp(position.z, minBounds.y, maxBounds.y);
        return position;
    }

    // ------------------------------------------------------------
    // END DRAG
    // ------------------------------------------------------------
    private void EndDrag()
    {
        dragging = false;
        dragParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        OnDraggingEnded?.Invoke();
        if (debugLogs) Debug.Log("Drag ended");
    }

    // ------------------------------------------------------------
    // GIZMOS (Visualizar límites)
    // ------------------------------------------------------------
    private void OnDrawGizmos()
    {
        if (!visualizeBounds || !useLimits) return;

        Gizmos.color = Color.yellow;

        // Esquinas del rectángulo de límites
        Vector3 corner1 = new Vector3(minBounds.x, groundY, minBounds.y);
        Vector3 corner2 = new Vector3(maxBounds.x, groundY, minBounds.y);
        Vector3 corner3 = new Vector3(maxBounds.x, groundY, maxBounds.y);
        Vector3 corner4 = new Vector3(minBounds.x, groundY, maxBounds.y);

        // Dibujar rectángulo
        Gizmos.DrawLine(corner1, corner2);
        Gizmos.DrawLine(corner2, corner3);
        Gizmos.DrawLine(corner3, corner4);
        Gizmos.DrawLine(corner4, corner1);

        // Dibujar X en las esquinas
        float crossSize = 0.3f;
        DrawCross(corner1, crossSize);
        DrawCross(corner2, crossSize);
        DrawCross(corner3, crossSize);
        DrawCross(corner4, crossSize);
    }

    private void DrawCross(Vector3 center, float size)
    {
        Gizmos.DrawLine(center + new Vector3(-size, 0, -size), center + new Vector3(size, 0, size));
        Gizmos.DrawLine(center + new Vector3(-size, 0, size), center + new Vector3(size, 0, -size));
    }

    // ------------------------------------------------------------
    // PUBLIC HELPERS
    // ------------------------------------------------------------

    /// <summary>
    /// Establece los límites de movimiento dinámicamente
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useLimits = true;
        if (debugLogs) Debug.Log($"Bounds set: min={min}, max={max}");
    }

    /// <summary>
    /// Obtiene si el objeto está siendo arrastrado
    /// </summary>
    public bool IsDragging() => dragging;

    /// <summary>
    /// Fuerza el fin del drag (útil para interrupciones)
    /// </summary>
    public void ForceStopDrag()
    {
        if (dragging)
        {
            EndDrag();
            currentTouchId = -1;
        }
    }
}