using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public static class PlayCacheManager
{
    private static string CacheFolder => Path.Combine(Application.persistentDataPath, "PlaysCache");

    // 🔹 Guardar lista de jugadas (cada una como archivo individual)
    public static void SavePlayToCache(int playId, PlayData playData)
    {
        if (!Directory.Exists(CacheFolder))
            Directory.CreateDirectory(CacheFolder);

        string path = Path.Combine(CacheFolder, $"play_{playId}.json");
        string json = JsonUtility.ToJson(playData, true);
        File.WriteAllText(path, json);
        Debug.Log($"💾 Guardada en caché: {path}");
    }

    // 🔹 Cargar una jugada del cache
    public static bool TryLoadFromCache(int playId, out PlayData playData)
    {
        string path = Path.Combine(CacheFolder, $"play_{playId}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playData = JsonUtility.FromJson<PlayData>(json);
            return true;
        }

        playData = null;
        return false;
    }

    // 🔹 Ver si hay jugadas cacheadas
    public static bool IsPlayCached(int playId)
    {
        string path = Path.Combine(CacheFolder, $"play_{playId}.json");
        return File.Exists(path);
    }

    // 🔹 Limpiar todo el cache (si se quiere forzar recarga)
    public static void ClearCache()
    {
        if (Directory.Exists(CacheFolder))
        {
            Directory.Delete(CacheFolder, true);
            Debug.Log("🧹 Caché de jugadas eliminada.");
        }
    }
}
