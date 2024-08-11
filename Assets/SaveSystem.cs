using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string progressSave = Application.persistentDataPath + "/currentProgress.json";
    private static string unlockedLevels = Application.persistentDataPath + "/unlockedLevels.json";
    public static void SaveProgress(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress);
        File.WriteAllText(progressSave, json);
    }

    public static GameProgress LoadProgress()
    {
        if (File.Exists(progressSave))
        {
            string json = File.ReadAllText(progressSave);
            return JsonUtility.FromJson<GameProgress>(json);
        }
        else
        {
            // Return a new GameProgress instance with default values if no save file exists
            return new GameProgress();
        }
    }

    public static void UnlockLevel(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress.currentLevel);
        File.WriteAllText(progressSave, json);
    }


}
