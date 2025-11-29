using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Play
{
    private List<PlayStep> steps = new List<PlayStep>();

    // Metadatos
    public int id = -1;
    public string name;
    public int teamId = -1;
    public string createdAt;

    public Play() { }

    public void AddStep(PlayStep step)
    {
        if (step != null)
            steps.Add(step);
    }

    public List<PlayStep> GetSteps() => new List<PlayStep>(steps);

    public int GetStepCount() => steps.Count;

    public bool IsValid() => steps != null && steps.Count > 0;

    // ================================================================
    // CONVERSIÓN: Play → PlayData (para enviar a API)
    // ================================================================
    public PlayData ToPlayData()
    {
        PlayData data = new PlayData();

        foreach (var step in steps)
        {
            PlayStepData stepData = new PlayStepData
            {
                duration = step.duration
            };

            foreach (var kvp in step.positions)
            {
                stepData.positions.Add(new PositionData
                {
                    id = kvp.Key,
                    x = kvp.Value.x,
                    y = kvp.Value.y,
                    z = kvp.Value.z
                });
            }

            foreach (var kvp in step.blockActions)
            {
                stepData.blockActions.Add(new BlockActionData
                {
                    id = kvp.Key,
                    block = kvp.Value
                });
            }

            data.steps.Add(stepData);
        }

        return data;
    }

    // ================================================================
    // CONVERSIÓN: PlayData → Play (desde API)
    // ================================================================
    public static Play FromPlayData(PlayData data)
    {
        if (data == null || data.steps == null || data.steps.Count == 0)
        {
            Debug.LogError("❌ PlayData is null or empty");
            return null;
        }

        Play play = new Play();

        foreach (var stepData in data.steps)
        {
            PlayStep step = new PlayStep(stepData.duration);

            if (stepData.positions != null)
            {
                foreach (var pos in stepData.positions)
                {
                    step.positions[pos.id] = pos.ToVector3();
                }
            }

            if (stepData.blockActions != null)
            {
                foreach (var block in stepData.blockActions)
                {
                    step.blockActions[block.id] = block.block;
                }
            }

            play.AddStep(step);
        }

        return play;
    }

    // ================================================================
    // CONVERSIÓN: PlayDetail → Play (desde API)
    // ================================================================
    public static Play FromPlayDetail(PlayDetail detail)
    {
        if (detail == null || detail.data == null)
        {
            Debug.LogError("❌ PlayDetail or data is null");
            return null;
        }

        Play play = FromPlayData(detail.data);

        if (play != null)
        {
            play.id = detail.id;
            play.name = detail.name;
            play.teamId = detail.team_id;
            play.createdAt = detail.created_at;
        }

        return play;
    }

    public override string ToString() =>
        $"Play: {(string.IsNullOrEmpty(name) ? "Unnamed" : name)} | Steps: {steps.Count}";
}

// ================================================================
// ESTRUCTURA INTERNA (Dictionary para uso en Unity)
// ================================================================
[System.Serializable]
public class PlayStep
{
    public Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();
    public Dictionary<int, bool> blockActions = new Dictionary<int, bool>();
    public float duration;

    public PlayStep(float duration)
    {
        this.duration = duration;
    }
}

