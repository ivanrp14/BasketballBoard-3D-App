using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [Header("Actors")]
    public List<PlayActor> actors;   // Jugadores + pelota
    public Transform ball;

    [Header("Recording")]
    public float stepDuration = 0.5f;
    private bool isRecording = false;
    public int stepValue;

    [Header("Playback")]
    public bool isPlaying = false;

    [Header("Current Play")]
    private Play currentPlay;

    // ================================================================
    // ----------------------- GRABACIÓN -------------------------------
    // ================================================================

    public void StartRecording()
    {
        currentPlay = new Play();
        stepValue = 0;
        isRecording = true;

        RecordStep();  // primer paso
        Debug.Log("🔴 Start Recording");
    }

    public void StopRecording()
    {
        isRecording = false;

        Debug.Log($"⏹️ Stop Recording. Total steps: {currentPlay.GetStepCount()}");

    }
    public void StopPlay()
    {
        StopAllCoroutines();
        isPlaying = false;
        ResetActors();
    }

    public void Reset()
    {
        ResetActors();
        currentPlay = null;
        stepValue = 0;
        isRecording = false;
    }

    public void RecordStep()
    {

        if (!isRecording) return;

        PlayStep step = new PlayStep(stepDuration);

        foreach (var actor in actors)
        {
            step.positions[actor.id] = actor.transform.position;
            step.blockActions[actor.id] = actor.animator && actor.animator.GetBool("Block");
        }

        currentPlay.AddStep(step);
        stepValue++;

        Debug.Log($"📍 Recorded step {stepValue}");
    }

    // ================================================================
    // ---------------------- REPRODUCCIÓN -----------------------------
    // ================================================================

    public void PlayCurrentPlay()
    {
        if (!isPlaying && HasPlay())
            StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;

        List<PlayStep> steps = currentPlay.GetSteps();

        for (int i = 0; i < steps.Count - 1; i++)
        {
            PlayStep startStep = steps[i];
            PlayStep endStep = steps[i + 1];

            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime / startStep.duration;

                foreach (var actor in actors)
                {
                    Vector3 startPos = startStep.positions[actor.id];
                    Vector3 endPos = endStep.positions[actor.id];
                    bool block = endStep.blockActions[actor.id];

                    PlayInterpolator.LerpActor(actor, startPos, endPos, t, block, ball);
                }

                yield return null;
            }
        }

        foreach (var actor in actors)
        {
            actor.SetMoving(false);
            actor.SetBlock(false);
        }

        isPlaying = false;
    }

    // ================================================================
    // -------------------------- RESET --------------------------------
    // ================================================================

    public void ResetActors()
    {
        foreach (var actor in actors)
            actor.ResetAnimations();
    }

    // ================================================================
    // --------------------- SISTEMA DE GUARDADO -----------------------
    // ================================================================

    /// <summary>
    /// Sube la jugada actual a la API.
    /// </summary>
    public void SavePlayToAPI(int teamId, string playName, System.Action<bool, string> onComplete = null)
    {
        if (currentPlay == null || !HasPlay())
        {
            Debug.LogWarning("⚠️ No hay jugada para subir.");
            onComplete?.Invoke(false, "No play to upload");
            return;
        }

        if (teamId <= 0)
        {
            Debug.LogWarning("⚠️ Team ID inválido.");
            onComplete?.Invoke(false, "Invalid team ID");
            return;
        }

        if (string.IsNullOrEmpty(playName))
        {
            Debug.LogWarning("⚠️ El nombre de la jugada es obligatorio.");
            onComplete?.Invoke(false, "Play name is required");
            return;
        }

        PlayData playData = currentPlay.ToPlayData();

        StartCoroutine(PlayService.UploadPlay(teamId, playName, playData, (detail, error) =>
        {
            if (error == null)
            {
                // Actualizar metadatos de la jugada actual
                currentPlay.id = detail.id;
                currentPlay.name = detail.name;
                currentPlay.teamId = detail.team_id;
                currentPlay.createdAt = detail.created_at;

                Debug.Log($"✅ Play uploaded successfully: {detail.name} (ID: {detail.id})");
                onComplete?.Invoke(true, "Play uploaded successfully");
            }
            else
            {
                Debug.LogError($"❌ Error uploading play: {error}");
                onComplete?.Invoke(false, error);
            }
        }));
    }

    /// <summary>
    /// Carga jugada desde la API.
    /// </summary>
    /// <summary>
    /// Carga jugada desde la API.
    /// </summary>
    // Reemplaza el método LoadPlayFromAPI con este que tiene más debug:

    public void LoadPlayFromAPI(int playId, System.Action<bool, string> onComplete = null)
    {
        if (playId <= 0)
        {
            Debug.LogError("❌ Invalid play ID");
            onComplete?.Invoke(false, "Invalid play ID");
            return;
        }

        // Limpiar estado anterior
        StopAllCoroutines();
        isPlaying = false;
        ResetActors();

        Debug.Log($"📥 Loading play ID: {playId}");

        StartCoroutine(PlayService.GetPlayData(playId, (playDetail, error) =>
        {
            if (error != null)
            {
                Debug.LogError($"❌ Error loading play: {error}");
                onComplete?.Invoke(false, error);
                return;
            }

            // Verificar que data existe
            if (playDetail.data == null || playDetail.data.steps == null || playDetail.data.steps.Count == 0)
            {
                Debug.LogError("❌ Play data is empty");
                onComplete?.Invoke(false, "Empty play data");
                return;
            }

            Debug.Log($"✅ Received {playDetail.data.steps.Count} steps from API");

            // Convertir PlayDetail → Play
            Play loadedPlay = Play.FromPlayDetail(playDetail);
            if (loadedPlay == null)
            {
                Debug.LogError("❌ Failed to convert play data");
                onComplete?.Invoke(false, "Failed to convert play data");

                return;
            }

            currentPlay = loadedPlay;

            Debug.Log($"✅ Play loaded: {playDetail.name} ({currentPlay.GetStepCount()} steps)");
            onComplete?.Invoke(true, "Play loaded successfully");
        }));
    }


    /// <summary>
    /// Actualiza la jugada actual en la API.
    /// </summary>
    public void UpdatePlayInAPI(System.Action<bool, string> onComplete = null)
    {
        if (currentPlay == null || !HasPlay())
        {
            Debug.LogWarning("⚠️ No hay jugada para actualizar.");
            onComplete?.Invoke(false, "No play to update");
            return;
        }

        if (currentPlay.id <= 0)
        {
            Debug.LogWarning("⚠️ La jugada no tiene un ID válido. Usa SavePlayToAPI en su lugar.");
            onComplete?.Invoke(false, "Play has no ID, use SavePlayToAPI instead");
            return;
        }

        PlayData playData = currentPlay.ToPlayData();

        StartCoroutine(PlayService.UpdatePlay(currentPlay.id, currentPlay.name, playData, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ Play updated successfully: {currentPlay.name}");
                onComplete?.Invoke(true, message);
            }
            else
            {
                Debug.LogError($"❌ Error updating play: {message}");
                onComplete?.Invoke(false, message);
            }
        }));
    }

    /// <summary>
    /// Elimina la jugada actual de la API.
    /// </summary>
    public void DeletePlayFromAPI(System.Action<bool, string> onComplete = null)
    {
        if (currentPlay == null)
        {
            Debug.LogWarning("⚠️ No hay jugada para eliminar.");
            onComplete?.Invoke(false, "No play to delete");
            return;
        }

        if (currentPlay.id <= 0)
        {
            Debug.LogWarning("⚠️ La jugada no tiene un ID válido.");
            onComplete?.Invoke(false, "Play has no ID");
            return;
        }

        int playIdToDelete = currentPlay.id;

        StartCoroutine(PlayService.DeletePlay(playIdToDelete, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ Play deleted successfully");

                // Limpiar la jugada actual si era la que se eliminó
                if (currentPlay != null && currentPlay.id == playIdToDelete)
                {
                    currentPlay = null;
                }

                onComplete?.Invoke(true, message);
            }
            else
            {
                Debug.LogError($"❌ Error deleting play: {message}");
                onComplete?.Invoke(false, message);
            }
        }));
    }

    // ================================================================
    // ---------------------- GETTERS ÚTILES ---------------------------
    // ================================================================

    public int GetStepCount()
    {
        return currentPlay?.GetStepCount() ?? 0;
    }

    public bool IsRecording() => isRecording;

    public bool HasPlay() => currentPlay != null && currentPlay.GetStepCount() > 1;

    public Play GetCurrentPlay() => currentPlay;

    public void SetCurrentPlay(Play playInstance)
    {
        currentPlay = playInstance;
        if (playInstance == null)
        {
            stepValue = 0;
        }
        else
        {
            stepValue = playInstance.GetStepCount();
        }
    }
}