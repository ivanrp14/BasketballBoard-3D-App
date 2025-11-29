using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Maneja el almacenamiento local de jugadas en formato JSON.
/// </summary>
/*
public static class PlayStorage
{
    private const string FILE_EXTENSION = ".json";

    // ================================================================
    // ----------------------- GUARDAR ---------------------------------
    // ================================================================

    /// <summary>
    /// Guarda una jugada localmente. 
    /// Devuelve true si ya existe (y NO se guardó), false si se guardó exitosamente.
    /// </summary>
    public static bool SavePlay(string playName, Play play)
    {
        if (play == null || !play.IsValid())
        {
            Debug.LogWarning("No se puede guardar una jugada vacía o inválida.");
            return true;
        }

        if (string.IsNullOrEmpty(playName))
        {
            Debug.LogWarning("Nombre de jugada vacío.");
            return true;
        }

        string path = GetPlayPath(playName);

        if (File.Exists(path))
        {
            Debug.LogWarning($"Ya existe la jugada: {playName}");
            return true; // Ya existe, no guardamos
        }

        SavePlayToFile(path, play);
        return false; // Guardado exitoso
    }

    /// <summary>
    /// Versión legacy que acepta List de PlayStep.
    /// </summary> 
    public static bool SavePlay(string playName, List<PlayStep> steps)
    {
        Play play = new Play(steps);
        play.name = playName;
        return SavePlay(playName, play);
    }

    /// <summary>
    /// Sobreescribe una jugada existente o crea una nueva.
    /// </summary>
    public static void SavePlayOverwrite(string playName, Play play)
    {
        if (play == null || !play.IsValid())
        {
            Debug.LogWarning("No se puede guardar una jugada vacía o inválida.");
            return;
        }

        string path = GetPlayPath(playName);
        SavePlayToFile(path, play);
        Debug.Log($"Jugada guardada/sobrescrita: {playName}");
    }

    /// <summary>
    /// Versión legacy que acepta List de PlayStep.
    /// </summary>
    public static void SavePlayOverwrite(string playName, List<PlayStep> steps)
    {
        Play play = new Play(steps);
        play.name = playName;
        SavePlayOverwrite(playName, play);
    }

    // ================================================================
    // ----------------------- CARGAR ----------------------------------
    // ================================================================

    /// <summary>
    /// Carga una jugada desde archivo local.
    /// Devuelve el objeto Play o null si no existe.
    /// </summary>
    public static Play LoadPlay(string playName)
    {
        string path = GetPlayPath(playName);

        if (!File.Exists(path))
        {
            Debug.LogError($"No existe la jugada: {playName}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            PlayData data = JsonUtility.FromJson<PlayData>(json);

            Play play = Play.FromPlayData(data);

            if (play != null)
            {
                play.name = playName;
            }

            return play;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar jugada {playName}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Versión legacy que devuelve List de PlayStep.
    /// </summary>
    public static List<PlayStep> LoadPlaySteps(string playName)
    {
        Play play = LoadPlay(playName);
        return play?.GetSteps();
    }

    // ================================================================
    // ----------------------- ELIMINAR --------------------------------
    // ================================================================

    /// <summary>
    /// Elimina una jugada del almacenamiento local.
    /// </summary>
    public static bool DeletePlay(string playName)
    {
        string path = GetPlayPath(playName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"No existe la jugada para eliminar: {playName}");
            return false;
        }

        try
        {
            File.Delete(path);
            Debug.Log($"Jugada eliminada: {playName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al eliminar jugada {playName}: {e.Message}");
            return false;
        }
    }

    // ================================================================
    // ----------------------- LISTAR ----------------------------------
    // ================================================================

    /// <summary>
    /// Obtiene la lista de nombres de todas las jugadas guardadas localmente.
    /// </summary>
    public static List<string> GetSavedPlayNames()
    {
        List<string> playNames = new List<string>();
        string directory = Application.persistentDataPath;

        try
        {
            string[] files = Directory.GetFiles(directory, "*" + FILE_EXTENSION);

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                playNames.Add(fileName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al listar jugadas: {e.Message}");
        }

        return playNames;
    }

    /// <summary>
    /// Verifica si existe una jugada con ese nombre.
    /// </summary>
    public static bool PlayExists(string playName)
    {
        string path = GetPlayPath(playName);
        return File.Exists(path);
    }

    // ================================================================
    // ----------------------- UTILIDADES ------------------------------
    // ================================================================

    /// <summary>
    /// Obtiene la ruta completa del archivo de una jugada.
    /// </summary>
    private static string GetPlayPath(string playName)
    {
        return Path.Combine(Application.persistentDataPath, playName + FILE_EXTENSION);
    }

    /// <summary>
    /// Guarda una jugada en el archivo especificado.
    /// </summary>
    private static void SavePlayToFile(string path, Play play)
    {
        try
        {
            PlayData data = play.ToPlayData();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json, Encoding.UTF8);
            Debug.Log($"Jugada guardada en: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar jugada: {e.Message}");
        }
    }

    /// <summary>
    /// Obtiene información sobre el espacio de almacenamiento.
    /// </summary>
    public static string GetStorageInfo()
    {
        List<string> plays = GetSavedPlayNames();
        long totalSize = 0;

        foreach (string playName in plays)
        {
            string path = GetPlayPath(playName);
            FileInfo info = new FileInfo(path);
            totalSize += info.Length;
        }

        return $"Jugadas guardadas: {plays.Count} | Espacio usado: {totalSize / 1024f:F2} KB";
    }
}*/